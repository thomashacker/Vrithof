using UnityEngine;

namespace Vrithof.Spieler
{
    /// M2: Sprinten kostet Ausdauer. Sie kommt nur zurueck, wenn du wirklich
    /// stehen bleibst — im Gehen nur ein Rinnsal, im Sprint gar nicht.
    ///
    /// Das ist der Zomboid-Ansatz und der Grund, warum Ausdauer hier kein
    /// Wartebalken ist: Ausruhen kostet Tageszeit. Damit haengt die Ausdauer
    /// an der Uhr statt an sich selbst.
    ///
    /// Der Controller wird nicht mehr heimlich verstellt. Er hat dafuer die
    /// Schalter 'sprintErlaubt' und 'sprungErlaubt' und meldet einen Absprung
    /// ueber das Ereignis 'Gesprungen'. Frueher wurde stattdessen SprintSpeed
    /// verbogen und die Sprungtaste geloescht — beides hing an einer
    /// Ausfuehrungsreihenfolge, die Unity nicht garantiert.
    ///
    /// Auf den Player legen.
    [RequireComponent(typeof(SpielerController))]
    public class Ausdauer : MonoBehaviour
    {
        [Header("Ausdauer")]
        public float maxAusdauer = 100f;
        [Tooltip("Verbrauch pro Sekunde Sprint. 20 = fuenf Sekunden am Stueck.")]
        public float verbrauchProSekunde = 20f;

        [Header("Einzelne Anstrengungen")]
        [Tooltip("Ausdauer pro Sprung. Bei leerem Balken geht kein Sprung mehr.")]
        public float sprungKosten = 15f;
        [Tooltip("Ausdauer fuers Durchsteigen einer Oeffnung. Teurer als ein " +
                 "Sprung — sich hochzuziehen ist anstrengender als abzuspringen.")]
        public float kletterKosten = 22f;

        [Header("Erholung")]
        [Tooltip("Regeneration pro Sekunde im Stillstand. Das ist der einzige " +
                 "Weg, ernsthaft aufzutanken — und er kostet Tageszeit.")]
        public float ruheRegeneration = 15f;
        [Tooltip("Regeneration pro Sekunde im Gehen. Bewusst ein Rinnsal. " +
                 "Auf 0 setzen, wenn nur echtes Stehenbleiben zaehlen soll.")]
        public float gehRegeneration = 2f;
        [Tooltip("Nach dem Leerlaufen erst ab diesem Anteil wieder sprintbar. " +
                 "Verhindert Stotter-Sprint in Mikro-Schueben.")]
        [Range(0f, 1f)]
        public float erholungsSchwelle = 0.25f;

        [Header("Anzeige")]
        public bool balkenZeigen = true;

        float ausdauer;
        bool erschoepft;
        SpielerController controller;

        /// Ist der Balken leer? Wer erschoepft ist, laeuft im Gehtempo — und
        /// muss dann auch nur so laut sein.
        public bool Erschoepft => erschoepft;

        void Awake()
        {
            controller = GetComponent<SpielerController>();
            ausdauer = maxAusdauer;
        }

        void OnEnable()
        {
            if (controller == null) controller = GetComponent<SpielerController>();
            controller.Gesprungen += Absprung;
        }

        void OnDisable()
        {
            if (controller != null) controller.Gesprungen -= Absprung;
        }

        void Absprung() => Verbrauchen(sprungKosten);

        void Update()
        {
            // Der Controller sagt selbst, ob wirklich gerannt wird — geduckt,
            // gesperrt oder stehend zaehlt nicht.
            if (controller.Sprintet && !erschoepft)
            {
                ausdauer -= verbrauchProSekunde * Time.deltaTime;
                if (ausdauer <= 0f)
                {
                    ausdauer = 0f;
                    erschoepft = true;
                }
            }
            else
            {
                // Stillstand tankt auf, Gehen haelt kaum mit, Sprint gibt nichts.
                float rate = controller.Tempo > 0.1f ? gehRegeneration : ruheRegeneration;
                ausdauer = Mathf.Min(maxAusdauer, ausdauer + rate * Time.deltaTime);
                if (erschoepft && ausdauer >= maxAusdauer * erholungsSchwelle)
                    erschoepft = false;
            }

            // Statt fremde Werte zu verbiegen: zwei Schalter.
            controller.sprintErlaubt = !erschoepft;
            controller.sprungErlaubt = Reicht(sprungKosten);
        }

        /// Randvoll — nach einer durchschlafenen Nacht.
        public void Auffuellen()
        {
            ausdauer = maxAusdauer;
            erschoepft = false;
        }

        /// Reicht die Ausdauer fuer eine einzelne Anstrengung?
        public bool Reicht(float menge) => !erschoepft && ausdauer >= menge;

        /// Eine Anstrengung abbuchen. Gibt false zurueck, wenn es nicht reicht —
        /// dann findet sie nicht statt.
        public bool Verbrauchen(float menge)
        {
            if (!Reicht(menge)) return false;
            ausdauer -= menge;
            if (ausdauer <= 0f)
            {
                ausdauer = 0f;
                erschoepft = true;
            }
            return true;
        }

        void OnGUI()
        {
            if (!balkenZeigen) return;
            // Unter Uhr (y=8) und Leben (y=34).
            var rahmen = new Rect(12, 64, 160, 10);
            var alt = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.5f);
            GUI.DrawTexture(rahmen, Texture2D.whiteTexture);
            GUI.color = erschoepft ? new Color(0.6f, 0.3f, 0.2f) : new Color(0.85f, 0.8f, 0.5f);
            rahmen.width *= ausdauer / maxAusdauer;
            GUI.DrawTexture(rahmen, Texture2D.whiteTexture);
            GUI.color = alt;
        }
    }
}
