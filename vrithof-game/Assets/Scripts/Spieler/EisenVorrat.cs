using UnityEngine;

namespace Vrithof.Spieler
{
    /// M3: das Eisen. Eine Zahl, mehr braucht es nicht.
    ///
    /// Ab M4 wird es nicht mehr geschenkt, sondern aus Truhen gepluendert.
    /// Die Regel dahinter steht im Design: eine Ressource, die reicht, ist
    /// keine Ressource. Wenn du beim Testen merkst, dass alle Oeffnungen zu
    /// sichern sind, liegt zu viel in den Truhen.
    ///
    /// Auf den Player legen.
    public class EisenVorrat : MonoBehaviour
    {
        [Tooltip("Startvorrat. Ab M4 bei 0 — alles, was du hast, hast du gefunden.")]
        public int eisen = 0;

        public bool anzeigen = true;

        public bool Reicht(int menge) => eisen >= menge;

        /// Zieht ab, wenn genug da ist. Sonst passiert nichts und es kommt false.
        public bool Abziehen(int menge)
        {
            if (!Reicht(menge)) return false;
            eisen -= menge;
            return true;
        }

        void OnGUI()
        {
            if (!anzeigen) return;
            // Unter Uhr (8), Leben (34) und Ausdauerbalken (64..74).
            var style = new GUIStyle(GUI.skin.label) { fontSize = 20 };
            style.normal.textColor = eisen > 0 ? Color.white : Color.red;
            GUI.Label(new Rect(12, 80, 300, 30), $"Eisen {eisen}", style);
        }
    }
}
