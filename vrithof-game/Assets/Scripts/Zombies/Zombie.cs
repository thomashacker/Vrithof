using UnityEngine;
using UnityEngine.AI;
using Vrithof.Spieler;
using Vrithof.Welt;
using Vrithof.Worldbuilding;

namespace Vrithof.Zombies
{
    /// M3: geht dorthin, wo es zuletzt laut war — nicht dorthin, wo der Spieler
    /// ist. Er weiss nicht, wo du bist. Er weiss nur, was er gehoert hat.
    ///
    /// Daraus entsteht das ganze Nacht-Gameplay: Rennen verraet dich, Haemmern
    /// zieht sie an die Wand, an der du arbeitest, Stillstehen macht dich
    /// unsichtbar. Und du kannst sie abhaengen, indem du Laerm woanders machst.
    ///
    /// Ist der Weg zum Geraeusch versperrt, schlaegt er auf die Oeffnung ein,
    /// die dem Geraeusch am naechsten liegt. Die Wegfrage beantwortet das
    /// NavMesh selbst: findet der Agent keinen vollstaendigen Pfad, ist dicht.
    ///
    /// Tempo und Stoppdistanz stehen im NavMeshAgent, nicht hier.
    [RequireComponent(typeof(NavMeshAgent))]
    public class Zombie : MonoBehaviour
    {
        [Header("Ziel")]
        [Tooltip("Leer lassen — dann wird das Objekt mit Tag 'Player' gesucht.")]
        public Transform ziel;

        [Header("Verfolgung")]
        [Tooltip("Sekunden zwischen zwei Zielaktualisierungen. Jeden Frame waere Verschwendung.")]
        public float zielIntervall = 0.2f;

        [Header("Angriff")]
        [Tooltip("Ab dieser Distanz wird zugeschlagen. Etwas groesser als die Stoppdistanz " +
                 "des Agents, sonst zappelt er an der Grenze.")]
        public float angriffsReichweite = 1.8f;
        [Tooltip("Sekunden zwischen zwei Schlaegen.")]
        public float angriffsIntervall = 1f;
        [Tooltip("Schaden pro Schlag auf den Spieler. 20 bei 100 Leben = fuenf Treffer.")]
        public float schaden = 20f;
        [Tooltip("Schaden pro Schlag auf eine Barrikade. Bewusst niedriger: " +
                 "der Spieler soll gegen einen einzelnen Zombie anreparieren koennen, " +
                 "gegen mehrere nicht.")]
        public float schadenAnBarrikade = 10f;

        [Header("Augen")]
        [Tooltip("Wie weit er sieht. Kurz halten — er soll blind wirken, nicht wachsam.")]
        public float sichtWeite = 12f;
        [Tooltip("Oeffnungswinkel des Blickfelds in Grad.")]
        public float sichtWinkel = 100f;
        [Tooltip("Er nimmt nur Bewegung wahr. Wer stillsteht, ist fuer ihn nicht da.")]
        public float bewegungsSchwelle = 0.05f;

        [Header("Klang")]
        [Tooltip("Leer lassen — dann wird ein Platzhalter erzeugt.")]
        public AudioClip stoehnKlang;
        [Range(0f, 1f)] public float lautstaerke = 0.6f;
        public float stoehnAbstandMin = 3f;
        public float stoehnAbstandMax = 9f;
        [Tooltip("Ab dieser Entfernung ist er nicht mehr zu hoeren.")]
        public float hoerweite = 25f;

        [Header("Gesicht")]
        [Tooltip("Klotz an der Vorderseite, damit man die Blickrichtung sieht.")]
        public bool gesichtZeigen = true;

        [Header("Wandern")]
        [Tooltip("Ohne Geraeusch ziehen sie umher statt reglos zu warten. " +
                 "Damit ist auch der Tag nie ganz sicher.")]
        public bool wandern = true;
        [Tooltip("Tempo beim Wandern, als Anteil des normalen. Ziellos schlurfen " +
                 "sie langsamer.")]
        public float wanderTempo = 0.55f;
        [Tooltip("Umkreis fuer Zufallsziele, wenn gerade kein Gebaeude gewaehlt wird.")]
        public float wanderRadius = 25f;
        [Tooltip("Wie oft sie ein Gebaeude ansteuern statt irgendwohin zu laufen. " +
                 "So ziehen sie von Gehoeft zu Gehoeft statt im Kreis.")]
        [Range(0f, 1f)]
        public float gehoeftChance = 0.4f;
        public float pauseMin = 2f;
        public float pauseMax = 8f;

        [Header("Gehoer")]
        [Tooltip("Umkreis um das Geraeusch, in dem eine Oeffnung noch dazugehoert. " +
                 "Klein halten — er soll das Haus aufbrechen, aus dem der Laerm kam, " +
                 "nicht irgendeines in der Naehe.")]
        public float oeffnungsUmkreis = 12f;

