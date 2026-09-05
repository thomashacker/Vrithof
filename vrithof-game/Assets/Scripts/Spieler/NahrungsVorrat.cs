using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vrithof.Spieler
{
    /// M4: die Uhr, die dich nach draussen treibt.
    ///
    /// Ohne sie waere Verstecken die beste Strategie — und ein Spiel, in dem
    /// Nichtstun optimal ist, ist kaputt. Die Regel dahinter: Verstecken muss
    /// teurer sein als Rausgehen.
    ///
    /// Nahrung tickt unabhaengig davon, was du tust, und kommt nur aus Truhen.
    /// Deshalb ist jeder Pluenderausflug doppelt gedeckt: du holst Eisen fuer
    /// die Nacht und Nahrung fuer den Tag, und beides liegt am selben Ort.
    ///
    /// Auf den Player legen.
    public class NahrungsVorrat : MonoBehaviour
    {
        [Header("Nahrung")]
        public float maxNahrung = 200f;
        [Tooltip("Startwert. Bewusst nicht voll — der erste Ausflug soll nicht " +
                 "warten koennen.")]
        public float startNahrung = 140f;
        [Tooltip("Verbrauch pro Spielstunde. Mal 24 ergibt den Tagesbedarf — " +
                 "der muss deutlich unter maxNahrung liegen, sonst laesst sich nie " +
                 "ein Vorrat anlegen und es bleibt bei Hetze statt Planung.")]
        public float verbrauchProStunde = 1.5f;
        [Tooltip("Wie viel Nahrung eine gefundene Einheit bringt.")]
        public float einheitenWert = 15f;

        [Header("Hunger")]
        [Tooltip("Ab diesem Wert tut es weh: Leben tickt herunter.")]
        public float hungerSchwelle = 0f;
        [Tooltip("Schaden pro Sekunde bei leerem Magen.")]
        public float hungerSchaden = 2f;

        [Header("Anzeige")]
        public bool anzeigen = true;

        float nahrung;
        SpielerLeben leben;
        Zeit.TageszeitZyklus zyklus;

        void Awake()
        {
            nahrung = Mathf.Clamp(startNahrung, 0f, maxNahrung);
            leben = GetComponent<SpielerLeben>();
        }

        void Start()
        {
            zyklus = FindAnyObjectByType<Zeit.TageszeitZyklus>();
        }

        /// Essen aus einer Truhe. Eine Einheit Nahrung ist ein Tagesbedarf-Bruchteil.
        public void Essen(int einheiten)
        {
            if (einheiten <= 0) return;
            nahrung = Mathf.Min(maxNahrung, nahrung + einheiten * einheitenWert);
        }

        void Update()
        {
            // An die Spielzeit koppeln, nicht an Sekunden: sonst haengt der
            // Hunger an der Tageslaenge, die wir beim Balancing staendig drehen.
            float stundenProSekunde = zyklus != null
                ? 24f / (Mathf.Max(0.1f, zyklus.tagLaengeMinuten) * 60f)
                : 24f / 360f;

            nahrung -= verbrauchProStunde * stundenProSekunde * Time.deltaTime;

            if (nahrung > hungerSchwelle) return;

            nahrung = hungerSchwelle;
            if (leben != null) leben.Schaden(hungerSchaden * Time.deltaTime);
        }

        void OnGUI()
        {
            if (!anzeigen) return;
            // Unter Uhr (8), Leben (34), Ausdauer (64), Eisen (80), Laerm (108/126).
            var style = new GUIStyle(GUI.skin.label) { fontSize = 20 };
            float anteil = nahrung / Mathf.Max(1f, maxNahrung);
            style.normal.textColor = anteil > 0.25f ? Color.white
                                   : anteil > 0f ? new Color(1f, 0.7f, 0.2f)
                                   : Color.red;
            GUI.Label(new Rect(12, 146, 300, 30),
                      $"Nahrung {Mathf.CeilToInt(nahrung)}", style);
        }
    }
}
