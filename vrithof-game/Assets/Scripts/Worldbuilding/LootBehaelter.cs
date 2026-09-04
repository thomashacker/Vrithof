using UnityEngine;

namespace Vrithof.Worldbuilding
{
    /// M4: eine Truhe. Einmal durchsuchen, dann ist sie leer — endgueltig.
    ///
    /// Das ist der Motor des ganzen Loops: Jede geplünderte Truhe macht die
    /// Umgebung aermer und schiebt dich beim naechsten Mal weiter hinaus.
    /// Weiter hinaus heisst laenger unterwegs, weniger Puffer bis zur Nacht.
    ///
    /// Manche Truhen sind leer. Ohne das waere Plündern eine Rechenaufgabe
    /// statt eines Risikos — man wuesste vorher, was ein Weg einbringt.
    ///
    /// Auf einen Wuerfel legen (Blockout-Optik, wie alles andere).
    public class LootBehaelter : MonoBehaviour
    {
        [Header("Inhalt")]
        public int eisenMin = 3;
        public int eisenMax = 9;
        [Tooltip("Nahrung liegt in denselben Truhen wie das Eisen. Genau daraus " +
                 "entsteht die Entscheidung: derselbe Weg deckt beide Beduerfnisse, " +
                 "und was er nicht hergibt, fehlt an zwei Stellen.")]
        public int nahrungMin = 0;
        public int nahrungMax = 2;
        [Tooltip("Fackeln. Selten genug, dass Licht eine Entscheidung bleibt.")]
        public int fackelMin = 0;
        public int fackelMax = 1;
        [Range(0f, 1f)]
        [Tooltip("Anteil der Truhen, die nichts hergeben.")]
        public float leerChance = 0.25f;

        [Header("Durchsuchen")]
        [Tooltip("Sekunden Wuehlen. Waehrenddessen ist man gebunden.")]
        public float suchZeit = 2f;
        [Tooltip("Wie weit das Wuehlen zu hoeren ist. Leise, aber nicht lautlos.")]
        public float suchLaerm = 7f;

        [Header("Darstellung")]
        [Tooltip("Farbe, wenn die Truhe leer ist. Man soll von weitem sehen, " +
                 "wo man schon war.")]
        public Color leerFarbe = new Color(0.28f, 0.26f, 0.24f);

        public bool Gepluendert { get; private set; }

        int eisen;
        int nahrung;
        int fackeln;

        void Awake()
        {
            // Vorher wuerfeln, nicht erst beim Oeffnen: so laesst sich eine Runde
            // reproduzieren, wenn beim Testen etwas seltsam aussieht.
            if (Random.value < leerChance) return;
            eisen = Random.Range(eisenMin, eisenMax + 1);
            nahrung = Random.Range(nahrungMin, nahrungMax + 1);
            fackeln = Random.Range(fackelMin, fackelMax + 1);
        }

        /// Ausraeumen. Gibt zurueck, was drin war — auch nichts ist ein gueltiges
        /// Ergebnis, die Truhe gilt danach trotzdem als durchsucht.
        public void Ausraeumen(out int eisenBeute, out int nahrungBeute, out int fackelBeute)
        {
            eisenBeute = 0;
            nahrungBeute = 0;
            fackelBeute = 0;
            if (Gepluendert) return;

            Gepluendert = true;
            eisenBeute = eisen;
            nahrungBeute = nahrung;
            fackelBeute = fackeln;
            Verblassen();
        }

        void Verblassen()
        {
            var r = GetComponent<Renderer>();
            if (r != null) r.material.color = leerFarbe;
        }
    }
}
