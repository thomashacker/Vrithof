using UnityEngine;
using UnityEngine.AI;
using Vrithof.Spieler;

namespace Vrithof.Zombies
{
    /// M2: laeuft stur auf den Spieler zu und schlaegt zu, wenn er nah genug ist.
    /// Auf eine Kapsel mit NavMeshAgent legen.
    ///
    /// Tempo, Beschleunigung und Stoppdistanz stehen im NavMeshAgent selbst,
    /// nicht hier — sonst gibt es zwei Wahrheiten. Startwert fuer speed: 4.3.
    /// Der Spieler geht 4 und sprintet 6, also: entkommen nur im Sprint.
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
        [Tooltip("Schaden pro Schlag. 20 bei 100 Leben = fuenf Treffer.")]
        public float schaden = 20f;

        NavMeshAgent agent;
        SpielerLeben opfer;
        float naechsteAktualisierung;
        float naechsterSchlag;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        void Start()
        {
            if (ziel == null)
            {
                var spieler = GameObject.FindGameObjectWithTag("Player");
                if (spieler != null) ziel = spieler.transform;
                else Debug.LogWarning("Zombie findet keinen Spieler (Tag 'Player').", this);
            }
            if (ziel != null) opfer = ziel.GetComponentInParent<SpielerLeben>();
        }

        void Update()
        {
            if (ziel == null) return;

            // Ohne NavMesh unter den Fuessen wirft SetDestination Fehler im Sekundentakt.
            if (!agent.isOnNavMesh) return;

            if (Time.time >= naechsteAktualisierung)
            {
                naechsteAktualisierung = Time.time + zielIntervall;
                agent.SetDestination(ziel.position);
            }

            Zuschlagen();
        }

        void Zuschlagen()
        {
            if (opfer == null || Time.time < naechsterSchlag) return;

            // Flach messen: die Pivots von Spieler und Kapsel liegen auf
            // unterschiedlicher Hoehe, das soll die Reichweite nicht verfaelschen.
            Vector3 d = ziel.position - transform.position;
            d.y = 0f;
            if (d.sqrMagnitude > angriffsReichweite * angriffsReichweite) return;

            naechsterSchlag = Time.time + angriffsIntervall;
            opfer.Schaden(schaden);
        }
    }
}