        NavMeshAgent agent;
        SpielerLeben opfer;
        Fackel fackel;
        Openable zielOeffnung;
        Vector3 letztesGeraeusch;
        bool hatGeraeusch;
        float normalTempo;
        float wanderWeiterAb;
        Vector3 zielVorherigePosition;
        AudioSource stimme;
        float naechstesStoehnen;
        float naechsteAktualisierung;
        float naechsterSchlag;
        NavMeshPath pfad;   // erst in Awake — im Konstruktor verbietet Unity das

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            pfad = new NavMeshPath();
            normalTempo = agent.speed;

            if (stoehnKlang == null) stoehnKlang = Klangwerkstatt.Stoehnen();
            stimme = gameObject.AddComponent<AudioSource>();
            stimme.playOnAwake = false;
            stimme.spatialBlend = 1f;        // im Raum verortet — man hoert die Richtung
            stimme.rolloffMode = AudioRolloffMode.Linear;
            stimme.minDistance = 2f;
            stimme.maxDistance = hoerweite;

            if (gesichtZeigen) GesichtBauen();
        }

        // Eine Kapsel hat keine Vorderseite. Ohne Markierung sieht man nicht,
        // wohin er blickt — und damit auch nicht, ob die Sicht funktioniert.
        void GesichtBauen()
        {
            var nase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nase.name = "Blickrichtung";
            nase.transform.SetParent(transform, false);
            nase.transform.localPosition = new Vector3(0f, 0.55f, 0.45f);
            nase.transform.localScale = new Vector3(0.45f, 0.14f, 0.2f);
            Destroy(nase.GetComponent<Collider>());
        }

        void Start()
        {
            if (ziel == null)
            {
                var spieler = GameObject.FindGameObjectWithTag("Player");
                if (spieler != null) ziel = spieler.transform;
                else Debug.LogWarning("Zombie findet keinen Spieler (Tag 'Player').", this);
            }
            if (ziel != null)
            {
                opfer = ziel.GetComponentInParent<SpielerLeben>();
                fackel = ziel.GetComponentInParent<Fackel>();
                zielVorherigePosition = ziel.position;
            }
        }

        void Update()
        {
            if (ziel == null) return;

            // Ohne NavMesh unter den Fuessen wirft SetDestination Fehler im Sekundentakt.
            if (!agent.isOnNavMesh) return;

            if (Time.time >= naechsteAktualisierung)
            {
                naechsteAktualisierung = Time.time + zielIntervall;
                ZielWaehlen();
            }

            Zuschlagen();
            Stoehnen();
        }

        /// Von aussen ein Ziel geben — der Spawner setzt damit den Ort, der sie
        /// ueberhaupt erst hergelockt hat.
        public void GeraeuschMerken(Vector3 ort)
        {
            letztesGeraeusch = ort;
            hatGeraeusch = true;
        }

        /// Alles vergessen. Im Morgengrauen bleiben sie stehen, wo sie sind,
        /// statt weiter auf die Stelle zuzulaufen, an der sie dich nachts
        /// zuletzt gehoert haben. So gehoert der Tag wieder dir — auch wenn
        /// sie noch da sind.
        public void GeraeuschVergessen()
        {
            hatGeraeusch = false;
            zielOeffnung = null;
            if (agent != null && agent.isOnNavMesh) agent.ResetPath();
        }

        // Sehen und hoeren speisen dasselbe Gedaechtnis: er weiss immer nur,
        // wo er zuletzt *etwas* wahrgenommen hat. Kein zweites Zielverhalten.
        void ZielWaehlen()
        {
            if (Laerm.Lautestes(transform.position, out var gehoert))
                GeraeuschMerken(gehoert);

            if (Sieht()) GeraeuschMerken(ziel.position);

            if (!hatGeraeusch)
            {
                zielOeffnung = null;
                if (wandern) Wandern();
                return;
            }

            agent.speed = normalTempo;   // etwas gehoert — nicht mehr schlurfen

            if (agent.CalculatePath(letztesGeraeusch, pfad)
                && pfad.status == NavMeshPathStatus.PathComplete)
            {
                zielOeffnung = null;
                agent.SetDestination(letztesGeraeusch);
                return;
            }

            zielOeffnung = NaechsteVerschlossene();
            if (zielOeffnung != null)
                agent.SetDestination(zielOeffnung.transform.position);
            else
                agent.SetDestination(letztesGeraeusch);
        }

