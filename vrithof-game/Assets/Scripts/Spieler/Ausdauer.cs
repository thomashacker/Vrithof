using UnityEngine;
using StarterAssets;

namespace Vrithof.Spieler
{
    /// M2: Sprinten kostet Ausdauer. Sie kommt nur zurueck, wenn du wirklich
    /// stehen bleibst — im Gehen nur ein Rinnsal, im Sprint gar nicht.
    ///
    /// Das ist der Zomboid-Ansatz und der Grund, warum Ausdauer hier kein
    /// Wartebalken ist: Ausruhen kostet Tageszeit. Damit haengt die Ausdauer
    /// an der Uhr statt an sich selbst.
    ///
    /// Der FirstPersonController wird bewusst nicht angefasst (Starter Asset).
    /// Stattdessen wird bei Erschoepfung sein SprintSpeed auf MoveSpeed gesetzt —
    /// er merkt davon nichts, und die Sprint-Taste bleibt unberuehrt. Haelt der
    /// Spieler sie gedrueckt, laeuft er von selbst wieder los, sobald es reicht.
    ///
    /// Auf den Player legen (dort, wo auch FirstPersonController haengt).
    [RequireComponent(typeof(FirstPersonController))]
    [RequireComponent(typeof(StarterAssetsInputs))]
    [RequireComponent(typeof(CharacterController))]
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

        FirstPersonController fpc;
        StarterAssetsInputs eingabe;
        CharacterController koerper;
        float vollesTempo;
        bool warAmBoden = true;

        void Awake()
        {
            fpc = GetComponent<FirstPersonController>();
            eingabe = GetComponent<StarterAssetsInputs>();
            koerper = GetComponent<CharacterController>();
            vollesTempo = fpc.SprintSpeed;   // Inspector-Wert merken, wird gleich manipuliert
            ausdauer = maxAusdauer;
        }

        void Update()
        {
            bool bewegtSich = eingabe.move != Vector2.zero;
            bool sprintet = bewegtSich && eingabe.sprint && !erschoepft;

            if (sprintet)
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
                float rate = bewegtSich ? gehRegeneration : ruheRegeneration;
                ausdauer = Mathf.Min(maxAusdauer, ausdauer + rate * Time.deltaTime);
                if (erschoepft && ausdauer >= maxAusdauer * erholungsSchwelle)
                    erschoepft = false;
            }

            fpc.SprintSpeed = erschoepft ? fpc.MoveSpeed : vollesTempo;

            Sprung();
        }

        /// Ist der Balken leer? Wer erschoepft ist, laeuft im Gehtempo — und
        /// muss dann auch nur so laut sein.
        public bool Erschoepft => erschoepft;

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

        // Der Sprung selbst steckt im Starter-Asset-Controller, den wir nicht
        // anfassen. Also wird er von aussen erkannt: wer den Boden verlaesst und
        // dabei nach oben faehrt, ist gesprungen — wer faellt, nicht.
        //
        // Reicht die Ausdauer nicht, wird die Sprungtaste geloescht, bevor der
        // Controller sie liest. Bei ungluecklicher Ausfuehrungsreihenfolge kann
        // dabei ein einzelner Sprung durchrutschen; das ist es wert, den
        // Controller in Ruhe zu lassen.
        void Sprung()
        {
            if (eingabe.jump && !Reicht(sprungKosten))
                eingabe.jump = false;

            bool amBoden = fpc.Grounded;
            if (warAmBoden && !amBoden && koerper.velocity.y > 0.1f)
                Verbrauchen(sprungKosten);
            warAmBoden = amBoden;
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
