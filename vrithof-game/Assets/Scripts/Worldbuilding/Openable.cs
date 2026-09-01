using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

namespace Vrithof.Worldbuilding
{
    /// M3: eine verbarrikadierbare Oeffnung. Gehoert auf denselben GameObject
    /// wie der OpeningSlot — der Builder setzt beides zusammen.
    ///
    /// Zwei Schichten, bewusst getrennt:
    ///
    ///   GRUNDSUBSTANZ — das Tuerblatt. Haelt Zombies auf, laesst den Spieler
    ///   aber durch (kein Collider, nur HP). Ein Fenster hat davon nichts,
    ///   grundHP bleibt 0: es ist ein Loch.
    ///
    ///   BRETTER — die Barrikade obendrauf, kostet Eisen. Sie hat Collider und
    ///   sperrt *beide* Seiten. Wer seine Tuer zunagelt, kommt selbst nicht mehr
    ///   raus. Das ist Absicht.
    ///
    /// Schaden frisst sich von aussen nach innen: erst die Bretter, eins nach
    /// dem anderen, dann die Grundsubstanz. Wie viele Bretter noch stehen,
    /// sieht man ohne UI — sie verschwinden einzeln.
    [RequireComponent(typeof(OpeningSlot))]
    public class Openable : MonoBehaviour
    {
        [Header("Bretter")]
        public int maxBretter = 3;
        [Tooltip("Bretter, die von Anfang an drauf sind. Normalerweise 0.")]
        public int startBretter = 0;
        [Tooltip("Aushaltevermoegen eines einzelnen Bretts.")]
        public float hpProBrett = 40f;

        [Header("Kosten")]
        [Tooltip("Eisen pro Brett — die Naegel, die es braucht.")]
        public int eisenProBrett = 4;
        [Tooltip("Was beim Abbauen zurueckkommt. Weniger als es gekostet hat: " +
                 "ein Teil der Naegel verbiegt sich.")]
        public int eisenZurueck = 2;
        [Tooltip("Ein angeschlagenes Brett nachnageln ist billiger als ein neues " +
                 "zu setzen. Sonst frisst das Reparieren den Vorrat schneller auf, " +
                 "als eine Nacht lang ist.")]
        public int eisenProReparatur = 2;

        [Header("Grundsubstanz")]
        [Tooltip("HP des Tuerblatts. 0 = offenes Loch (Fenster). " +
                 "Der Spieler kann hier immer durch, Zombies erst bei 0.")]
        public float grundHP = 0f;

        [Header("Darstellung")]
        public float brettDicke = 0.08f;
        [Tooltip("Anteil der Oeffnungshoehe, den ein Brett einnimmt. " +
                 "Unter 1 bleiben Spalte — man sieht hindurch.")]
        [Range(0.1f, 1f)]
        public float brettFuellung = 0.6f;
        public Material brettMaterial;

        [Header("Durchgang")]
        [Tooltip("Wie weit die Endpunkte des NavMeshLink beidseits der Wand " +
                 "liegen. Muss ueber die Wanddicke hinausreichen und auf " +
                 "begehbarer Flaeche landen.")]
        public float linkTiefe = 1f;
        [Tooltip("Tiefe der NavMesh-Sperre. Muss die Wand durchdringen, sonst " +
                 "bleibt daneben ein Streifen begehbar.")]
        public float sperreTiefe = 0.6f;

        [Header("Hervorhebung")]
        [Tooltip("Staerke des Rahmens, der beim Anvisieren aufleuchtet.")]
        public float rahmenDicke = 0.06f;
        public Color rahmenFarbe = new Color(1f, 0.85f, 0.3f);

        /// Alle lebenden Oeffnungen. Jede meldet sich selbst an — kein Manager,
        /// der alles kennen muss. Dieselbe Liste traegt spaeter das Laerm-System.
        public static readonly List<Openable> Alle = new List<Openable>();

        void OnEnable() { Alle.Add(this); }
        void OnDisable() { Alle.Remove(this); }

