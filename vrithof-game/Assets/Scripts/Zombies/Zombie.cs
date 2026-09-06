using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Vrithof.Spieler;
using Vrithof.Welt;
using Vrithof.Worldbuilding;
using Random = UnityEngine.Random;

namespace Vrithof.Zombies
{
    /// Das Verhalten eines Zombies, als Zustandsmaschine.
    ///
    /// Er weiss nie, wo du bist — nur, wo er zuletzt etwas wahrgenommen hat.
    /// Das liefert ZombieSinne; hier wird nur entschieden, was daraus folgt.
    ///
    ///   WANDERN     kein Ziel. Zieht von Gehoeft zu Gehoeft.
    ///   SPUR        laeuft zu der Stelle, an der er etwas bemerkt hat.
    ///   AUFBRECHEN  kommt nicht durch, schlaegt auf eine Oeffnung ein.
    ///   SCHLAGEN    holt aus und trifft. Kurz, blockiert alles andere.
    ///
    /// Der Kern: **ein Zustand haelt sein Ziel fest**, bis es erledigt oder
    /// ungueltig ist. Frueher wurde alle 0.2 s alles neu entschieden, und der
    /// Zombie pendelte zwischen zwei Oeffnungen, weil abwechselnd die eine und
    /// die andere naeher lag.
    ///
    /// Wahrnehmung unterbricht jeden Zustand — eine Zeile, ganz oben, statt
    /// sechs verstreuter Bedingungen.
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(ZombieSinne))]
    public class Zombie : MonoBehaviour
    {
        public enum Lage { Wandern, Spur, Aufbrechen, Schlagen }

        [Header("Angriff")]
        [Tooltip("Ab dieser Distanz wird zugeschlagen. Muss groesser sein als die " +
                 "Stoppdistanz des Agents, sonst kommt er nie in Schlagweite.")]
        public float angriffsReichweite = 1.8f;
        [Tooltip("Sekunden zwischen zwei Schlaegen.")]
        public float angriffsIntervall = 1f;
        [Tooltip("An welcher Stelle der Schlag-Animation die Hand ankommt, als " +
                 "Anteil ihrer Laenge. Wer bis dahin ausweicht, wird nicht getroffen.")]
        [Range(0.1f, 0.9f)]
        public float ausholAnteil = 0.4f;
        [Tooltip("Rueckfall, falls kein Schlag-Clip an der ZombieAnimation haengt.")]
        public float angriffsStarre = 0.7f;
        [Tooltip("Wie weit er sich beim Zuschlagen nach vorn wirft. 0 schaltet es ab.")]
        public float schwungWeite = 0.45f;
        [Tooltip("Ueber wie viele Sekunden der Schub laeuft. Endet mit dem Treffer.")]
        public float schwungDauer = 0.18f;
        public float schaden = 20f;
        [Tooltip("Schaden an Barrikaden. Bewusst weniger: der Spieler soll gegen " +
                 "einen einzelnen anreparieren koennen, gegen mehrere nicht.")]
        public float schadenAnBarrikade = 10f;
        [Tooltip("Wie weit sein Haemmern zu hoeren ist. Zieht andere herbei.")]
        public float schlagLaerm = 16f;
        [Tooltip("Zusatzreichweite, wenn er am Ende seines Weges steht und nicht " +
                 "naeher kommt. Die NavMesh-Sperre einer verschlossenen Oeffnung " +
                 "haelt ihn weiter weg, als die reine Reichweite erlaubt — ohne " +
                 "diese Toleranz steht er davor und tut nichts.")]
        public float schlagToleranz = 1f;

        [Header("Aufbrechen")]
        [Tooltip("Wie viel laenger als die Luftlinie ein Weg hoechstens sein darf. " +
                 "Ohne diese Grenze findet die Wegsuche jedes offene Fenster auf " +
                 "der Rueckseite — er wuesste Dinge, die er nie gesehen hat.")]
        [Range(1f, 6f)]
        public float umwegFaktor = 1.8f;
        [Tooltip("Umkreis um die Spur, in dem eine Oeffnung noch dazugehoert.")]
        public float oeffnungsUmkreis = 12f;

        [Header("Wandern")]
        public bool wandern = true;
        [Tooltip("Tempo beim Wandern, als Anteil des normalen.")]
        public float wanderTempo = 0.55f;
        public float wanderRadius = 25f;
        [Tooltip("Wie oft er ein Gebaeude ansteuert statt irgendwohin zu laufen.")]
        [Range(0f, 1f)]
        public float gehoeftChance = 0.4f;
        public float pauseMin = 2f;
        public float pauseMax = 8f;

        [Header("Aufgeben")]
        [Tooltip("Sekunden, die er sich an der Spur umsieht, bevor er weiterzieht.")]
        public float spurGeduld = 4f;
        [Tooltip("Notbremse: bewegt er sich so lange nicht und arbeitet auch " +
                 "nicht, faengt er von vorn an.")]
        public float feststeckZeit = 10f;

        [Header("Klang")]
        [Tooltip("Leer lassen — dann werden Platzhalter erzeugt.")]
        public AudioClip stoehnKlang;
        public AudioClip schlagKlang;
        public AudioClip schrittKlang;
        [Range(0f, 1f)] public float lautstaerke = 0.6f;
        public float stoehnAbstandMin = 3f;
        public float stoehnAbstandMax = 9f;
        [Tooltip("Nach wie vielen Metern ein Schritt faellt.")]
        public float schrittWeite = 1.1f;
        [Tooltip("Ab dieser Entfernung ist er nicht mehr zu hoeren.")]
        public float hoerweite = 25f;

        [Header("Gesicht")]
        [Tooltip("Klotz an der Vorderseite. Bei einem echten Modell ausschalten.")]
        public bool gesichtZeigen;

        [Header("Takt")]
        [Tooltip("Sekunden zwischen zwei Entscheidungen. Darin steckt eine " +
                 "vollstaendige Pfadberechnung — jeden Frame waere das bei einer " +
                 "Horde der teuerste Posten im ganzen Spiel.")]
        public float entscheidungsTakt = 0.2f;

        [Header("Debug")]
        public bool zustandZeigen = true;

        /// Alle lebenden Zombies. Jeder meldet sich selbst an.
        public static readonly List<Zombie> Alle = new List<Zombie>();

        /// Feuert bei jedem Schlag. Daran haengt die Animation.
        public event Action Zugeschlagen;

        /// Tempo aus dem NavMeshAgent, wie es beim Start stand.
        public float NormalTempo => normalTempo;
        public Lage Zustand { get; private set; } = Lage.Wandern;

        NavMeshAgent agent;
        ZombieSinne sinne;
        SpielerLeben opfer;
        ZombieAnimation darstellung;
        NavMeshPath pfad;
        float normalTempo;
        float normaleStoppdistanz;

        // Genau ein Ziel pro Zustand — festgehalten, nicht jede Runde neu gewaehlt.
        Openable zielOeffnung;
        float angekommenSeit = -1f;
        float wanderWeiterAb;

        float naechsterSchlag;
        AudioSource stimme;
        AudioSource fuesse;
        float naechstesStoehnen;
        float strecke;
        Vector3 letzteStelle;
        float stehtSeit;
        float naechsteEntscheidung;

        void OnEnable() { Alle.Add(this); }

        void OnDisable()
        {
            Alle.Remove(this);
            if (agent != null && agent.isOnNavMesh) agent.isStopped = false;
        }

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            sinne = GetComponent<ZombieSinne>();
            pfad = new NavMeshPath();
            normalTempo = agent.speed;
            normaleStoppdistanz = agent.stoppingDistance;
            darstellung = GetComponent<ZombieAnimation>();
            letzteStelle = transform.position;

            KlangBauen();
            if (gesichtZeigen) GesichtBauen();
        }

        void Start()
        {
            if (sinne.Ziel != null)
                opfer = sinne.Ziel.GetComponentInParent<SpielerLeben>();
        }

        // ------------------------------------------------------------------
        //  Der Ablauf: wahrnehmen, Lage pruefen, handeln.
        // ------------------------------------------------------------------
        void Update()
        {
            // Neben der Flaeche greift nichts von alledem — zurueck darauf.
            if (!agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(transform.position, out var halt, 4f, NavMesh.AllAreas))
                    agent.Warp(halt.position);
                return;
            }

            // Wahrnehmung schlaegt alles. Wer dich hoert, laesst die Tuer stehen,
            // an der er gerade arbeitet.
            if (sinne.NimmtWahr && Zustand != Lage.Schlagen)
                Wechsle(Lage.Spur);

            // Klang laeuft jeden Frame, Entscheidungen nicht: Wegsuche ist teuer.
            Stoehnen();
            Schritte();

            if (Time.time < naechsteEntscheidung) return;
            naechsteEntscheidung = Time.time + entscheidungsTakt;

            Feststecken();

            switch (Zustand)
            {
                case Lage.Wandern:    TueWandern();    break;
                case Lage.Spur:       TueSpur();       break;
                case Lage.Aufbrechen: TueAufbrechen(); break;
                case Lage.Schlagen:                    break;   // laeuft als Coroutine
            }
        }

        void Wechsle(Lage neu)
        {
            if (Zustand == neu) return;

            Zustand = neu;
            angekommenSeit = -1f;
            stehtSeit = Time.time;

            if (neu != Lage.Aufbrechen) zielOeffnung = null;
            if (neu != Lage.Schlagen && agent.isOnNavMesh) agent.isStopped = false;

            // An einer Oeffnung so dicht heran wie das NavMesh erlaubt. Die
            // Stoppdistanz ist dafuer da, dem Spieler nicht auf die Fuesse zu
            // treten — vor einer Tuer waere sie nur im Weg.
            agent.stoppingDistance = neu == Lage.Aufbrechen ? 0f : normaleStoppdistanz;

            agent.speed = neu == Lage.Wandern ? normalTempo * wanderTempo : normalTempo;
        }

        // ------------------------------------------------------------------
        //  WANDERN
        // ------------------------------------------------------------------
        void TueWandern()
        {
            if (!wandern) return;

            bool unterwegs = agent.hasPath && !agent.pathPending
                             && agent.remainingDistance > agent.stoppingDistance + 0.5f;
            if (unterwegs)
            {
                // Solange er laeuft, die Rast nach vorn schieben: sie beginnt
                // erst, wenn er ankommt.
                wanderWeiterAb = Time.time + Random.Range(pauseMin, pauseMax);
                return;
            }

            if (Time.time < wanderWeiterAb) return;

            if (NavMesh.SamplePosition(WanderZiel(), out var treffer, 8f, NavMesh.AllAreas))
                agent.SetDestination(treffer.position);
            else
                wanderWeiterAb = Time.time + 1f;
        }

        Vector3 WanderZiel()
        {
            // Manchmal ein Gebaeude ansteuern — so ziehen sie ueber die Zeit von
            // Gehoeft zu Gehoeft, ohne dass jemand Routen anlegen muesste.
            if (Random.value < gehoeftChance && Openable.Alle.Count > 0)
            {
                var o = Openable.Alle[Random.Range(0, Openable.Alle.Count)];
                if (o != null) return o.transform.position;
            }

            Vector2 r = Random.insideUnitCircle * wanderRadius;
            return transform.position + new Vector3(r.x, 0f, r.y);
        }

        // ------------------------------------------------------------------
        //  SPUR
        // ------------------------------------------------------------------
        void TueSpur()
        {
            if (!sinne.HatSpur) { Wechsle(Lage.Wandern); return; }

            // Der Spieler selbst in Reichweite? Dann zaehlt nichts anderes.
            if (opfer != null && sinne.Ziel != null && InReichweite(sinne.Ziel.position))
            {
                Zuschlagen(null);
                return;
            }

            if (!WegBekannt(sinne.Spur))
            {
                var oeffnung = NaechsteVerschlossene();
                if (oeffnung != null)
                {
                    zielOeffnung = oeffnung;
                    Wechsle(Lage.Aufbrechen);
                    zielOeffnung = oeffnung;   // Wechsel raeumt es sonst weg
                    return;
                }
            }

            agent.SetDestination(sinne.Spur);

            // Am Ende des Weges, nicht am Geraeusch: ein unerreichbares Ziel
            // klemmt Unity auf den naechsten begehbaren Punkt, und dorthin ist
            // der Pfad dann vollstaendig.
            if (!AmEnde()) { angekommenSeit = -1f; return; }

            if (angekommenSeit < 0f) angekommenSeit = Time.time;
            if (Time.time - angekommenSeit < spurGeduld) return;

            sinne.Vergessen();
            Wechsle(Lage.Wandern);
        }

        // ------------------------------------------------------------------
        //  AUFBRECHEN
        // ------------------------------------------------------------------
        void TueAufbrechen()
        {
            // Ziel weg oder offen? Das ist der einzige Weg aus diesem Zustand —
            // deshalb bleibt er an der Tuer, auch wenn er nichts mehr hoert.
            if (zielOeffnung == null || zielOeffnung.IstOffen)
            {
                Wechsle(sinne.HatSpur ? Lage.Spur : Lage.Wandern);
                return;
            }

            Vector3 wo = zielOeffnung.transform.position;
            agent.SetDestination(wo);

            bool amEnde = AmEnde();
            float abstand = Abstand(wo);

            // Entweder wirklich nah — oder er steht am Ende seines Weges und
            // kommt nicht naeher. Dann schlaegt er von dort, wo er ist.
            if (abstand <= angriffsReichweite
                || (amEnde && abstand <= angriffsReichweite + schlagToleranz))
            {
                Zuschlagen(zielOeffnung);
                return;
            }

            // Am Ende und trotzdem zu weit weg: hier ist nichts zu holen.
            if (!amEnde) { angekommenSeit = -1f; return; }

            if (angekommenSeit < 0f) angekommenSeit = Time.time;
            if (Time.time - angekommenSeit < spurGeduld) return;

            zielOeffnung = null;
            Wechsle(Lage.Wandern);
        }

        // Zwei Schritte, und die Reihenfolge ist der Punkt: erst die Oeffnungen
        // nahe der Spur — das ist das Haus, aus dem der Laerm kam —, davon die,
        // die er selbst am schnellsten erreicht.
        Openable NaechsteVerschlossene()
        {
            Openable beste = null;
            float kuerzeste = float.MaxValue;
            float umkreis = oeffnungsUmkreis * oeffnungsUmkreis;

            foreach (var o in Openable.Alle)
            {
                if (o == null || o.IstOffen) continue;
                if ((o.transform.position - sinne.Spur).sqrMagnitude > umkreis) continue;

                float d = (o.transform.position - transform.position).sqrMagnitude;
                if (d >= kuerzeste) continue;
                kuerzeste = d;
                beste = o;
            }
            return beste;
        }

        // ------------------------------------------------------------------
        //  SCHLAGEN
        // ------------------------------------------------------------------
        void Zuschlagen(Openable gegen)
        {
            if (Time.time < naechsterSchlag) return;
            naechsterSchlag = Time.time + angriffsIntervall;

            Wechsle(Lage.Schlagen);
            zielOeffnung = gegen;
            StartCoroutine(Schlagen(gegen));
        }

        /// Wie lange ein Schlag dauert. Die Animation weiss es besser als eine
        /// Zahl im Inspector.
        float SchlagDauer =>
            darstellung != null && darstellung.SchlagDauer > 0.01f
                ? darstellung.SchlagDauer
                : angriffsStarre;

        // Der Schaden faellt nicht beim Ausholen, sondern wenn die Hand ankommt.
        // Damit wird Ausweichen moeglich.
        IEnumerator Schlagen(Openable gegen)
        {
            if (agent.isOnNavMesh) agent.isStopped = true;

            float dauer = Mathf.Clamp(SchlagDauer, 0.1f, 5f);
            float bisZumTreffer = dauer * ausholAnteil;

            Zugeschlagen?.Invoke();

            // Richtung jetzt merken: waehrend der Starre dreht der Agent nicht
            // mehr nach, der Schub soll aber dorthin gehen, wohin er ausholt.
            Vector3 stoss = StossRichtung(gegen);
            float schub = Mathf.Min(schwungDauer, bisZumTreffer);
            float abWann = bisZumTreffer - schub;

            for (float t = 0f; t < bisZumTreffer; t += Time.deltaTime)
            {
                if (schwungWeite > 0f && schub > 0.001f && t >= abWann && agent.isOnNavMesh)
                    agent.Move(stoss * (schwungWeite / schub * Time.deltaTime));
                yield return null;
            }

            // Erst jetzt zaehlt, wo alle stehen.
            if (gegen != null && !gegen.IstOffen
                && Abstand(gegen.transform.position) <= angriffsReichweite + schlagToleranz)
            {
                gegen.Schaden(schadenAnBarrikade);
                Schlaggeraeusch();
                if (schlagLaerm > 0f) Laerm.Machen(transform.position, schlagLaerm);
            }
            else if (gegen == null && opfer != null && sinne.Ziel != null
                     && InReichweite(sinne.Ziel.position))
            {
                opfer.Schaden(schaden);
                Schlaggeraeusch();
            }

            float rest = dauer - bisZumTreffer;
            if (rest > 0f) yield return new WaitForSeconds(rest);

            if (agent.isOnNavMesh) agent.isStopped = false;

            // Zurueck in die Lage, die den Schlag ausgeloest hat.
            Zustand = Lage.Schlagen;   // sicherstellen, dass Wechsle() greift
            if (gegen != null && !gegen.IstOffen)
            {
                Wechsle(Lage.Aufbrechen);
                zielOeffnung = gegen;
            }
            else
            {
                Wechsle(sinne.HatSpur ? Lage.Spur : Lage.Wandern);
            }
        }

        Vector3 StossRichtung(Openable gegen)
        {
            Vector3 hin = gegen != null
                ? gegen.transform.position - transform.position
                : sinne.Ziel != null ? sinne.Ziel.position - transform.position
                                     : transform.forward;
            hin.y = 0f;
            return hin.sqrMagnitude < 0.01f ? transform.forward : hin.normalized;
        }

        // ------------------------------------------------------------------
        //  Hilfen
        // ------------------------------------------------------------------
        bool AmEnde() =>
            !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.6f;

        // Flach messen: die Pivots liegen auf unterschiedlicher Hoehe.
        float Abstand(Vector3 punkt)
        {
            Vector3 d = punkt - transform.position;
            d.y = 0f;
            return d.magnitude;
        }

        bool InReichweite(Vector3 punkt) => Abstand(punkt) <= angriffsReichweite;

        // Ein Weg gilt nur als bekannt, wenn er halbwegs direkt ist. Die
        // Wegsuche wuerde sonst zuverlaessig das offene Fenster hinter dem Haus
        // finden, ohne dass der Zombie je dort war.
        bool WegBekannt(Vector3 wohin)
        {
            if (!agent.CalculatePath(wohin, pfad)) return false;
            if (pfad.status != NavMeshPathStatus.PathComplete) return false;

            float luftlinie = Vector3.Distance(transform.position, wohin);
            if (luftlinie < 1f) return true;

            return PfadLaenge(pfad) <= luftlinie * umwegFaktor;
        }

        static float PfadLaenge(NavMeshPath p)
        {
            float summe = 0f;
            var ecken = p.corners;
            for (int i = 1; i < ecken.Length; i++)
                summe += Vector3.Distance(ecken[i - 1], ecken[i]);
            return summe;
        }

        // Letzte Sicherung. Wer sich nicht ruehrt und dabei nichts bearbeitet,
        // faengt von vorn an — lieber ein Zombie, der grundlos weiterzieht, als
        // einer, der bis zum Rundenende an einer Wand klebt.
        void Feststecken()
        {
            bool arbeitet = Zustand == Lage.Schlagen
                || (zielOeffnung != null && InReichweite(zielOeffnung.transform.position));

            if (arbeitet || (transform.position - letzteStelle).sqrMagnitude > 0.09f)
            {
                letzteStelle = transform.position;
                stehtSeit = Time.time;
                return;
            }

            if (Time.time - stehtSeit < feststeckZeit) return;

            sinne.Vergessen();
            zielOeffnung = null;
            wanderWeiterAb = 0f;
            Wechsle(Lage.Wandern);
            stehtSeit = Time.time;
        }

        // ------------------------------------------------------------------
        //  Klang und Aussehen
        // ------------------------------------------------------------------
        void KlangBauen()
        {
            if (stoehnKlang == null) stoehnKlang = Klangwerkstatt.Stoehnen();
            if (schlagKlang == null) schlagKlang = Klangwerkstatt.HolzSchlag();
            if (schrittKlang == null) schrittKlang = Klangwerkstatt.Schritt(true);

            stimme = Quelle(2f);
            // Eigene Quelle fuer die Fuesse: sonst verstellt ein Schritt die
            // Tonhoehe eines laufenden Stoehnens.
            fuesse = Quelle(1.5f);
        }

        AudioSource Quelle(float nah)
        {
            var q = gameObject.AddComponent<AudioSource>();
            q.playOnAwake = false;
            q.spatialBlend = 1f;
            q.rolloffMode = AudioRolloffMode.Linear;
            q.minDistance = nah;
            q.maxDistance = hoerweite;
            return q;
        }

        void Schlaggeraeusch()
        {
            if (stimme == null || schlagKlang == null) return;
            stimme.pitch = Random.Range(0.85f, 1.15f);
            stimme.PlayOneShot(schlagKlang, lautstaerke);
        }

        void Stoehnen()
        {
            if (Time.time < naechstesStoehnen) return;
            naechstesStoehnen = Time.time + Random.Range(stoehnAbstandMin, stoehnAbstandMax);
            if (stimme == null || stoehnKlang == null) return;
            stimme.pitch = Random.Range(0.8f, 1.15f);
            stimme.PlayOneShot(stoehnKlang, lautstaerke);
        }

        // Ueber die zurueckgelegte Strecke getaktet: der Takt passt damit von
        // allein zum Tempo.
        void Schritte()
        {
            if (fuesse == null || schrittKlang == null) return;

            strecke += agent.velocity.magnitude * Time.deltaTime;
            if (strecke < schrittWeite) return;

            strecke = 0f;
            fuesse.pitch = Random.Range(0.65f, 0.85f);
            fuesse.PlayOneShot(schrittKlang, lautstaerke * 0.7f);
        }

        // Eine Kapsel hat keine Vorderseite. Bei einem echten Modell unnoetig.
        void GesichtBauen()
        {
            var nase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nase.name = "Blickrichtung";
            nase.transform.SetParent(transform, false);
            nase.transform.localPosition = new Vector3(0f, 0.55f, 0.45f);
            nase.transform.localScale = new Vector3(0.45f, 0.14f, 0.2f);
            Destroy(nase.GetComponent<Collider>());
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!Application.isPlaying || !zustandZeigen || sinne == null) return;

            string spur = sinne.SpurZeit < 0f
                ? "keine Spur"
                : $"{sinne.SpurArt} vor {(Time.time - sinne.SpurZeit):0.0}s";

            string agentZustand = !agent.isOnNavMesh ? "NEBEN dem NavMesh"
                : agent.isStopped ? "gestoppt"
                : agent.pathPending ? "sucht Weg"
                : !agent.hasPath ? "kein Weg"
                : $"Rest {agent.remainingDistance:0.0} m";

            var stil = new GUIStyle { fontSize = 11, alignment = TextAnchor.MiddleCenter };
            stil.normal.textColor = sinne.SiehtGerade ? new Color(1f, 0.4f, 0.3f)
                                  : sinne.HoertGerade ? new Color(1f, 0.85f, 0.3f)
                                  : new Color(0.75f, 0.78f, 0.8f);

            Vector3 oben = transform.position + Vector3.up * 2.6f;
            Zeile(oben, 0, $"[{Zustand}]  sieht: {(sinne.SiehtGerade ? "JA" : "–")}" +
                           $"  hört: {(sinne.HoertGerade ? "JA" : "–")}", stil);
            Zeile(oben, 1, spur, stil);
            string oeffnung = zielOeffnung == null ? "—"
                : $"{zielOeffnung.name} ({Abstand(zielOeffnung.transform.position):0.0} m)";
            Zeile(oben, 2, "Öffnung: " + oeffnung, stil);
            Zeile(oben, 3, agentZustand, stil);
        }

        static void Zeile(Vector3 oben, int nummer, string text, GUIStyle stil)
        {
            UnityEditor.Handles.Label(oben + Vector3.down * (nummer * 0.22f), text, stil);
        }
#endif
    }
}