        // Zwei Schritte, und die Reihenfolge ist der Punkt:
        //
        //   1. Nur Oeffnungen nahe am Geraeusch — das ist das Haus, aus dem es
        //      kam. Frueher stand hier der Abstand zum Spieler, und das war ein
        //      Hack: der Zombie lief zum naechstbesten Gebaeude, egal ob dort
        //      jemand war. Das Geraeusch sagt ihm jetzt, welches gemeint ist.
        //   2. Davon die, die er selbst am schnellsten erreicht. So verteilt
        //      sich eine Horde von allein auf die Oeffnungen einer Huette.
        // Ohne Geraeusch ziehen sie umher. Meist irgendwohin, manchmal gezielt
        // zu einem Gebaeude — die Oeffnungen im Register markieren ja genau die.
        // Dadurch wandern sie ueber die Zeit von Gehoeft zu Gehoeft, ohne dass
        // jemand Routen anlegen muesste.
        void Wandern()
        {
            agent.speed = normalTempo * wanderTempo;

            bool unterwegs = agent.hasPath && !agent.pathPending
                             && agent.remainingDistance > agent.stoppingDistance + 0.5f;
            if (unterwegs)
            {
                // Solange er laeuft, die Rast nach vorn schieben: sie beginnt
                // erst in dem Moment, in dem er ankommt.
                wanderWeiterAb = Time.time + Random.Range(pauseMin, pauseMax);
                return;
            }

            if (Time.time < wanderWeiterAb) return;

            Vector3 wunsch = WanderZiel();
            if (NavMesh.SamplePosition(wunsch, out var treffer, 8f, NavMesh.AllAreas))
                agent.SetDestination(treffer.position);
            else
                wanderWeiterAb = Time.time + 1f;   // nichts gefunden, gleich nochmal
        }

        Vector3 WanderZiel()
        {
            if (Random.value < gehoeftChance && Openable.Alle.Count > 0)
            {
                var o = Openable.Alle[Random.Range(0, Openable.Alle.Count)];
                if (o != null) return o.transform.position;
            }

            Vector2 r = Random.insideUnitCircle * wanderRadius;
            return transform.position + new Vector3(r.x, 0f, r.y);
        }

        // Zombies sehen schlecht: kurze Reichweite, breiter aber stumpfer Blick,
        // und nur Bewegung. Wer stillsteht, ist fuer sie nicht vorhanden.
        bool Sieht()
        {
            Vector3 hin = ziel.position - transform.position;
            float weg = (ziel.position - zielVorherigePosition).magnitude;
            zielVorherigePosition = ziel.position;

            if (weg < bewegungsSchwelle) return false;

            // Wer eine Fackel traegt, ist weiter zu sehen. Das ist der Preis
            // fuers Sehen — und der Grund, sie manchmal auszumachen.
            float weite = sichtWeite * (fackel != null ? fackel.SichtFaktor : 1f);
            if (hin.sqrMagnitude > weite * weite) return false;

            hin.y = 0f;
            if (Vector3.Angle(transform.forward, hin) > sichtWinkel * 0.5f) return false;

            // Sichtlinie: Waende und Bretter verdecken. Etwas ueber dem Boden
            // messen, sonst blockiert der Untergrund selbst.
            Vector3 auge = transform.position + Vector3.up * 1.2f;
            Vector3 kopf = ziel.position + Vector3.up * 1.2f;
            if (Physics.Linecast(auge, kopf, out var sperre, ~0, QueryTriggerInteraction.Ignore)
                && sperre.collider.GetComponentInParent<SpielerLeben>() == null)
                return false;

            return true;
        }

        void Stoehnen()
        {
            if (Time.time < naechstesStoehnen)
                return;
            naechstesStoehnen = Time.time + Random.Range(stoehnAbstandMin, stoehnAbstandMax);
            if (stimme == null || stoehnKlang == null) return;
            stimme.pitch = Random.Range(0.8f, 1.15f);
            stimme.PlayOneShot(stoehnKlang, lautstaerke);
        }

        Openable NaechsteVerschlossene()
        {
            Openable beste = null;
            float kuerzeste = float.MaxValue;
            float umkreis = oeffnungsUmkreis * oeffnungsUmkreis;

            foreach (var o in Openable.Alle)
            {
                if (o == null || o.IstOffen) continue;
                if ((o.transform.position - letztesGeraeusch).sqrMagnitude > umkreis) continue;

                float d = (o.transform.position - transform.position).sqrMagnitude;
                if (d >= kuerzeste) continue;
                kuerzeste = d;
                beste = o;
            }

            return beste;
        }

        void Zuschlagen()
        {
            if (Time.time < naechsterSchlag) return;

            if (zielOeffnung != null)
            {
                if (!InReichweite(zielOeffnung.transform.position)) return;
                naechsterSchlag = Time.time + angriffsIntervall;
                zielOeffnung.Schaden(schadenAnBarrikade);
                return;
            }

            if (opfer == null || !InReichweite(ziel.position)) return;
            naechsterSchlag = Time.time + angriffsIntervall;
            opfer.Schaden(schaden);
        }

        // Flach messen: die Pivots liegen auf unterschiedlicher Hoehe, das soll
        // die Reichweite nicht verfaelschen.
        bool InReichweite(Vector3 punkt)
        {
            Vector3 d = punkt - transform.position;
            d.y = 0f;
            return d.sqrMagnitude <= angriffsReichweite * angriffsReichweite;
        }
    }
}
