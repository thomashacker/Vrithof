using UnityEngine;
using UnityEngine.AI;

namespace Vrithof.Zombies
{
    /// M5b: fuettert den Animator mit dem, was der Zombie ohnehin schon tut.
    ///
    /// Bewusst duenn: die Animation trifft keine Entscheidungen, sie zeigt nur
    /// an. Gelaufen wird weiter vom NavMeshAgent, gedacht weiter im Zombie —
    /// diese Komponente uebersetzt bloss.
    ///
    /// Root Motion muss am Animator AUS sein. Sonst schieben sich Agent und
    /// Animation gegenseitig, und der Zombie rutscht oder bleibt stehen.
    ///
    /// Auf dasselbe GameObject wie Zombie legen, das Modell als Kind darunter.
    [RequireComponent(typeof(Zombie))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class ZombieAnimation : MonoBehaviour
    {
        [Header("Referenz")]
        [Tooltip("Leer lassen — dann wird der Animator im Kind gesucht.")]
        public Animator animator;

        [Header("Parameter im Animator Controller")]
        [Tooltip("Float von 0 bis 1: Anteil am Grundtempo des Agents. Der Blend " +
                 "Tree braucht deshalb nur die Schwellen 0 (Idle) und 1 (Walk) — " +
                 "und muss nie nachgezogen werden, wenn das Tempo sich aendert.")]
        public string tempoParameter = "Tempo";
        [Tooltip("Trigger. Wird beim Zuschlagen gefeuert.")]
        public string schlagAusloeser = "Schlag";
        [Tooltip("Float. Als Speed Multiplier NUR am Lauf-State eintragen — " +
                 "nicht global, sonst laeuft auch der Schlag in Zeitlupe, " +
                 "wenn der Zombie langsam ist.")]
        public string schrittfaktorParameter = "Schrittfaktor";
        [Tooltip("Trigger fuer die Todes-Animation.")]
        public string todesAusloeser = "Sterben";

        [Header("Dauer")]
        [Tooltip("Den Attack-Clip hier hineinziehen. Daraus liest der Zombie, wie " +
                 "lange er beim Schlagen stehen bleibt — statt dass die Zahl an " +
                 "zwei Stellen steht und auseinanderlaeuft.")]
        public AnimationClip schlagClip;

        [Header("Feinschliff")]
        [Tooltip("Wie schnell der Tempo-Wert nachzieht. Ohne Glaettung zuckt der " +
                 "Blend Tree bei jeder Richtungsaenderung.")]
        public float glaettung = 8f;
        [Tooltip("Streuung der Abspielgeschwindigkeit je Zombie. Der Gleichschritt " +
                 "einer Horde ist das, was billig aussieht.")]
        public Vector2 tempoStreuung = new Vector2(0.85f, 1.15f);
        [Tooltip("Abspielgeschwindigkeit an das tatsaechliche Tempo koppeln, damit " +
                 "die Schritte zur zurueckgelegten Strecke passen und die Fuesse " +
                 "nicht ueber den Boden rutschen.")]
        public bool schritteAnpassen = true;
        [Tooltip("Wie schnell die Lauf-Animation von sich aus laeuft, in Metern " +
                 "pro Sekunde. Mixamo-Walk liegt meist bei 1.0 bis 1.5. Das ist " +
                 "die Zahl, an der man dreht, bis die Fuesse nicht mehr rutschen.")]
        public float animationsTempo = 1.2f;

        /// Wie lange ein Schlag wirklich dauert — Cliplaenge geteilt durch die
        /// Abspielgeschwindigkeit dieses Zombies. 0, wenn kein Clip gesetzt ist.
        public float SchlagDauer =>
            schlagClip == null ? 0f : schlagClip.length / Mathf.Max(0.1f, streuung);

        Zombie zombie;
        NavMeshAgent agent;
        float gezeigtesTempo;
        float streuung = 1f;
        int tempoId;
        int schlagId;
        int schrittfaktorId;
        // Fehlt ein Parameter im Controller, meldet Unity das bei *jedem*
        // Setzen — mit vollem Stack Trace. Das kostete mehr Leistung als
        // saemtliche Zombie-Logik zusammen. Also vorher nachsehen.
        bool hatTempo, hatSchlag, hatSchrittfaktor, hatTod;
        int todId;
        ZombieLeben leben;

        void Awake()
        {
            zombie = GetComponent<Zombie>();
            agent = GetComponent<NavMeshAgent>();
            if (animator == null) animator = GetComponentInChildren<Animator>();

            tempoId = Animator.StringToHash(tempoParameter);
            schlagId = Animator.StringToHash(schlagAusloeser);
            schrittfaktorId = Animator.StringToHash(schrittfaktorParameter);
            todId = Animator.StringToHash(todesAusloeser);
            leben = GetComponent<ZombieLeben>();

            hatTempo = Kennt(tempoParameter);
            hatSchlag = Kennt(schlagAusloeser);
            hatSchrittfaktor = Kennt(schrittfaktorParameter);
            hatTod = Kennt(todesAusloeser);

            if (animator == null) return;

            // Der Agent bewegt, nicht die Animation.
            animator.applyRootMotion = false;

            // Ausserhalb des Bildes nur noch die Zustandsmaschine mitlaufen
            // lassen, keine Knochen berechnen. Bei einer Horde ist das der
            // groesste Einzelposten — und niemand sieht, was hinter einem
            // passiert.
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

            // Jeder Zombie laeuft ein bisschen anders und faengt woanders an.
            streuung = Random.Range(tempoStreuung.x, tempoStreuung.y);
            animator.speed = streuung;
            animator.Update(Random.Range(0f, 1f));
        }

        // Einmal beim Start nachsehen, was der Controller ueberhaupt kennt.
        // Fehlt etwas, sagt es das einmal — nicht sechzigmal pro Sekunde.
        bool Kennt(string name)
        {
            if (animator == null || animator.runtimeAnimatorController == null) return false;

            foreach (var p in animator.parameters)
                if (p.name == name) return true;

            Debug.LogWarning($"ZombieAnimation: Parameter '{name}' fehlt im Animator " +
                             $"Controller. Wird uebersprungen.", this);
            return false;
        }

        void OnEnable()
        {
            if (zombie == null) zombie = GetComponent<Zombie>();
            zombie.Zugeschlagen += Schlag;

            if (leben == null) leben = GetComponent<ZombieLeben>();
            if (leben != null) leben.Gestorben += Tod;
        }

        void OnDisable()
        {
            if (zombie != null) zombie.Zugeschlagen -= Schlag;
            if (leben != null) leben.Gestorben -= Tod;
        }

        void Tod()
        {
            if (animator == null) return;

            // Jetzt darf die Animation die Wurzel bewegen. Solange er lebt,
            // steuert der Agent — der ist beim Tod abgeschaltet, und ohne Root
            // Motion legt sich der Koerper um einen Punkt, der in der Luft
            // haengen bleibt, statt zu Boden zu gehen.
            animator.applyRootMotion = true;
            // Wieder auf normale Geschwindigkeit: der Schrittfaktor gilt nur
            // fuer den Lauf-State, aber die Streuung wirkt global weiter.
            if (hatTempo) animator.SetFloat(tempoId, 0f);
            if (hatTod) animator.SetTrigger(todId);
        }

        void Schlag()
        {
            if (animator != null && hatSchlag) animator.SetTrigger(schlagId);
        }

        void Update()
        {
            if (animator == null) return;
            if (leben != null && leben.Tot) return;   // eine Leiche laeuft nicht mehr

            Vector3 flach = agent.velocity;
            flach.y = 0f;

            // Als Anteil am Grundtempo, nicht in Metern pro Sekunde: dann steht
            // die Schwelle im Blend Tree fest bei 1, egal wie schnell die Zombies
            // eingestellt sind. Eine Zahl, eine Stelle.
            float anteil = flach.magnitude / Mathf.Max(0.1f, zombie.NormalTempo);
            gezeigtesTempo = Mathf.Lerp(gezeigtesTempo, anteil, Time.deltaTime * glaettung);
            if (hatTempo) animator.SetFloat(tempoId, gezeigtesTempo);

            if (!hatSchrittfaktor) return;

            if (!schritteAnpassen)
            {
                // Trotzdem schreiben: der Parameter haengt am Speed Multiplier
                // des Lauf-States, und auf 0 stuende die Animation still.
                animator.SetFloat(schrittfaktorId, 1f);
                return;
            }

            // Gegen das Tempo rechnen, das die Animation selbst darstellt —
            // nicht gegen das des Agents. Sonst schreitet der Zombie immer
            // gleich weit, egal wie schnell er sich bewegt, und rutscht.
            float faktor = flach.magnitude / Mathf.Max(0.1f, animationsTempo);

            // Ueber einen Parameter statt ueber animator.speed: der wirkt global
            // und wuerde den Schlag mitverlangsamen. Im Stand auf 1, sonst
            // friert die Idle-Animation ein.
            animator.SetFloat(schrittfaktorId, faktor > 0.05f ? faktor : 1f);
        }
    }
}
