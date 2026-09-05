using System.Collections.Generic;
using UnityEngine;

namespace Vrithof.Worldbuilding
{
    /// Autoren-Werkzeug wie der GehoeftBuilder: legt Wege zwischen den Gehoeften
    /// an. Auf ein leeres GameObject legen, im Kontextmenue "Wege bauen".
    ///
    /// Sternfoermig von der Basis aus — das ist die Form, die dem Spiel
    /// entspricht: man geht von zuhause los und kommt zurueck.
    ///
    /// Der Weg ist heller als der Boden, damit ihn nachts das bisschen
    /// Umgebungslicht noch abhebt. Dazu Pfaehle im Abstand: eine Silhouette
    /// sieht man auch dann noch, wenn vom Boden nichts mehr zu erkennen ist.
    [DisallowMultipleComponent]
    public class Wegenetz : MonoBehaviour
    {
        [Header("Weg")]
        public float breite = 3f;
        [Tooltip("Wie weit der Weg ueber dem Boden liegt. Zu wenig flackert " +
                 "(Z-Fighting), zu viel sieht nach Teppich aus.")]
        public float hoehe = 0.03f;
        public Material wegMaterial;

        [Header("Pfaehle")]
        [Tooltip("Markierungen am Wegrand. Nachts wichtiger als der Weg selbst.")]
        public bool pfaehleBauen = true;
        public float pfahlAbstand = 12f;
        public float pfahlHoehe = 1.6f;
        public float pfahlDicke = 0.18f;
        public Material pfahlMaterial;

        [ContextMenu("Wege bauen")]
        public void Bauen()
        {
            Loeschen();

            var gehoefte = new List<GehoeftZustand>(
                FindObjectsByType<GehoeftZustand>(FindObjectsSortMode.None));
            if (gehoefte.Count < 2)
            {
                Debug.LogWarning("Wegenetz: weniger als zwei Gehoefte gefunden.", this);
                return;
            }

            GehoeftZustand basis = gehoefte.Find(g => g.istBasis) ?? gehoefte[0];

            int gebaut = 0;
            foreach (var g in gehoefte)
            {
                if (g == basis) continue;
                Weg(basis.transform.position, g.transform.position, g.name);
                gebaut++;
            }
            Debug.Log($"Wegenetz: {gebaut} Wege von '{basis.name}' aus gebaut.", this);
        }

        [ContextMenu("Wege loeschen")]
        public void Loeschen()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);
        }

        void Weg(Vector3 von, Vector3 nach, string name)
        {
            // Die Hoehe der Gehoefte uebernehmen statt 0 anzunehmen — sonst
            // liegt der Weg im Boden, sobald die Ebene woanders sitzt.
            von.y += hoehe;
            nach.y += hoehe;
            Vector3 strecke = nach - von;
            float laenge = strecke.magnitude;
            if (laenge < 1f) return;

            var wurzel = new GameObject("Weg_" + name);
            wurzel.transform.SetParent(transform, false);
            wurzel.transform.position = (von + nach) * 0.5f;
            wurzel.transform.rotation = Quaternion.LookRotation(strecke.normalized, Vector3.up);

            var band = GameObject.CreatePrimitive(PrimitiveType.Cube);
            band.name = "Band";
            band.transform.SetParent(wurzel.transform, false);
            band.transform.localScale = new Vector3(breite, hoehe, laenge);
            // Kein Collider: der Weg soll niemanden anheben oder haengen lassen.
            DestroyImmediate(band.GetComponent<Collider>());
            if (wegMaterial) band.GetComponent<Renderer>().sharedMaterial = wegMaterial;

            if (!pfaehleBauen || pfahlAbstand < 1f) return;

            int wieViele = Mathf.FloorToInt(laenge / pfahlAbstand);
            for (int i = 1; i < wieViele; i++)
            {
                float z = -laenge * 0.5f + i * pfahlAbstand;
                Pfahl(wurzel.transform, new Vector3(breite * 0.5f + 0.4f, 0f, z));
                Pfahl(wurzel.transform, new Vector3(-breite * 0.5f - 0.4f, 0f, z));
            }
        }

        void Pfahl(Transform eltern, Vector3 lokal)
        {
            var p = GameObject.CreatePrimitive(PrimitiveType.Cube);
            p.name = "Pfahl";
            p.transform.SetParent(eltern, false);
            p.transform.localPosition = lokal + Vector3.up * (pfahlHoehe * 0.5f);
            p.transform.localScale = new Vector3(pfahlDicke, pfahlHoehe, pfahlDicke);
            if (pfahlMaterial) p.GetComponent<Renderer>().sharedMaterial = pfahlMaterial;
        }
    }
}
