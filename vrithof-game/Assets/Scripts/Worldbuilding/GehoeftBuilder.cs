using System.Collections.Generic;
using UnityEngine;

namespace Vrithof.Worldbuilding
{
    /// Autoren-Werkzeug (kein Laufzeit-System): baut ein Gehoeft-Blockout aus
    /// Unity-Wuerfeln. Auf ein leeres GameObject legen, im Kontextmenue des
    /// Components "Gehoeft bauen" klicken. Alle Masse im Inspector.
    ///
    /// Wichtig: das GameObject wird zur Wurzel des Gehoefts. "Bauen" loescht
    /// zuerst alle Kinder und baut neu -> darum ein eigenes leeres Objekt nehmen.
    [DisallowMultipleComponent]
    public class GehoeftBuilder : MonoBehaviour
    {
        [Header("Grundriss (Meter)")]
        public float breite = 8f;    // X-Achse
        public float tiefe = 6f;     // Z-Achse
        public float wandHoehe = 3f;
        public float wandDicke = 0.3f;

        [Header("Tuer (Sued-Wand)")]
        public float tuerBreite = 1.2f;
        public float tuerHoehe = 2.2f;

        [Header("Fenster (Ost- & West-Wand)")]
        public float fensterBreite = 1.2f;
        public float fensterHoehe = 1.2f;
        public float fensterBruestung = 1.2f;   // Wandstueck unter dem Fenster

        [Header("Boden & Dach")]
        public bool bodenBauen = true;
        public bool dachBauen = true;
        public float bodenDicke = 0.2f;
        public float dachDicke = 0.2f;

        [Header("Material (optional, sonst Unity-Standard)")]
        public Material wandMaterial;
        public Material bodenMaterial;
        public Material dachMaterial;

        // Eine Oeffnung in Wand-lokalen Koordinaten.
        struct Oeffnung
        {
            public float u;          // Mitte entlang der Wandlaenge (lokale X)
            public float breite;
            public float bruestung;  // Hoehe des Wandstuecks darunter (0 = bis Boden)
            public float hoehe;
            public OpeningKind art;
        }

        [ContextMenu("Gehoeft bauen")]
        public void Bauen()
        {
            Loeschen();

            if (bodenBauen)
                Quader("Boden", new Vector3(0, -bodenDicke * 0.5f, 0),
                       new Vector3(breite, bodenDicke, tiefe), bodenMaterial);

            if (dachBauen)
                Quader("Dach", new Vector3(0, wandHoehe + dachDicke * 0.5f, 0),
                       new Vector3(breite, dachDicke, tiefe), dachMaterial);

            // Sued (vorne, -Z): Tuer
            WandBauen("Wand_Sued", new Vector3(0, 0, -tiefe * 0.5f), 0f, breite,
                new List<Oeffnung> {
                    new Oeffnung { u = 0, breite = tuerBreite, bruestung = 0,
                                   hoehe = tuerHoehe, art = OpeningKind.Tuer } });

            // Nord (hinten, +Z): massiv
            WandBauen("Wand_Nord", new Vector3(0, 0, tiefe * 0.5f), 0f, breite,
                new List<Oeffnung>());

            // West (-X): Fenster
            WandBauen("Wand_West", new Vector3(-breite * 0.5f, 0, 0), 90f, tiefe,
                new List<Oeffnung> {
                    new Oeffnung { u = 0, breite = fensterBreite, bruestung = fensterBruestung,
                                   hoehe = fensterHoehe, art = OpeningKind.Fenster } });

            // Ost (+X): Fenster
            WandBauen("Wand_Ost", new Vector3(breite * 0.5f, 0, 0), 90f, tiefe,
                new List<Oeffnung> {
                    new Oeffnung { u = 0, breite = fensterBreite, bruestung = fensterBruestung,
                                   hoehe = fensterHoehe, art = OpeningKind.Fenster } });
        }

        [ContextMenu("Gehoeft loeschen")]
        public void Loeschen()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // Baut eine Wand entlang der lokalen X-Achse (zentriert), Dicke entlang lokaler Z.
        // Oeffnungen werden als echte Luecken freigelassen und mit Cube-Segmenten gerahmt.
        void WandBauen(string name, Vector3 mitte, float drehungY, float laenge, List<Oeffnung> oeffnungen)
        {
            var wand = new GameObject(name);
            wand.transform.SetParent(transform, false);
            wand.transform.localPosition = mitte;
            wand.transform.localRotation = Quaternion.Euler(0, drehungY, 0);

            oeffnungen.Sort((a, b) => a.u.CompareTo(b.u));

            // Volle Saeulen zwischen den Oeffnungen (und an den Enden).
            float grenze = -laenge * 0.5f;
            foreach (var o in oeffnungen)
            {
                float links = o.u - o.breite * 0.5f;
                if (links > grenze)
                    Segment(wand.transform, (grenze + links) * 0.5f, wandHoehe * 0.5f,
                            links - grenze, wandHoehe);
                grenze = o.u + o.breite * 0.5f;
            }
            if (laenge * 0.5f > grenze)
                Segment(wand.transform, (grenze + laenge * 0.5f) * 0.5f, wandHoehe * 0.5f,
                        laenge * 0.5f - grenze, wandHoehe);

            // Sturz oben, Bruestung unten, Marker in der Mitte jeder Oeffnung.
            foreach (var o in oeffnungen)
            {
                float oben = o.bruestung + o.hoehe;
                if (wandHoehe > oben)
                    Segment(wand.transform, o.u, (oben + wandHoehe) * 0.5f, o.breite, wandHoehe - oben);
                if (o.bruestung > 0f)
                    Segment(wand.transform, o.u, o.bruestung * 0.5f, o.breite, o.bruestung);

                var slot = new GameObject("Oeffnung_" + o.art);
                slot.transform.SetParent(wand.transform, false);
                slot.transform.localPosition = new Vector3(o.u, o.bruestung + o.hoehe * 0.5f, 0);
                var os = slot.AddComponent<OpeningSlot>();
                os.kind = o.art;
                os.width = o.breite;
                os.height = o.hoehe;
            }
        }

        // Ein Wand-Segment: lokale X-Mitte, lokale Y-Mitte, Laenge (X), Hoehe (Y).
        void Segment(Transform parent, float x, float y, float laenge, float hoehe)
        {
            if (laenge <= 0.001f || hoehe <= 0.001f) return;
            var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.name = "Segment";
            c.transform.SetParent(parent, false);
            c.transform.localPosition = new Vector3(x, y, 0);
            c.transform.localScale = new Vector3(laenge, hoehe, wandDicke);
            if (wandMaterial) c.GetComponent<Renderer>().sharedMaterial = wandMaterial;
        }

        void Quader(string name, Vector3 lokalePos, Vector3 groesse, Material mat)
        {
            var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.name = name;
            c.transform.SetParent(transform, false);
            c.transform.localPosition = lokalePos;
            c.transform.localScale = groesse;
            if (mat) c.GetComponent<Renderer>().sharedMaterial = mat;
        }
    }
}
