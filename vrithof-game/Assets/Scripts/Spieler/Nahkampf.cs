using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Vrithof.Welt;
using Vrithof.Zombies;

namespace Vrithof.Spieler
{
    /// M5b: zuschlagen. Linke Maustaste.
    ///
    /// Nach demselben Muster wie der Zombie-Schlag: ausholen, treffen,
    /// nachschwingen. Waehrend des Ausholens passiert nichts — wer danebenhaut,
    /// steht die ganze Zeit ueber offen da.
    ///
    /// Daraus entsteht das Zomboid-Verhaeltnis von allein, ohne einen einzigen
    /// Balancing-Trick: Gegen einen Zombie reicht die Zeit zwischen zwei
    /// Schlaegen. Gegen zwei nicht mehr — waehrend man den einen faellt, hat
    /// der andere zweimal zugeschlagen.
    ///
    /// Das Design nennt Kampf einen Notausgang, keine Strategie. Deshalb kostet
    /// er Ausdauer, macht Laerm und stumpft die Axt ab.
    ///
    /// Auf den Player legen.
    public class Nahkampf : MonoBehaviour
    {
        [Header("Reichweite")]
        public float reichweite = 2.2f;
        [Tooltip("Oeffnungswinkel des Schlags in Grad. Trifft alles darin — " +
                 "gegen mehrere hilft das, ersetzt aber kein Zielen.")]
        [Range(20f, 180f)]
        public float kegel = 90f;

        [Header("Zeiten")]
        [Tooltip("Sekunden bis der Schlag landet. Waehrenddessen ist man offen.")]
        public float ausholZeit = 0.28f;
        [Tooltip("Sekunden danach, bevor man erneut schlagen kann.")]
        public float nachschwingen = 0.42f;

        [Header("Wirkung")]
        [Tooltip("Schaden mit scharfer Axt. Bei 100 Zombie-Leben sind das zwei Treffer.")]
        public float schadenMitAxt = 55f;
        [Tooltip("Schaden mit blossen Haenden oder stumpfer Axt. Bewusst kaum " +
                 "brauchbar — Kampf ohne Werkzeug soll die schlechte Wahl sein.")]
        public float schadenOhneAxt = 12f;
        [Tooltip("Ausdauer pro Schlag. Wer sich verausgabt, kann nicht mehr " +
                 "zuschlagen und auch nicht mehr weglaufen.")]
        public float ausdauerKosten = 25f;
        [Tooltip("Wie weit ein Schlag zu hoeren ist. Zwischen Gehen und Sprinten.")]
        public float laerm = 18f;
        [Tooltip("Tempo waehrend eines Schlags, als Anteil des normalen. Ohne " +
                 "diese Bremse kann man rueckwaerts gehen und dabei zuschlagen, " +
                 "und der Kampf kostet nichts.")]
        [Range(0.1f, 1f)]
        public float tempoBeimSchlagen = 0.35f;

        [Header("Klang")]
        [Tooltip("Leer lassen — dann wird ein Platzhalter erzeugt.")]
        public AudioClip hiebKlang;
        [Range(0f, 1f)] public float lautstaerke = 0.5f;

        [Header("Anzeige")]
        public bool hinweisZeigen = true;

        public bool SchlaegtGerade { get; private set; }

        Ausdauer kraft;
        Axt axt;
        SpielerLaerm geraeusch;
        Schlafen schlaf;
        SpielerController controller;
        Camera blick;
        AudioSource quelle;
        float rueckmeldungBis;
        string rueckmeldung = "";
        // Erst sammeln, dann zuschlagen. Ein toedlicher Treffer meldet den
        // Zombie aus Zombie.Alle ab — mitten in einer foreach-Schleife ueber
        // genau diese Liste wirft das eine Exception.
        readonly List<ZombieLeben> getroffene = new List<ZombieLeben>();

        void Awake()
        {
            kraft = GetComponent<Ausdauer>();
            axt = GetComponent<Axt>();
            geraeusch = GetComponent<SpielerLaerm>();
            schlaf = GetComponent<Schlafen>();
            controller = GetComponent<SpielerController>();

            if (hiebKlang == null) hiebKlang = Klangwerkstatt.Hieb();
            quelle = gameObject.AddComponent<AudioSource>();
            quelle.playOnAwake = false;
            quelle.spatialBlend = 0f;
        }

