using UnityEngine;
using UnityEngine.SceneManagement;
using Vrithof.Welt;

namespace Vrithof.Spieler
{
    /// M2: Lebenspunkte, Anzeige, Tod. Tod laedt die Szene neu — kein Save-System,
    /// kein Menue, keine Wiederbelebung. Auf den Player legen (Tag 'Player').
    public class SpielerLeben : MonoBehaviour
    {
        [Header("Leben")]
        public float maxLeben = 100f;

        [Header("Treffer-Feedback")]
        [Tooltip("Roter Schleier beim Treffer.")]
        public bool flashZeigen = true;
        public Color flashFarbe = new Color(0.65f, 0f, 0f, 0.4f);
        public float flashDauer = 0.35f;
        [Tooltip("Leer lassen — dann wird ein Platzhalter erzeugt.")]
        public AudioClip trefferKlang;
        [Range(0f, 1f)] public float lautstaerke = 0.6f;
        [Tooltip("Erst ab diesem Schaden gibt es Feedback. Hunger tickt in " +
                 "Bruchteilen pro Frame — ohne Schwelle wuerde der Bildschirm " +
                 "dauerhaft blinken.")]
        public float feedbackAbSchaden = 2f;

        [Header("Anzeige")]
        public bool lebenZeigen = true;

        float leben;
        bool tot;
        float flashBis;
        AudioSource quelle;

        void Awake()
        {
            leben = maxLeben;

            if (trefferKlang == null) trefferKlang = Klangwerkstatt.Treffer();
            quelle = gameObject.AddComponent<AudioSource>();
            quelle.playOnAwake = false;
            quelle.spatialBlend = 0f;   // am eigenen Leib, nicht im Raum
        }

        public void Schaden(float menge)
        {
            if (tot) return;
            leben -= menge;

            if (menge >= feedbackAbSchaden) Zucken();
            if (leben <= 0f)
            {
                leben = 0f;
                Sterben();
            }
        }

        void Zucken()
        {
            flashBis = Time.time + flashDauer;
            if (quelle == null || trefferKlang == null) return;
            quelle.pitch = Random.Range(0.9f, 1.1f);
            quelle.PlayOneShot(trefferKlang, lautstaerke);
        }

        void Sterben()
        {
            tot = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        void OnGUI()
        {
            Flash();

            if (!lebenZeigen) return;
            // Unter der Uhr des Tageszeit-Zyklus.
            var farbe = leben > maxLeben * 0.3f ? Color.white : Color.red;
            GUI.Label(new Rect(12, 34, 300, 30),
                      "Leben " + Mathf.CeilToInt(leben), Welt.Anzeige.Zahl(farbe));
        }

        void Flash()
        {
            if (!flashZeigen || Time.time >= flashBis) return;

            float rest = (flashBis - Time.time) / Mathf.Max(0.01f, flashDauer);
            var alt = GUI.color;
            GUI.color = new Color(flashFarbe.r, flashFarbe.g, flashFarbe.b,
                                  flashFarbe.a * rest);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height),
                            Texture2D.whiteTexture);
            GUI.color = alt;
        }
    }
}
