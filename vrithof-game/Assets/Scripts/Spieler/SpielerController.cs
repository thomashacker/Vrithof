using System;
using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

namespace Vrithof.Spieler
{
    /// Eigener First-Person-Controller. Ersetzt den aus den Starter Assets und
    /// die Schleichen-Komponente.
    ///
    /// Der Grund ist nicht Geschmack, sondern dass sich drei Umwege angesammelt
    /// hatten: die Ausdauer hat heimlich SprintSpeed verstellt, das Schleichen
    /// MoveSpeed, und die Sprungsperre hat die Taste geloescht, bevor der fremde
    /// Controller sie liest — in einer Reihenfolge, die Unity nicht garantiert.
    /// Hier gibt es dafuer jetzt Schalter: 'sprintErlaubt', 'sprungErlaubt' und
    /// das Ereignis 'Gesprungen'.
    ///
    /// Bodenkontakt kommt von CharacterController.isGrounded statt von einer
    /// eigenen Kugelabfrage. Das braucht keine LayerMask und kann deshalb auch
    /// nicht dadurch kaputtgehen, dass irgendwo ein Layer umgestellt wird.
    ///
    /// Die Eingabe kommt weiter von StarterAssetsInputs — das Skript haengt am
    /// Input-Action-Asset und funktioniert; es neu zu bauen waere Arbeit ohne
    /// Ertrag.
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(StarterAssetsInputs))]
    public class SpielerController : MonoBehaviour
    {
        [Header("Tempo")]
        public float gehTempo = 4f;
        public float sprintTempo = 6f;
        public float duckTempo = 1.6f;
        [Tooltip("Wie schnell Tempo auf- und abgebaut wird. Niedriger heisst " +
                 "traeger — das ist die Schraube fuer spaeteres Gewicht.")]
        public float beschleunigung = 10f;

        [Header("Springen")]
        public float sprungHoehe = 1.2f;
        public float schwerkraft = -15f;
        [Tooltip("Sekunden nach der Landung, bevor wieder gesprungen werden kann.")]
        public float sprungPause = 0.1f;

        [Header("Ducken")]
        [Tooltip("Wird beim Start aus dem CharacterController uebernommen — " +
                 "der Wert hier ist nur ein Platzhalter.")]
        public float standHoehe = 2f;
        public float duckHoehe = 1.2f;
        [Tooltip("Wie schnell hoch und runter.")]
        public float duckWechsel = 9f;

        [Header("Kamera")]
        [Tooltip("Das Objekt, dem Cinemachine folgt. Wird um X gedreht, der " +
                 "Spieler selbst um Y.")]
        public GameObject kameraZiel;
        public float empfindlichkeit = 1f;
        public float obenGrenze = 89f;
        public float untenGrenze = -89f;

        /// Faktor auf das Tempo, den andere setzen duerfen — der Nahkampf
        /// bremst damit waehrend eines Schlags. 1 heisst unveraendert.
        [NonSerialized] public float tempoDaempfer = 1f;

        /// Von der Ausdauer gesetzt. Kein Herumschrauben an fremden Werten mehr.
        [NonSerialized] public bool sprintErlaubt = true;
        [NonSerialized] public bool sprungErlaubt = true;

        /// Feuert genau dann, wenn wirklich abgesprungen wurde — daran haengt
        /// die Ausdauer, statt den Absprung an der Velocity zu erraten.
        public event Action Gesprungen;

        /// Liegt gerade Bewegungs-Eingabe an? Nicht dasselbe wie Tempo > 0:
        /// nach dem Loslassen rollt man noch aus. Wer daran haengt (die
        /// Schrittgeraeusche tun es), soll beim Loslassen sofort aufhoeren.
        public bool WillLaufen { get; private set; }

        public bool AufDemBoden { get; private set; }
        public bool IstGeduckt { get; private set; }
        public bool Sprintet { get; private set; }
        /// Waagerechtes Tempo, das gerade tatsaechlich anliegt.
        public float Tempo { get; private set; }

        CharacterController koerper;
        StarterAssetsInputs eingabe;
        float fallTempo;
        float blickX;
        float sprungBereitAb;
        float standKameraY;

        void Awake()
        {
            koerper = GetComponent<CharacterController>();
            eingabe = GetComponent<StarterAssetsInputs>();
            standHoehe = koerper.height;
            if (kameraZiel != null) standKameraY = kameraZiel.transform.localPosition.y;
        }

        void Update()
        {
            Ducken();
            Bewegen();
            Fallen();
        }

        void LateUpdate()
        {
            Umsehen();
        }

        void Ducken()
        {
            // Sprinten hebt das Ducken auf — beides gleichzeitig ergibt kein Tempo,
            // das man erklaeren koennte.
            bool will = Keyboard.current != null
                        && Keyboard.current.leftCtrlKey.isPressed
                        && !eingabe.sprint;
            IstGeduckt = will;

            float ziel = will ? duckHoehe : standHoehe;
            float h = Mathf.Lerp(koerper.height, ziel, Time.deltaTime * duckWechsel);
            koerper.height = h;
            // Fuesse bleiben am Boden: der Mittelpunkt sitzt auf halber Hoehe.
            koerper.center = new Vector3(koerper.center.x, h * 0.5f, koerper.center.z);

            // Der Blick muss mit. Ohne das schrumpft nur der Collider, und es
            // fuehlt sich an, als wuerde man sich gar nicht ducken.
            if (kameraZiel != null)
            {
                var p = kameraZiel.transform.localPosition;
                kameraZiel.transform.localPosition =
                    new Vector3(p.x, standKameraY - (standHoehe - h), p.z);
            }
        }

        void Bewegen()
        {
            WillLaufen = eingabe.move != Vector2.zero;
            Sprintet = eingabe.sprint && sprintErlaubt && !IstGeduckt && WillLaufen;

            float zielTempo = !WillLaufen ? 0f
                            : IstGeduckt ? duckTempo
                            : Sprintet ? sprintTempo
                            : gehTempo;

            // Auf dem eigenen Wert aufbauen, nicht auf der gemessenen Geschwindigkeit:
            // an einer Wand ist die naemlich fast null, und der Spieler wuerde
            // an ihr entlangkriechen statt zu gleiten.
            zielTempo *= Mathf.Clamp01(tempoDaempfer);
            Tempo = Mathf.Lerp(Tempo, zielTempo, Time.deltaTime * beschleunigung);
            if (Tempo < 0.01f) Tempo = 0f;

            Vector3 richtung = transform.right * eingabe.move.x
                             + transform.forward * eingabe.move.y;
            if (richtung.sqrMagnitude > 1f) richtung.Normalize();

            koerper.Move(richtung * (Tempo * Time.deltaTime)
                         + Vector3.up * (fallTempo * Time.deltaTime));
        }

        void Fallen()
        {
            AufDemBoden = koerper.isGrounded;

            if (AufDemBoden)
            {
                // Leicht nach unten gedrueckt halten, sonst flackert isGrounded.
                if (fallTempo < 0f) fallTempo = -2f;

                if (eingabe.jump && sprungErlaubt && Time.time >= sprungBereitAb)
                {
                    fallTempo = Mathf.Sqrt(sprungHoehe * -2f * schwerkraft);
                    sprungBereitAb = Time.time + sprungPause;
                    eingabe.jump = false;
                    Gesprungen?.Invoke();
                }
            }
            else
            {
                eingabe.jump = false;   // in der Luft verfaellt der Wunsch
                fallTempo += schwerkraft * Time.deltaTime;
            }
        }

        void Umsehen()
        {
            if (eingabe.look.sqrMagnitude < 0.0001f) return;

            transform.Rotate(Vector3.up * (eingabe.look.x * empfindlichkeit));

            blickX = Mathf.Clamp(blickX + eingabe.look.y * empfindlichkeit,
                                 untenGrenze, obenGrenze);
            if (kameraZiel != null)
                kameraZiel.transform.localRotation = Quaternion.Euler(blickX, 0f, 0f);
        }
    }
}
