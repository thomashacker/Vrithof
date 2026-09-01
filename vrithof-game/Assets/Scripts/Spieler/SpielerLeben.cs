using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vrithof.Spieler
{
    /// M2: Lebenspunkte, Anzeige, Tod. Tod laedt die Szene neu — kein Save-System,
    /// kein Menue, keine Wiederbelebung. Auf den Player legen (Tag 'Player').
    public class SpielerLeben : MonoBehaviour
    {
        [Header("Leben")]
        public float maxLeben = 100f;

        [Header("Anzeige")]
        public bool lebenZeigen = true;

        float leben;
        bool tot;

        void Awake()
        {
            leben = maxLeben;
        }

        public void Schaden(float menge)
        {
            if (tot) return;
            leben -= menge;
            if (leben <= 0f)
            {
                leben = 0f;
                Sterben();
            }
        }

        void Sterben()
        {
            tot = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        void OnGUI()
        {
            if (!lebenZeigen) return;
            // Unter der Uhr des Tageszeit-Zyklus.
            var style = new GUIStyle(GUI.skin.label) { fontSize = 20 };
            style.normal.textColor = leben > maxLeben * 0.3f ? Color.white : Color.red;
            GUI.Label(new Rect(12, 34, 300, 30), $"Leben {Mathf.CeilToInt(leben)}", style);
        }
    }
}
