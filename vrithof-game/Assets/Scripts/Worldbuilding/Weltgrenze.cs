using UnityEngine;

namespace Vrithof.Worldbuilding
{
    /// Unsichtbare Wand am Rand der Welt, damit man nachts nicht versehentlich
    /// von der Ebene faellt. Das ist kein Balancing, das ist ein Loch im Boden.
    ///
    /// Direkt auf die Boden-Plane legen: die Grenze liest deren Bounds aus und
    /// passt sich an, auch wenn die Plane skaliert wird. Kein zweiter Ort, an
    /// dem eine Groesse steht und veralten kann.
    ///
    /// Die Waende werden ausserhalb der Hierarchie erzeugt — als Kinder einer
    /// zehnfach skalierten Plane waeren sie selbst zehnfach skaliert.
    ///
    /// Dazu ein Fangnetz: wer trotzdem unten landet (Luecke, Sprung, Bug), wird
    /// zurueckgesetzt statt endlos zu fallen.
    [DisallowMultipleComponent]
    public class Weltgrenze : MonoBehaviour
    {
        [Header("Mauer")]
        public float mauerHoehe = 30f;
        public float mauerDicke = 2f;
        [Tooltip("Wie weit die Mauer nach innen rueckt. Positiv heisst: etwas " +
                 "vor der Kante, damit man nicht bis an den Abgrund kommt.")]
        public float randAbstand = 1f;
        [Tooltip("Sichtbar machen, um die Grenze beim Bauen zu sehen.")]
        public bool sichtbar;

        [Header("Fangnetz")]
        [Tooltip("Wie weit unter dem Boden jemand als abgestuerzt gilt.")]
        public float fangTiefe = 10f;
        [Tooltip("Wohin zurueckgesetzt wird. Leer = Mitte der Flaeche.")]
        public Transform rettungsPunkt;

        Transform spieler;
        CharacterController spielerKoerper;
        Bounds flaeche;
        bool bereit;

        void Start()
        {
            if (!FlaecheLesen()) return;
            MauerBauen();

            var p = GameObject.FindGameObjectWithTag("Player");
            if (p == null) return;
            spieler = p.transform;
            spielerKoerper = p.GetComponent<CharacterController>();
        }

        bool FlaecheLesen()
        {
            // Collider zuerst: er beschreibt, wo man tatsaechlich stehen kann.
            var c = GetComponent<Collider>();
            if (c != null) { flaeche = c.bounds; bereit = true; return true; }

            var r = GetComponent<Renderer>();
            if (r != null) { flaeche = r.bounds; bereit = true; return true; }

            Debug.LogWarning("Weltgrenze: weder Collider noch Renderer gefunden — " +
                             "auf die Boden-Plane legen.", this);
            return false;
        }

        void MauerBauen()
        {
            var wurzel = new GameObject("Weltgrenze");   // bewusst ohne Eltern
            Vector3 m = flaeche.center;
            float x = flaeche.extents.x - randAbstand;
            float z = flaeche.extents.z - randAbstand;
            float y = flaeche.max.y + mauerHoehe * 0.5f;

            Wand(wurzel.transform, "Nord", new Vector3(m.x, y, m.z + z),
                 new Vector3(x * 2f, mauerHoehe, mauerDicke));
            Wand(wurzel.transform, "Sued", new Vector3(m.x, y, m.z - z),
                 new Vector3(x * 2f, mauerHoehe, mauerDicke));
            Wand(wurzel.transform, "Ost", new Vector3(m.x + x, y, m.z),
                 new Vector3(mauerDicke, mauerHoehe, z * 2f));
            Wand(wurzel.transform, "West", new Vector3(m.x - x, y, m.z),
                 new Vector3(mauerDicke, mauerHoehe, z * 2f));
        }

        void Wand(Transform eltern, string name, Vector3 pos, Vector3 groesse)
        {
            var w = GameObject.CreatePrimitive(PrimitiveType.Cube);
            w.name = "Grenze_" + name;
            w.transform.SetParent(eltern, true);
            w.transform.position = pos;
            w.transform.localScale = groesse;

            var r = w.GetComponent<Renderer>();
            if (r != null) r.enabled = sichtbar;
        }

        void Update()
        {
            if (!bereit || spieler == null) return;
            if (spieler.position.y > flaeche.min.y - fangTiefe) return;

            Vector3 ziel = rettungsPunkt != null
                ? rettungsPunkt.position
                : new Vector3(flaeche.center.x, flaeche.max.y + 2f, flaeche.center.z);

            // Der CharacterController muss aus, sonst schreibt er die Position
            // gleich wieder zurueck.
            if (spielerKoerper != null) spielerKoerper.enabled = false;
            spieler.position = ziel;
            if (spielerKoerper != null) spielerKoerper.enabled = true;
        }

        void OnDrawGizmosSelected()
        {
            var c = GetComponent<Collider>();
            var r = GetComponent<Renderer>();
            Bounds b = c != null ? c.bounds : r != null ? r.bounds : default;
            if (b.size == Vector3.zero) return;

            Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.8f);
            Gizmos.DrawWireCube(b.center + Vector3.up * mauerHoehe * 0.5f,
                                new Vector3((b.extents.x - randAbstand) * 2f,
                                            mauerHoehe,
                                            (b.extents.z - randAbstand) * 2f));
        }
    }
}
