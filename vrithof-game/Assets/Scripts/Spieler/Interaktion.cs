using UnityEngine;
using UnityEngine.InputSystem;
using Vrithof.Welt;
using Vrithof.Worldbuilding;

namespace Vrithof.Spieler
{
    /// M3: Raycast aus der Kamera, E *halten* -> Brett nageln, Eisen weg.
    ///
    /// Gehalten statt gedrueckt, und das ist kein Detail: waehrend du naegelst,
    /// bist du gebunden. Nachts heisst das, du entscheidest dich fuer eine
    /// Oeffnung und bist fuer die Dauer blind fuer die anderen. Loslassen oder
    /// wegsehen bricht ab — angefangene Arbeit ist verloren.
    ///
    /// Dasselbe E repariert auch: eine angeschlagene Barrikade auszubessern ist
    /// derselbe Handgriff wie sie zu bauen.
    ///
    /// Die Taste wird direkt abgefragt statt ueber eine Input-Action. Rebinding
    /// wird in diesem Projekt nicht getestet, also braucht es dafuer auch kein
    /// Action-Asset.
    ///
    /// Auf den Player legen.
    [RequireComponent(typeof(EisenVorrat))]
    public class Interaktion : MonoBehaviour
    {
        [Tooltip("Wie weit man langt. Armlaenge plus etwas.")]
        public float reichweite = 3f;
        [Tooltip("Sekunden Haemmern pro Brett. Der Preis ist Zeit, nicht nur Eisen.")]
        public float bauZeit = 1.5f;
        [Tooltip("Sekunden fuer ein Brett wieder ab. Schneller als nageln — " +
                 "aber im Zweifel immer noch zu langsam, wenn schon jemand davorsteht.")]
        public float abbauZeit = 1f;

        [Header("Durchsteigen")]
        [Tooltip("Wie weit man von der Oeffnung entfernt sein darf, um " +
                 "hindurchzusteigen.")]
        public float steigReichweite = 2f;
        [Tooltip("Wie lange das Durchsteigen dauert. Waehrend dessen ist man " +
                 "wehrlos — ein Fluchtweg soll etwas kosten.")]
        public float steigDauer = 0.55f;
        [Tooltip("Wie weit hinter der Wand man landet.")]
        public float steigAbstand = 1.3f;
        [Tooltip("Wie weit das Hinueberklettern zu hoeren ist.")]
        public float steigLaerm = 12f;

        [Header("Laerm")]
        [Tooltip("Wie weit das Haemmern zu hoeren ist. Absichtlich lauter als " +
                 "Sprinten: wer eine Wand repariert, zieht die Horde genau dorthin.")]
        public float haemmerLaerm = 28f;
        [Tooltip("Sekunden zwischen zwei Hammerschlaegen.")]
        public float schlagIntervall = 0.4f;
        [Tooltip("Leer lassen — dann wird ein Platzhalter erzeugt.")]
        public AudioClip hammerKlang;
        [Range(0f, 1f)] public float lautstaerke = 0.5f;

        [Header("Anzeige")]
        public bool fadenkreuzZeigen = true;
        [Tooltip("Schreibt oben rechts hin, was der Strahl trifft. Zum Einrichten.")]
        public bool diagnose = true;

        EisenVorrat vorrat;
        NahrungsVorrat magen;
        Fackel fackel;
        Ausdauer kraft;
        Camera blick;
        Openable imVisier;
        LootBehaelter truheImVisier;
        string beuteText = "";
        float beuteBis;
        float fortschritt;
        string letzterTreffer = "-";
        float naechsterSchlag;
        AudioSource quelle;
        SpielerLaerm laerm;
        CharacterController koerper;
        bool steigtGerade;

        void Awake()
        {
            vorrat = GetComponent<EisenVorrat>();
            laerm = GetComponent<SpielerLaerm>();
            magen = GetComponent<NahrungsVorrat>();
            fackel = GetComponent<Fackel>();
            kraft = GetComponent<Ausdauer>();
            koerper = GetComponent<CharacterController>();
            quelle = GetComponent<AudioSource>();
            if (quelle == null) quelle = gameObject.AddComponent<AudioSource>();
            quelle.playOnAwake = false;
            quelle.spatialBlend = 0f;
            if (hammerKlang == null) hammerKlang = Klangwerkstatt.Hammer();
        }