        void OnDisable()
        {
            if (controller != null) controller.tempoDaempfer = 1f;
        }

        void Update()
        {
            if (blick == null) blick = Camera.main;
            if (SchlaegtGerade || Mouse.current == null) return;
            if (schlaf != null && schlaf.Schlaeft) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            if (kraft != null && !kraft.Reicht(ausdauerKosten))
            {
                Melden("zu erschöpft");
                return;
            }

            StartCoroutine(Schlagen());
        }

        System.Collections.IEnumerator Schlagen()
        {
            SchlaegtGerade = true;
            if (kraft != null) kraft.Verbrauchen(ausdauerKosten);
            if (controller != null) controller.tempoDaempfer = tempoBeimSchlagen;

            // Der Krach entsteht beim Ausholen, nicht beim Treffer: das Rauschen
            // der Klinge ist da, auch wenn man danebenhaut.
            if (geraeusch != null) geraeusch.Melden(laerm, hiebKlang);
            else if (quelle != null && hiebKlang != null) quelle.PlayOneShot(hiebKlang, lautstaerke);

            yield return new WaitForSeconds(ausholZeit);

            Treffen();

            yield return new WaitForSeconds(nachschwingen);
            if (controller != null) controller.tempoDaempfer = 1f;
            SchlaegtGerade = false;
        }

        void Treffen()
        {
            bool scharf = axt != null && axt.IstScharf;
            float schaden = scharf ? schadenMitAxt : schadenOhneAxt;

            Vector3 auge = blick != null ? blick.transform.position : transform.position;
            Vector3 richtung = blick != null ? blick.transform.forward : transform.forward;
            richtung.y = 0f;
            richtung.Normalize();

            getroffene.Clear();

            // Ueber das Register statt ueber Physik: die Zombie-Collider sind
            // Kapseln, und ein Kegel trifft naeher an dem, was man sieht.
            foreach (var z in Zombie.Alle)
            {
                if (z == null) continue;

                var leben = z.GetComponent<ZombieLeben>();
                if (leben == null || leben.Tot) continue;

                Vector3 hin = z.transform.position - transform.position;
                hin.y = 0f;
                if (hin.sqrMagnitude > reichweite * reichweite) continue;
                if (Vector3.Angle(richtung, hin) > kegel * 0.5f) continue;

                // Nicht durch Waende schlagen.
                Vector3 brust = z.transform.position + Vector3.up * 1f;
                if (Physics.Linecast(auge, brust, out var sperre, ~0,
                                     QueryTriggerInteraction.Ignore)
                    && sperre.collider.GetComponentInParent<Zombie>() != z)
                    continue;

                getroffene.Add(leben);
            }

            // Erst hier Schaden austeilen: waehrend der Schleife wuerde ein
            // toedlicher Treffer die Liste veraendern, ueber die wir gerade
            // laufen.
            foreach (var leben in getroffene)
            {
                Vector3 hin = leben.transform.position - transform.position;
                hin.y = 0f;
                leben.Schaden(schaden, hin.normalized);
            }

            int getroffen = getroffene.Count;
            if (getroffen == 0) return;

            // Die Klinge nimmt es uebel — dieselbe Abnutzung wie beim Aufbrechen.
            if (scharf) axt.Abnutzen();
            Melden(getroffen == 1 ? "getroffen" : $"{getroffen} getroffen");
        }

        void Melden(string text)
        {
            rueckmeldung = text;
            rueckmeldungBis = Time.time + 1.2f;
        }

        void OnGUI()
        {
            if (!hinweisZeigen || Time.time > rueckmeldungBis) return;

            var stil = new GUIStyle(GUI.skin.label)
            { fontSize = 16, alignment = TextAnchor.MiddleCenter };
            stil.normal.textColor = new Color(1f, 0.85f, 0.6f);
            GUI.Label(new Rect(0, Screen.height * 0.5f + 70, Screen.width, 24),
                      rueckmeldung, stil);
        }
    }
}
