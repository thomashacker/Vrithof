using UnityEngine;

namespace Vrithof.Worldbuilding
{
    /// M5a: der Amboss in der Basis. Hier entsteht die Axt und hier wird sie
    /// wieder scharf.
    ///
    /// Bewusst ortsgebunden: Das zwingt zur Rueckkehr und gibt der Basis einen
    /// Zweck, den sie seit M4 nicht mehr hatte. Und es bleibt trotzdem auf der
    /// richtigen Seite der Nein-Liste — ein einzelner Amboss ist keine Werkbank
    /// mit Rezeptbaum.
    ///
    /// Auf einen Wuerfel in der Basis legen.
    public class Amboss : MonoBehaviour
    {
        [Header("Preise in Eisen")]
        public int schmiedeKosten = 18;
        [Tooltip("Schaerfen ist deutlich billiger als neu schmieden — sonst " +
                 "wuerde man die stumpfe Axt einfach wegwerfen.")]
        public int schliffKosten = 6;

        [Header("Arbeit")]
        [Tooltip("Sekunden am Amboss. Laut und lang genug, um es nachts nicht " +
                 "nebenbei zu machen.")]
        public float schmiedeZeit = 4f;
        public float schliffZeit = 2f;
        [Tooltip("Wie weit der Amboss zu hoeren ist. Lauter als alles andere.")]
        public float laerm = 34f;
    }
}
