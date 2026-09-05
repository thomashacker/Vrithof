using UnityEngine;
using Vrithof.Worldbuilding;
using Vrithof.Zombies;

namespace Vrithof.Spieler
{
    /// M5: die Nacht durchschlafen — wenn man kann.
    ///
    /// Ohne das ist eine sichere Nacht schlicht langweilig: fuenf Minuten
    /// dasitzen und warten. Mit dem Schlafen wird daraus eine Entscheidung, und
    /// die Barrikaden bekommen endlich einen Grund. Man baut sie nicht, um zu
    /// bauen, sondern um schlafen zu koennen.
    ///
    /// Die Zeit wird geraffft, nicht uebersprungen: die Welt laeuft mit, Zombies
    /// kommen wirklich naeher. Deshalb ist Schlafen kein Freifahrtschein — wer
    /// sich nicht gesichert hat, wacht mitten in der Nacht auf, und dann steht
    /// schon jemand vor der Tuer.
    ///
    /// Auf den Player legen.
    public class Schlafen : MonoBehaviour
    {
        [Header("Aufwachen")]
        [Tooltip("Erholt sich die Ausdauer beim Aufwachen vollstaendig?")]
        public bool ausgeruhtAufwachen = true;

        [Header("Anzeige")]
        public bool schleierZeigen = true;

        public bool Schlaeft { get; private set; }

        Bett bett;
        Zeit.TageszeitZyklus zyklus;
        Ausdauer kraft;
        SpielerController controller;
        string weckGrund = "";
        float weckGrundBis;

        void Awake()
        {
            kraft = GetComponent<Ausdauer>();
            controller = GetComponent<SpielerController>();
        }

        void Start()
        {
            zyklus = FindAnyObjectByType<Zeit.TageszeitZyklus>();
        }

        /// Kann an diesem Bett gerade geschlafen werden? Nur nachts — tagsueber
        /// wuerde man sofort wieder aufwachen, und das waere nur verwirrend.
        public bool KannSchlafen(Bett b)
        {
            return b != null && !Schlaeft && zyklus != null && zyklus.IstNacht;
        }

        public void Einschlafen(Bett b)
        {
            if (!KannSchlafen(b)) return;

            bett = b;
            Schlaeft = true;
            Time.timeScale = b.zeitraffer;
            if (controller != null) controller.enabled = false;
        }

        void Update()
        {
            if (Time.time > weckGrundBis) weckGrund = "";
            if (!Schlaeft) return;

            if (zyklus == null || !zyklus.IstNacht)
            {
                Aufwachen("Es wird hell.");
                return;
            }

            var stoerer = NaechsterZombie();
            if (stoerer < bett.weckDistanz)
                Aufwachen("Etwas ist nah!");
        }

        float NaechsterZombie()
        {
            float kuerzeste = float.MaxValue;
            foreach (var z in Zombie.Alle)
            {
                if (z == null) continue;
                float d = (z.transform.position - transform.position).sqrMagnitude;
                if (d < kuerzeste) kuerzeste = d;
            }
            return kuerzeste == float.MaxValue ? float.MaxValue : Mathf.Sqrt(kuerzeste);
        }

        void Aufwachen(string grund)
        {
            Schlaeft = false;
            Time.timeScale = 1f;
            if (controller != null) controller.enabled = true;

            if (ausgeruhtAufwachen && kraft != null)
                kraft.Auffuellen();

            weckGrund = grund;
            weckGrundBis = Time.time + 3f;
        }

        // Bei Szenenwechsel oder Tod darf die Zeit nicht geraffft stehenbleiben.
        void OnDisable()
        {
            if (Schlaeft) Time.timeScale = 1f;
        }

        void OnGUI()
        {
            if (!schleierZeigen) return;

            if (Schlaeft)
            {
                var alt = GUI.color;
                GUI.color = new Color(0f, 0f, 0f, 0.88f);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height),
                                Texture2D.whiteTexture);
                GUI.color = alt;

                var stil = new GUIStyle(GUI.skin.label)
                { fontSize = 22, alignment = TextAnchor.MiddleCenter };
                stil.normal.textColor = new Color(0.8f, 0.8f, 0.85f);
                GUI.Label(new Rect(0, Screen.height * 0.5f - 20, Screen.width, 40),
                          "Du schläfst …", stil);
            }

            if (weckGrund == "") return;

            var w = new GUIStyle(GUI.skin.label)
            { fontSize = 20, alignment = TextAnchor.MiddleCenter };
            w.normal.textColor = new Color(1f, 0.75f, 0.4f);
            GUI.Label(new Rect(0, Screen.height * 0.35f, Screen.width, 30), weckGrund, w);
        }
    }
}
