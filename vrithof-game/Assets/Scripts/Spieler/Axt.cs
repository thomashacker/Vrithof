using UnityEngine;

namespace Vrithof.Spieler
{
    /// M5a: die zweite Senke fuer Eisen.
    ///
    /// Bis M4 gab es nur eine — Barrikaden — und die hat nicht getragen. Eine
    /// Ressource mit einer einzigen unlustigen Senke ist tot. Die Axt schliesst
    /// den Kreis: Eisen verschafft Zugang zu Beute, Beute bringt Eisen.
    ///
    /// Sie ist eine Beschleunigung, keine Pflicht. Ohne sie kommt man auch durch
    /// eine Barrikade, nur dreimal so langsam — man kann sich also nie festfahren.
    ///
    /// Geschmiedet und geschaerft wird am Amboss in der Basis. Damit hat die
    /// Basis wieder einen Zweck, ohne dass es dafuer eine Werkbank-Mechanik
    /// braucht.
    ///
    /// Auf den Player legen.
    public class Axt : MonoBehaviour
    {
        [Header("Besitz")]
        public bool hatAxt;

        [Header("Schaerfe")]
        [Tooltip("Bretter, die eine frisch geschmiedete Axt schafft.")]
        public int maxSchaerfe = 25;
        [Tooltip("Aktueller Stand. Bei 0 ist sie stumpf — sie zerbricht nicht, " +
                 "sie taugt nur nichts mehr, bis sie geschaerft wird.")]
        public int schaerfe;

        [Header("Wirkung")]
        [Tooltip("Um diesen Faktor schneller geht das Aufbrechen mit scharfer Axt. " +
                 "Deutlich spuerbar, aber kein Freifahrtschein — auch mit Axt " +
                 "steht man lange genug davor, um gehoert zu werden.")]
        public float tempoFaktor = 2.5f;

        [Header("Anzeige")]
        public bool anzeigen = true;

        /// Taugt sie gerade etwas?
        public bool IstScharf => hatAxt && schaerfe > 0;
        /// Faktor auf die Abbaugeschwindigkeit.
        public float TempoFaktor => IstScharf ? Mathf.Max(1f, tempoFaktor) : 1f;
        public bool BrauchtSchliff => hatAxt && schaerfe < maxSchaerfe;

        /// Ein Brett aufgebrochen — die Klinge nimmt es uebel.
        public void Abnutzen()
        {
            if (!IstScharf) return;
            schaerfe = Mathf.Max(0, schaerfe - 1);
        }

        /// Frisch vom Amboss.
        public void Schmieden()
        {
            hatAxt = true;
            schaerfe = maxSchaerfe;
        }

        void OnGUI()
        {
            if (!anzeigen || !hatAxt) return;
            // Unter Fackel (172).
            var style = new GUIStyle(GUI.skin.label) { fontSize = 20 };
            style.normal.textColor = schaerfe > maxSchaerfe * 0.25f ? Color.white
                                   : schaerfe > 0 ? new Color(1f, 0.7f, 0.2f)
                                   : Color.red;
            GUI.Label(new Rect(12, 198, 300, 30),
                      schaerfe > 0 ? $"Axt {schaerfe}/{maxSchaerfe}" : "Axt stumpf", style);
        }
    }
}
