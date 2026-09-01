using UnityEngine;

namespace Vrithof.Spieler
{
    /// M3: das Eisen. Eine Zahl, mehr braucht es nicht.
    ///
    /// In M3 wird es geschenkt — der Startwert ist alles, was du je haben wirst.
    /// Ab M4 kommt es aus Loot-Containern. Die Regel dahinter steht im Design:
    /// eine Ressource, die reicht, ist keine Ressource. Wenn du beim Testen
    /// merkst, dass alle Oeffnungen zu sichern sind, ist der Startwert falsch.
    ///
    /// Auf den Player legen.
    public class EisenVorrat : MonoBehaviour
    {
        [Tooltip("Vier Fenster voll zu sichern kostet 48. Absichtlich weniger.")]
        public int eisen = 40;

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
