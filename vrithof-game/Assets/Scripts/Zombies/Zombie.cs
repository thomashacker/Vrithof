using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Vrithof.Spieler;
using Vrithof.Welt;
using Vrithof.Worldbuilding;
// System.Random und UnityEngine.Random beissen sich, sobald 'using System'
// dabei ist. Der Alias entscheidet das ein fuer alle Mal.
using Random = UnityEngine.Random;

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
        [Tooltip("An welcher Stelle der Schlag-Animation die Hand ankommt, als " +
                 "Anteil ihrer Laenge. Wer bis dahin aus der Reichweite ist, wird " +
                 "nicht getroffen.")]
        [Range(0.1f, 0.9f)]
        public float ausholAnteil = 0.5f;
        [Tooltip("Rueckfall, falls kein Schlag-Clip an der ZombieAnimation haengt. " +
                 "Mit Clip wird dessen Laenge genommen.")]
        public float angriffsStarre = 0.9f;
        [Tooltip("Wie weit er sich beim Zuschlagen nach vorn wirft. Gibt dem Schlag " +
                 "Wucht — und macht ihn gefaehrlicher, weil er damit noch trifft, " +
                 "wenn man knapp ausserhalb steht. 0 schaltet es ab.")]
        public float schwungWeite = 0.45f;
        [Tooltip("Ueber wie viele Sekunden der Schub laeuft. Endet mit dem Treffer.")]
        public float schwungDauer = 0.18f;
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
        [Tooltip("Augenhoehe ueber dem eigenen Pivot. Der Agent schwebt schon " +
                 "einen Meter ueber dem Boden, deshalb ist der Wert klein.")]
        public float augenHoehe = 0.6f;
        [Tooltip("Anteil der Sichtweite, der in tiefer Nacht uebrig bleibt. " +
                 "Tagsueber gilt die volle. Damit wird die Fackel zum Handel: " +
                 "man sieht etwas — und macht sich zu dem, was sie nachts " +
                 "ueberhaupt noch sehen koennen.")]
        [Range(0.05f, 1f)]
        public float nachtSicht = 0.4f;

        [Header("Klang")]
        [Tooltip("Leer lassen — dann werden Platzhalter erzeugt.")]
        public AudioClip stoehnKlang;
        [Tooltip("Der Schlag auf Bretter. Im Dunkeln die einzige Information " +
                 "darueber, an welcher Wand sie gerade arbeiten.")]
        public AudioClip schlagKlang;
        [Tooltip("Schlurfende Schritte. Wer sie hoert, wird nicht mehr aus dem " +
                 "Dunkeln ueberrascht.")]
        public AudioClip schrittKlang;
        [Tooltip("Nach wie vielen zurueckgelegten Metern ein Schritt faellt. " +
                 "Ueber die Strecke statt ueber die Zeit — dann passt der Takt " +
                 "von allein zum Tempo.")]
        public float schrittWeite = 1.1f;
        [Range(0f, 1f)] public float lautstaerke = 0.6f;
        public float stoehnAbstandMin = 3f;
        public float stoehnAbstandMax = 9f;
        [Tooltip("Ab dieser Entfernung ist er nicht mehr zu hoeren.")]
        public float hoerweite = 25f;

        [Header("Debug")]
        [Tooltip("Blickfeld und aktuelles Ziel als Gizmo. Scene-View, oder " +
                 "Game-View mit eingeschalteten Gizmos.")]
        public bool kegelZeigen = true;

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

        /// Alle lebenden Zombies. Wie bei den Oeffnungen: jeder meldet sich
        /// selbst an, statt dass ein Manager alle kennen muss.
        public static readonly List<Zombie> Alle = new List<Zombie>();

        /// Das Tempo aus dem NavMeshAgent, wie es beim Start stand. Beim Wandern
        /// wird der Agent gedrosselt, deshalb taugt agent.speed nicht als Bezug.
        public float NormalTempo => normalTempo;

        /// Feuert bei jedem Schlag — egal ob gegen Barrikade oder Spieler.
        /// Daran haengt die Animation, damit sie nichts selbst entscheiden muss.
        public event Action Zugeschlagen;

        void OnEnable() { Alle.Add(this); }

        void OnDisable()
        {
            Alle.Remove(this);
            // Sonst bliebe ein mitten im Schlag deaktivierter Zombie fuer immer
            // stehen, wenn er wieder aktiviert wird.
            schlaegtGerade = false;
            if (agent != null && agent.isOnNavMesh) agent.isStopped = false;
        }

        NavMeshAgent agent;
        SpielerLeben opfer;
        Fackel fackel;
        ZombieAnimation darstellung;
        CharacterController zielKoerper;
        Zeit.TageszeitZyklus zyklus;
        Openable zielOeffnung;
        Vector3 letztesGeraeusch;
        bool hatGeraeusch;
        float normalTempo;
        bool siehtGerade;
        float wanderWeiterAb;
        Vector3 zielVorherigePosition;
        AudioSource stimme;
        AudioSource fuesse;
        float naechstesStoehnen;
        float strecke;
        float naechsteAktualisierung;
        float naechsterSchlag;
        bool schlaegtGerade;
        NavMeshPath pfad;   // erst in Awake — im Konstruktor verbietet Unity das

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            pfad = new NavMeshPath();
            normalTempo = agent.speed;
            darstellung = GetComponent<ZombieAnimation>();

            if (stoehnKlang == null) stoehnKlang = Klangwerkstatt.Stoehnen();
            if (schlagKlang == null) schlagKlang = Klangwerkstatt.HolzSchlag();
            stimme = gameObject.AddComponent<AudioSource>();
            stimme.playOnAwake = false;
            stimme.spatialBlend = 1f;        // im Raum verortet — man hoert die Richtung
            stimme.rolloffMode = AudioRolloffMode.Linear;
            stimme.minDistance = 2f;
            stimme.maxDistance = hoerweite;

            if (schrittKlang == null) schrittKlang = Klangwerkstatt.Schritt(true);
            // Eigene Quelle: sonst verstellt ein Schritt die Tonhoehe eines
            // laufenden Stoehnens.
            fuesse = gameObject.AddComponent<AudioSource>();
            fuesse.playOnAwake = false;
            fuesse.spatialBlend = 1f;
            fuesse.rolloffMode = AudioRolloffMode.Linear;
            fuesse.minDistance = 1.5f;
            fuesse.maxDistance = hoerweite;

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
            zyklus = FindAnyObjectByType<Zeit.TageszeitZyklus>();

            if (ziel != null)
            {
                opfer = ziel.GetComponentInParent<SpielerLeben>();
                fackel = ziel.GetComponentInParent<Fackel>();
                zielKoerper = ziel.GetComponentInParent<CharacterController>();
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
            Schritte();
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

            siehtGerade = Sieht();
            if (siehtGerade) GeraeuschMerken(ziel.position);

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

        /// Wie weit er gerade wirklich sieht: nachts weniger, mit Fackel des
        /// Spielers wieder mehr.
        public float EffektiveSicht()
        {
            float hell = zyklus != null ? zyklus.Tageslicht : 1f;
            float grund = sichtWeite * Mathf.Lerp(nachtSicht, 1f, hell);
            return grund * (fackel != null ? fackel.SichtFaktor : 1f);
        }

        // Zombies sehen schlecht: kurze Reichweite, breiter aber stumpfer Blick,
        // und nur Bewegung. Wer stillsteht, ist fuer sie nicht vorhanden.
        bool Sieht()
        {
            Vector3 hin = ziel.position - transform.position;
            float weg = (ziel.position - zielVorherigePosition).magnitude;
            zielVorherigePosition = ziel.position;

            if (weg < bewegungsSchwelle) return false;

            float weite = EffektiveSicht();
            if (hin.sqrMagnitude > weite * weite) return false;

            hin.y = 0f;
            if (Vector3.Angle(transform.forward, hin) > sichtWinkel * 0.5f) return false;

            return Sichtlinie();
        }

        // Waende und Bretter verdecken — andere Zombies nicht. Ohne diese
        // Ausnahme wuerde in einer Horde nur der vorderste etwas sehen.
        //
        // Die Zielhoehe folgt der Koerperhoehe des Spielers: geduckt sinkt sie
        // mit, und damit gibt eine Fensterbruestung tatsaechlich Deckung.
        bool Sichtlinie()
        {
            Vector3 auge = transform.position + Vector3.up * augenHoehe;
            float kopfHoehe = zielKoerper != null ? zielKoerper.height * 0.8f : 1.6f;
            Vector3 kopf = ziel.position + Vector3.up * kopfHoehe;

            Vector3 strecke = kopf - auge;
            float weit = strecke.magnitude;
            if (weit < 0.01f) return true;

            foreach (var t in Physics.RaycastAll(auge, strecke / weit, weit,
                                                 ~0, QueryTriggerInteraction.Ignore))
            {
                if (t.collider.GetComponentInParent<Zombie>() != null) continue;
                if (t.collider.GetComponentInParent<SpielerLeben>() != null) continue;
                return false;
            }
            return true;
        }

        void OnDrawGizmos()
        {
            if (!kegelZeigen || !Application.isPlaying) return;

            Vector3 auge = transform.position + Vector3.up * augenHoehe;

            // Tageszeit und Fackel verschieben die Sichtweite — der Kegel muss
            // zeigen, was wirklich gilt, nicht den Inspector-Wert.
            float weite = EffektiveSicht();
            Gizmos.color = siehtGerade
                ? new Color(1f, 0.25f, 0.2f, 0.9f)
                : new Color(1f, 0.9f, 0.3f, 0.35f);
            Welt.Debugformen.Sichtkegel(auge, transform.forward, sichtWinkel, weite);

            if (!hatGeraeusch) return;

            // Wohin er gerade unterwegs ist und warum.
            Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.8f);
            Gizmos.DrawLine(auge, letztesGeraeusch);
            Welt.Debugformen.Bodenkreis(letztesGeraeusch, 0.6f, 16);

            if (zielOeffnung != null)
            {
                Gizmos.color = new Color(1f, 0.55f, 0.1f, 0.9f);
                Gizmos.DrawLine(auge, zielOeffnung.transform.position);
            }
        }

        // Ueber die zurueckgelegte Strecke getaktet: schlurfen sie langsam,
        // fallen die Schritte selten, verfolgen sie dich, wird es schneller —
        // ohne dass man den Takt an das Tempo koppeln muesste.
        void Schritte()
        {
            if (fuesse == null || schrittKlang == null) return;

            strecke += agent.velocity.magnitude * Time.deltaTime;
            if (strecke < schrittWeite) return;

            strecke = 0f;
            fuesse.pitch = Random.Range(0.65f, 0.85f);   // tiefer = schwerfaellig
            fuesse.PlayOneShot(schrittKlang, lautstaerke * 0.7f);
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
            if (schlaegtGerade || Time.time < naechsterSchlag) return;

            bool gegenOeffnung = zielOeffnung != null;
            Vector3 zielPunkt = gegenOeffnung ? zielOeffnung.transform.position : ziel.position;

            if (!gegenOeffnung && opfer == null) return;
            if (!InReichweite(zielPunkt)) return;

            naechsterSchlag = Time.time + angriffsIntervall;
            StartCoroutine(Schlagen(gegenOeffnung ? zielOeffnung : null));
        }

        /// Wie lange ein Schlag dauert. Die Animation weiss es besser als eine
        /// Zahl im Inspector — die haette sonst nachgezogen werden muessen, sobald
        /// jemand den Clip tauscht.
        float SchlagDauer =>
            darstellung != null && darstellung.SchlagDauer > 0.01f
                ? darstellung.SchlagDauer
                : angriffsStarre;

        // Der Schaden faellt nicht beim Ausholen, sondern wenn die Hand ankommt.
        // Damit wird Ausweichen moeglich: wer rechtzeitig aus der Reichweite ist,
        // wird nicht getroffen.
        //
        // Eine echte Hitbox an den Haenden waere der teure Weg zum selben
        // Ergebnis — sie braucht Knochen-Referenzen, Collider und Trigger, und
        // fuer einen taumelnden Zombie zaehlt am Ende doch nur das Timing.
        System.Collections.IEnumerator Schlagen(Openable gegen)
        {
            schlaegtGerade = true;
            if (agent.isOnNavMesh) agent.isStopped = true;

            float dauer = SchlagDauer;
            float bisZumTreffer = dauer * ausholAnteil;

            Zugeschlagen?.Invoke();          // Animation los

            // Richtung jetzt merken: waehrend der Starre dreht der Agent nicht
            // mehr nach, der Schub soll aber dorthin gehen, wohin er ausholt.
            Vector3 stossRichtung = StossRichtung(gegen);
            float schub = Mathf.Min(schwungDauer, bisZumTreffer);
            float abWann = bisZumTreffer - schub;

            for (float t = 0f; t < bisZumTreffer; t += Time.deltaTime)
            {
                // Der Schwung faellt ans Ende des Ausholens, damit er mit dem
                // Treffer zusammenfaellt statt vorher zu verpuffen.
                if (schwungWeite > 0f && schub > 0.001f && t >= abWann
                    && agent.isOnNavMesh)
                    agent.Move(stossRichtung * (schwungWeite / schub * Time.deltaTime));

                yield return null;
            }

            // Erst jetzt zaehlt, wo alle stehen.
            if (gegen != null)
            {
                if (InReichweite(gegen.transform.position))
                {
                    gegen.Schaden(schadenAnBarrikade);
                    Schlaggeraeusch();
                }
            }
            else if (opfer != null && ziel != null && InReichweite(ziel.position))
            {
                opfer.Schaden(schaden);
                Schlaggeraeusch();
            }

            float rest = dauer - bisZumTreffer;
            if (rest > 0f) yield return new WaitForSeconds(rest);

            if (agent.isOnNavMesh) agent.isStopped = false;
            schlaegtGerade = false;
        }

        void Schlaggeraeusch()
        {
            if (stimme == null || schlagKlang == null) return;
            stimme.pitch = Random.Range(0.85f, 1.15f);
            stimme.PlayOneShot(schlagKlang, lautstaerke);
        }

        Vector3 StossRichtung(Openable gegen)
        {
            Vector3 hin = gegen != null
                ? gegen.transform.position - transform.position
                : ziel != null ? ziel.position - transform.position
                               : transform.forward;
            hin.y = 0f;
            return hin.sqrMagnitude < 0.01f ? transform.forward : hin.normalized;
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
