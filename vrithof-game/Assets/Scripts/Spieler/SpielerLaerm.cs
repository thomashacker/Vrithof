using UnityEngine;
using Vrithof.Welt;

namespace Vrithof.Spieler
{
    /// M3: macht Krach beim Laufen. Jeder Schritt ist ein Laerm-Ereignis — und
    /// zugleich der Moment fuer ein Schrittgeraeusch, deshalb steckt beides hier.
    ///
    /// Sprinten ist weit zu hoeren, Gehen kaum, Stehen gar nicht. Das ist die
    /// eigentliche Waehrung der Nacht: Wer rennt, kommt schnell weg und zieht
    /// alles hinter sich her. Wer steht, verschwindet.
    ///
    /// Auf den Player legen.
    [RequireComponent(typeof(SpielerController))]
    public class SpielerLaerm : MonoBehaviour
    {
        [Header("Lautstaerke (zugleich Reichweite in Metern)")]
        [Tooltip("Geduckt. So leise, dass man an ihnen vorbeikommt.")]
        public float schleichen = 3f;
        public float gehen = 8f;
        public float sprinten = 22f;
        public float springen = 14f;

        [Header("Schrittfolge")]
        [Tooltip("Sekunden zwischen zwei Schritten im Gehen.")]
        public float schrittGehen = 0.5f;
        [Tooltip("Sekunden zwischen zwei Schritten im Sprint.")]
        public float schrittSprint = 0.3f;
        [Tooltip("Sekunden zwischen zwei Schritten im Ducken.")]
        public float schrittSchleichen = 0.8f;

        [Header("Klang")]
        [Tooltip("Leer lassen — dann wird ein Platzhalter erzeugt. Spaeter " +
                 "echte Clips hier hineinziehen.")]
        public AudioClip gehKlang;
        public AudioClip sprintKlang;
        public AudioClip sprungKlang;
        [Range(0f, 1f)] public float lautstaerke = 0.5f;

        [Header("Anzeige")]
        [Tooltip("Balken, der zeigt, wie weit man gerade zu hoeren ist.")]
        public bool balkenZeigen = true;
        [Tooltip("Bei welcher Staerke der Balken voll ist. Muss ueber dem " +
                 "lautesten Geraeusch liegen, sonst schlaegt er nur noch an.")]
        public float balkenMaximum = 30f;
        [Tooltip("Zeigt die aktuelle Hoerweite als Kreis am Boden. Nur Gizmo, " +
                 "also Scene-View oder Game-View mit eingeschalteten Gizmos.")]
        public bool kreisZeigen = true;

        SpielerController controller;
        AudioSource quelle;
        float naechsterSchritt;

        // Wie laut der letzte Schritt war — nur fuer die Anzeige.
        float letzteStaerke;
        float letzteStaerkeBis;

        void OnEnable()
        {
            if (controller == null) controller = GetComponent<SpielerController>();
            controller.Gesprungen += Absprung;
        }

        void OnDisable()
        {
            if (controller != null) controller.Gesprungen -= Absprung;
        }

        // Der Absprung selbst ist laut genug. Auf die Landung zu warten war nur
        // ein Umweg, weil der fremde Controller nichts gemeldet hat.
        void Absprung() => Ausloesen(springen, sprungKlang);

        void Awake()
        {
            controller = GetComponent<SpielerController>();
            quelle = GetComponent<AudioSource>();
            if (quelle == null) quelle = gameObject.AddComponent<AudioSource>();
            quelle.playOnAwake = false;
            quelle.spatialBlend = 0f;   // eigene Schritte hoert man im Kopf, nicht im Raum

            if (gehKlang == null) gehKlang = Klangwerkstatt.Schritt(false);
            if (sprintKlang == null) sprintKlang = Klangwerkstatt.Schritt(true);
            if (sprungKlang == null) sprungKlang = Klangwerkstatt.Schritt(true);
        }

        void Update()
        {
            Schritte();

            if (Time.time > letzteStaerkeBis) letzteStaerke = 0f;
        }