        void Update()
        {
            if (blick == null) blick = Camera.main;
            if (blick == null || steigtGerade) return;

            var vorherOeffnung = imVisier;
            var vorherTruhe = truheImVisier;
            Anvisieren();

            if (imVisier != vorherOeffnung)
            {
                if (vorherOeffnung) vorherOeffnung.Hervorheben(false);
                if (imVisier) imVisier.Hervorheben(true);
                fortschritt = 0f;   // Ziel gewechselt -> von vorn
            }
            if (truheImVisier != vorherTruhe) fortschritt = 0f;

            if (truheImVisier != null) Durchsuchen();
            else Haemmern();

            if (imVisier != null && KannDurchsteigen(imVisier)
                && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
                StartCoroutine(Durchsteigen(imVisier));
        }

        // Nur durch offene Oeffnungen — Bretter versperren den Weg genauso wie
        // ein heiles Tuerblatt. Und nur mit Kraft in den Armen: erschoepft
        // kommt man nicht mehr durchs Fenster, sondern muss zur Tuer.
        bool KannDurchsteigen(Openable o)
        {
            if (!o.IstOffen) return false;
            if (kraft != null && !kraft.Reicht(kraft.kletterKosten)) return false;
            Vector3 d = o.transform.position - transform.position;
            d.y = 0f;
            return d.sqrMagnitude <= steigReichweite * steigReichweite;
        }

        // Der CharacterController ist zwei Meter hoch und passt durch keine
        // Fensteroeffnung. Statt einer Kletter-Mechanik wird er kurz abgeschaltet
        // und hinuebergeschoben — waehrenddessen kann man nichts tun.
        System.Collections.IEnumerator Durchsteigen(Openable o)
        {
            steigtGerade = true;
            if (kraft != null) kraft.Verbrauchen(kraft.kletterKosten);
            if (laerm != null) laerm.Melden(steigLaerm, null);

            // Auf die Seite, die vom Spieler weg zeigt.
            Vector3 achse = o.transform.forward;
            if (Vector3.Dot(transform.position - o.transform.position, achse) > 0f)
                achse = -achse;

            Vector3 start = transform.position;
            Vector3 ende = o.transform.position + achse * steigAbstand;
            ende.y = start.y;   // flacher Boden auf beiden Seiten

            if (koerper != null) koerper.enabled = false;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.05f, steigDauer);
                transform.position = Vector3.Lerp(start, ende, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            if (koerper != null) koerper.enabled = true;
            steigtGerade = false;
        }

        void Haemmern()
        {
            if (imVisier == null || Keyboard.current == null)
            {
                fortschritt = 0f;
                return;
            }

            bool nageln = Keyboard.current.eKey.isPressed && Machbar(imVisier);
            bool reissen = Keyboard.current.qKey.isPressed && imVisier.KannAbbauen;

            // Beides gleichzeitig ergibt keinen Sinn — Nageln gewinnt.
            if (!nageln && !reissen)
            {
                fortschritt = 0f;
                return;
            }

            float dauer = nageln ? bauZeit : abbauZeit;
            Haemmerschlag();
            fortschritt += Time.deltaTime;
            if (fortschritt < dauer) return;

            fortschritt = 0f;
            if (nageln)
            {
                int kosten = imVisier.NaechsteKosten;
                if (imVisier.Verstaerken())
                    vorrat.Abziehen(kosten);
            }
            else if (imVisier.Abbauen())
            {
                vorrat.eisen += imVisier.eisenZurueck;
            }
        }

        // Jeder Schlag ist ein eigenes Geraeusch. Deshalb ist Arbeit an der
        // Barrikade nie umsonst: du sicherst die Nordwand und ziehst dadurch
        // die Horde zur Nordwand.
        void Haemmerschlag()
        {
            if (Time.time < naechsterSchlag) return;
            naechsterSchlag = Time.time + schlagIntervall;

            // Ueber SpielerLaerm, damit der Balken den Hammer mitbekommt.
            if (laerm != null)
            {
                laerm.Melden(haemmerLaerm, hammerKlang);
                return;
            }

            Laerm.Machen(transform.position, haemmerLaerm);
            if (hammerKlang != null && quelle != null)
            {
                quelle.pitch = Random.Range(0.92f, 1.08f);
                quelle.PlayOneShot(hammerKlang, lautstaerke);
            }
        }

        // Lohnt sich das Haemmern hier ueberhaupt?
        bool Machbar(Openable o)
        {
            return o.BrauchtArbeit && vorrat != null && vorrat.Reicht(o.NaechsteKosten);
        }

        void Anvisieren()
        {
            imVisier = null;
            truheImVisier = null;

            var strahl = new Ray(blick.transform.position, blick.transform.forward);
            if (!Physics.Raycast(strahl, out var treffer, reichweite,
                                 ~0, QueryTriggerInteraction.Collide))
            {
                letzterTreffer = "nichts in Reichweite";
                return;
            }

            // Trifft der Strahl ein Brett, sitzt das Openable am Elternobjekt.
            imVisier = treffer.collider.GetComponentInParent<Openable>();
            truheImVisier = treffer.collider.GetComponentInParent<LootBehaelter>();

            string was = imVisier != null ? " — Openable"
                       : truheImVisier != null ? " — Truhe"
                       : "";
            letzterTreffer = $"{treffer.collider.name} ({treffer.distance:0.0} m){was}";
        }

        // Wuehlen ist derselbe Handgriff wie Haemmern: halten, warten, gebunden
        // sein. Nur dass hier etwas herauskommt statt hineingeht.
        void Durchsuchen()
        {
            if (truheImVisier.Gepluendert
                || Keyboard.current == null || !Keyboard.current.eKey.isPressed)
            {
                fortschritt = 0f;
                return;
            }

            if (laerm != null && Time.time >= naechsterSchlag)
            {
                naechsterSchlag = Time.time + schlagIntervall;
                laerm.Melden(truheImVisier.suchLaerm, null);
            }

            fortschritt += Time.deltaTime;
            if (fortschritt < truheImVisier.suchZeit) return;

            fortschritt = 0f;
            truheImVisier.Ausraeumen(out int eisenBeute, out int nahrungBeute,
                                     out int fackelBeute);

            if (vorrat != null) vorrat.eisen += eisenBeute;
            if (magen != null) magen.Essen(nahrungBeute);
            if (fackel != null) fackel.vorrat += fackelBeute;

            beuteText = eisenBeute == 0 && nahrungBeute == 0 && fackelBeute == 0
                ? "nichts drin"
                : $"+{eisenBeute} Eisen   +{nahrungBeute} Nahrung" +
                  (fackelBeute > 0 ? $"   +{fackelBeute} Fackel" : "");
            beuteBis = Time.time + 2.5f;
        }

        void OnDisable()
        {
            if (imVisier) imVisier.Hervorheben(false);
        }

        void OnGUI()
        {
            if (diagnose)
            {
                var d = new GUIStyle(GUI.skin.label) { fontSize = 14 };
                d.normal.textColor = Color.yellow;
                string kamera = blick == null ? "KEINE Camera.main!" : blick.name;
                GUI.Label(new Rect(Screen.width - 420, 8, 410, 20),
                          $"Kamera: {kamera}", d);
                GUI.Label(new Rect(Screen.width - 420, 28, 410, 20),
                          $"Strahl: {letzterTreffer}", d);
            }

            if (!fadenkreuzZeigen) return;

            float mx = Screen.width * 0.5f;
            float my = Screen.height * 0.5f;

            var punkt = new GUIStyle(GUI.skin.label) { fontSize = 18 };
            punkt.normal.textColor = new Color(1f, 1f, 1f, 0.6f);
            GUI.Label(new Rect(mx - 5, my - 12, 20, 24), "+", punkt);

            if (Time.time < beuteBis)
            {
                var b = new GUIStyle(GUI.skin.label)
                { fontSize = 18, alignment = TextAnchor.MiddleCenter };
                b.normal.textColor = new Color(1f, 0.9f, 0.5f);
                GUI.Label(new Rect(mx - 200, my - 60, 400, 24), beuteText, b);
            }

            if (truheImVisier != null)
            {
                var t = new GUIStyle(GUI.skin.label)
                { fontSize = 16, alignment = TextAnchor.MiddleCenter };
                t.normal.textColor = Color.white;
                GUI.Label(new Rect(mx - 200, my + 20, 400, 24),
                          truheImVisier.Gepluendert ? "durchsucht" : "[E] halten — durchsuchen", t);
                Fortschrittsbalken(mx, my, truheImVisier.suchZeit);
                return;
            }

            if (imVisier == null) return;

            string text;
            if (!imVisier.BrauchtArbeit)
                text = "voll verbarrikadiert";
            else if (vorrat != null && !vorrat.Reicht(imVisier.NaechsteKosten))
                text = $"zu wenig Eisen ({imVisier.NaechsteKosten})";
            else if (imVisier.KannVerstaerken)
                text = $"[E] halten — Brett nageln, {imVisier.NaechsteKosten} Eisen";
            else
                text = $"[E] halten — ausbessern, {imVisier.NaechsteKosten} Eisen";

            if (imVisier.KannAbbauen)
                text += $"     [Q] halten — Brett ab (+{imVisier.eisenZurueck})";
            if (imVisier.IstOffen)
                text = KannDurchsteigen(imVisier) ? "[F] durchsteigen" : "zu erschoepft";

            var hinweis = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            hinweis.normal.textColor = Color.white;
            GUI.Label(new Rect(mx - 200, my + 20, 400, 24), text, hinweis);

            Fortschrittsbalken(mx, my, bauZeit);
        }

        void Fortschrittsbalken(float mx, float my, float dauer)
        {
            if (fortschritt <= 0f) return;

            var balken = new Rect(mx - 60, my + 46, 120, 6);
            var alt = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.5f);
            GUI.DrawTexture(balken, Texture2D.whiteTexture);
            GUI.color = new Color(1f, 0.85f, 0.3f);
            balken.width *= Mathf.Clamp01(fortschritt / Mathf.Max(0.05f, dauer));
            GUI.DrawTexture(balken, Texture2D.whiteTexture);
            GUI.color = alt;
        }
    }
}