        OpeningSlot slot;
        Transform[] bretterObjekte;
        GameObject rahmen;
        NavMeshLink link;
        NavMeshObstacle sperre;
        int bretter;
        float brettRest;    // HP des obersten stehenden Bretts
        float grundRest;    // HP der Grundsubstanz

        /// Koennen Zombies hier durch? Erst wenn kein Brett und keine Substanz mehr steht.
        public bool IstOffen => bretter == 0 && grundRest <= 0f;
        /// Passt noch ein Brett drauf?
        public bool KannVerstaerken => bretter < maxBretter;
        public int Bretter => bretter;

        void Awake()
        {
            slot = GetComponent<OpeningSlot>();
            ZielflaecheErzeugen();
            BretterErzeugen();
            RahmenErzeugen();
            LinkErzeugen();
            SperreErzeugen();
            bretter = Mathf.Clamp(startBretter, 0, maxBretter);
            grundRest = grundHP;
            brettRest = hpProBrett;
            Anzeigen();
            DurchgangAktualisieren();
        }

        /// Braucht diese Oeffnung Arbeit? Entweder fehlt ein Brett oder das
        /// oberste ist angeschlagen.
        public bool BrauchtArbeit => KannVerstaerken || brettRest < hpProBrett;

        /// Was der naechste Handgriff hier kostet: ein neues Brett oder nur
        /// das Nachnageln eines angeschlagenen.
        public int NaechsteKosten => KannVerstaerken ? eisenProBrett : eisenProReparatur;

        /// Ein Brett nachlegen — oder, wenn schon voll, das oberste ausbessern.
        /// Beides kostet gleich viel und ist derselbe Handgriff: haemmern.
        /// Gibt false zurueck, wenn nichts zu tun ist.
        public bool Verstaerken()
        {
            if (KannVerstaerken)
            {
                bretter++;
                brettRest = hpProBrett;   // das neue Brett ist frisch
                Anzeigen();
                DurchgangAktualisieren();
                return true;
            }

            if (brettRest < hpProBrett)   // voll, aber das oberste haengt schief
            {
                brettRest = hpProBrett;
                return true;
            }

            return false;
        }

        /// Steht hier ein Brett, das man wieder abnehmen kann?
        public bool KannAbbauen => bretter > 0;

        /// Ein Brett abnehmen. Ohne das koennte man sich einmauern und nur noch
        /// darauf hoffen, dass jemand von aussen ein Loch schlaegt.
        public bool Abbauen()
        {
            if (bretter == 0) return false;
            bretter--;
            brettRest = hpProBrett;
            Anzeigen();
            DurchgangAktualisieren();
            return true;
        }

        /// Schaden von aussen. Frisst erst die Bretter, dann die Grundsubstanz.
        public void Schaden(float menge)
        {
            if (bretter > 0)
            {
                brettRest -= menge;
                if (brettRest <= 0f)
                {
                    bretter--;
                    brettRest = hpProBrett;
                    Anzeigen();
                    DurchgangAktualisieren();
                }
                return;
            }

            if (grundRest > 0f)
            {
                grundRest = Mathf.Max(0f, grundRest - menge);
                if (grundRest <= 0f) DurchgangAktualisieren();
            }
        }

        // Der Weg durch die Oeffnung. Unity bringt das mit: ein NavMeshLink
        // verbindet die Flaeche innen mit der draussen. Ist er aus, ist die
        // Oeffnung fuer Zombies keine — dann muessen sie sie erst einschlagen.
        //
        // Ohne das waeren fuenf von sechs Oeffnungen Deko, weil nur das Tuerloch
        // im NavMesh existiert.
        void LinkErzeugen()
        {
            link = gameObject.AddComponent<NavMeshLink>();
            // Der Slot sitzt in Oeffnungsmitte; die Wand steht auf y=0. Also
            // liegt der Boden so viel tiefer, wie der Slot hoch haengt.
            float boden = -transform.localPosition.y;
            link.startPoint = new Vector3(0f, boden, -linkTiefe);
            link.endPoint = new Vector3(0f, boden, linkTiefe);
            link.width = slot.width;
            link.bidirectional = true;
        }