        void Schritte()
        {
            // Auf den Willen hoeren, nicht auf das Ausrollen: sonst fallen nach
            // dem Loslassen noch ein bis drei Schritte, waehrend der Controller
            // weich abbremst.
            if (!controller.WillLaufen || controller.Tempo < 0.5f)
            {
                naechsterSchritt = 0f;    // beim Anhalten sofort wieder bereit
                return;
            }

            // Nicht die Taste zaehlt, sondern was der Controller tatsaechlich
            // tut. Wer erschoepft ist, laeuft im Gehtempo und darf sich nicht
            // durch Sprintlaerm verraten, den er gar nicht verursacht.
            bool schleicht = controller.IstGeduckt;
            bool rennt = controller.Sprintet;
            if (Time.time < naechsterSchritt) return;

            float takt = rennt ? schrittSprint : schleicht ? schrittSchleichen : schrittGehen;
            float staerke = rennt ? sprinten : schleicht ? schleichen : gehen;
            naechsterSchritt = Time.time + takt;
            Ausloesen(staerke, rennt ? sprintKlang : gehKlang);
        }

        /// Laerm von aussen anmelden — das Haemmern laeuft hier durch, damit
        /// aller Krach des Spielers an einer Stelle zusammenkommt und der
        /// Balken auch wirklich alles zeigt.
        public void Melden(float staerke, AudioClip klang) => Ausloesen(staerke, klang);

        void Ausloesen(float staerke, AudioClip klang)
        {
            Laerm.Machen(transform.position, staerke);
            letzteStaerke = staerke;
            letzteStaerkeBis = Time.time + 0.35f;

            if (klang != null && quelle != null)
            {
                // Leichte Streuung, sonst klingen Schritte wie ein Metronom.
                quelle.pitch = Random.Range(0.9f, 1.1f);
                // Leiser Krach klingt auch leiser.
                float anteil = Mathf.Clamp01(staerke / Mathf.Max(1f, sprinten));
                quelle.PlayOneShot(klang, lautstaerke * Mathf.Max(0.25f, anteil));
            }
        }

        /// Wie weit man gerade zu hoeren waere. Auch im Stehen abfragbar — der
        /// Gizmo soll die Reichweite zeigen, nicht auf den naechsten Schritt warten.
        public float AktuelleReichweite()
        {
            if (controller == null || !controller.WillLaufen || controller.Tempo < 0.5f)
                return 0f;
            return controller.Sprintet ? sprinten
                 : controller.IstGeduckt ? schleichen
                 : gehen;
        }

        void OnDrawGizmos()
        {
            if (!kreisZeigen || !Application.isPlaying) return;

            // Was gerade verursacht wird, und daneben blass der letzte Ausschlag —
            // sonst waere der Hammer nie zu sehen, weil er kein Dauerzustand ist.
            float jetzt = AktuelleReichweite();
            if (jetzt > 0f)
            {
                Gizmos.color = Color.Lerp(new Color(0.3f, 0.9f, 0.3f, 0.9f),
                                          new Color(1f, 0.3f, 0.2f, 0.9f),
                                          Mathf.Clamp01(jetzt / Mathf.Max(1f, balkenMaximum)));
                Welt.Debugformen.Bodenkreis(transform.position, jetzt);
            }

            if (letzteStaerke > jetzt)
            {
                Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.5f);
                Welt.Debugformen.Bodenkreis(transform.position, letzteStaerke);
            }
        }

        void OnGUI()
        {
            if (!balkenZeigen) return;

            // Unter Uhr (8), Leben (34), Ausdauer (64), Eisen (80).
            var style = new GUIStyle(GUI.skin.label) { fontSize = 13 };
            style.normal.textColor = new Color(1f, 1f, 1f, 0.7f);
            GUI.Label(new Rect(12, 108, 200, 20), "Laerm", style);

            var rahmen = new Rect(12, 126, 160, 8);
            var alt = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.5f);
            GUI.DrawTexture(rahmen, Texture2D.whiteTexture);

            // Gegen den lautesten Wert skaliert, damit Sprint den Balken fuellt.
            float anteil = Mathf.Clamp01(letzteStaerke / Mathf.Max(1f, balkenMaximum));
            GUI.color = Color.Lerp(new Color(0.4f, 0.7f, 0.4f),
                                   new Color(0.9f, 0.3f, 0.2f), anteil);
            rahmen.width *= anteil;
            GUI.DrawTexture(rahmen, Texture2D.whiteTexture);
            GUI.color = alt;
        }
    }
}