        // Ein Link kann Wege nur hinzufuegen, nie wegnehmen. Die Tuer ist aber
        // ein Loch bis zum Boden — dort backt Unity durchgehende Flaeche, und
        // Bretter davor wuerden nichts aendern. Ein NavMeshObstacle mit Carving
        // schneidet sie zur Laufzeit heraus.
        //
        // Zusammen ergibt das fuer jede Oeffnung dasselbe Verhalten, egal ob
        // Tuer oder Fenster: offen heisst durchlaessig, zu heisst zu.
        void SperreErzeugen()
        {
            sperre = gameObject.AddComponent<NavMeshObstacle>();
            sperre.shape = NavMeshObstacleShape.Box;
            sperre.size = new Vector3(slot.width, slot.height, sperreTiefe);
            sperre.carving = true;
        }

        void DurchgangAktualisieren()
        {
            bool offen = IstOffen;
            if (link) link.enabled = offen;
            if (sperre) sperre.enabled = !offen;
        }

        /// Rahmen an oder aus. Ruft die Interaktion, wenn der Spieler hinsieht.
        public void Hervorheben(bool an)
        {
            if (rahmen) rahmen.SetActive(an);
        }

        // Vier duenne Balken auf den Kanten der Oeffnung. Ein richtiges Outline
        // waere in URP ein Renderer Feature — viel Aufwand fuer reine Optik.
        // Unlit, damit der Rahmen auch in der Nacht zu sehen ist.
        void RahmenErzeugen()
        {
            rahmen = new GameObject("Rahmen");
            rahmen.transform.SetParent(transform, false);

            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            var mat = shader != null ? new Material(shader) : null;
            if (mat != null) mat.color = rahmenFarbe;

            float w = slot.width, h = slot.height, d = rahmenDicke;
            Balken(mat, new Vector3(0, h * 0.5f, 0), new Vector3(w + d, d, d));
            Balken(mat, new Vector3(0, -h * 0.5f, 0), new Vector3(w + d, d, d));
            Balken(mat, new Vector3(-w * 0.5f, 0, 0), new Vector3(d, h, d));
            Balken(mat, new Vector3(w * 0.5f, 0, 0), new Vector3(d, h, d));

            rahmen.SetActive(false);
        }

        void Balken(Material mat, Vector3 pos, Vector3 groesse)
        {
            var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.SetParent(rahmen.transform, false);
            c.transform.localPosition = pos;
            c.transform.localScale = groesse;
            Destroy(c.GetComponent<Collider>());   // darf niemandem im Weg stehen
            if (mat) c.GetComponent<Renderer>().sharedMaterial = mat;
        }

        // Ohne Bretter haette die Oeffnung nichts, worauf ein Raycast treffen
        // koennte. Ein Trigger in Oeffnungsgroesse gibt ihr eine Zielflaeche,
        // ohne jemandem den Weg zu versperren.
        void ZielflaecheErzeugen()
        {
            var box = gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(slot.width, slot.height, 0.1f);
        }

        // Alle Bretter einmal erzeugen, danach nur noch ein- und ausblenden.
        void BretterErzeugen()
        {
            bretterObjekte = new Transform[maxBretter];
            float fach = slot.height / maxBretter;
            float hoehe = fach * brettFuellung;

            for (int i = 0; i < maxBretter; i++)
            {
                var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                c.name = "Brett_" + i;
                c.transform.SetParent(transform, false);
                // Von unten nach oben zunageln. Der Slot sitzt in der Oeffnungsmitte.
                float y = -slot.height * 0.5f + fach * (i + 0.5f);
                c.transform.localPosition = new Vector3(0, y, 0);
                c.transform.localScale = new Vector3(slot.width, hoehe, brettDicke);
                if (brettMaterial) c.GetComponent<Renderer>().sharedMaterial = brettMaterial;
                bretterObjekte[i] = c.transform;
            }
        }

        void Anzeigen()
        {
            if (bretterObjekte == null) return;
            for (int i = 0; i < bretterObjekte.Length; i++)
                if (bretterObjekte[i]) bretterObjekte[i].gameObject.SetActive(i < bretter);
        }
    }
}
