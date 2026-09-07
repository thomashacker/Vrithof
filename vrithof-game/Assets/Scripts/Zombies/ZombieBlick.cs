using UnityEngine;

namespace Vrithof.Zombies
{
    /// M5b: der Kopf folgt dir, egal wohin die Beine laufen.
    ///
    /// Laut Design der billigste Schrecken im ganzen Projekt — ein Zombie, der
    /// dich im Vorbeigehen ansieht, ist sofort unangenehm.
    ///
    /// Und es braucht dafuer kein Animation-Rigging-Paket: Unity hat fuer
    /// Humanoid-Avatare eine eingebaute Blick-IK. Die kostet nichts ausser dem
    /// IK Pass am Animator-Layer.
    ///
    /// WICHTIG — zwei Dinge:
    ///   1. Dieses Skript gehoert auf dasselbe GameObject wie der Animator,
    ///      also auf das Modell, nicht auf die Zombie-Wurzel. OnAnimatorIK wird
    ///      nur dort aufgerufen.
    ///   2. Im Animator Controller am Layer (Zahnrad oben rechts) muss
    ///      "IK Pass" angehakt sein, sonst passiert gar nichts.
    [RequireComponent(typeof(Animator))]
    public class ZombieBlick : MonoBehaviour
    {
        [Header("Wen")]
        [Tooltip("Leer lassen — dann wird das Objekt mit Tag 'Player' gesucht.")]
        public Transform ziel;
        [Tooltip("Hoehe am Ziel, auf die geschaut wird. Etwa Kopfhoehe.")]
        public float zielHoehe = 1.5f;

        [Header("Wie weit")]
        [Tooltip("Ab dieser Entfernung schaut er wieder geradeaus. Bewusst weiter " +
                 "als seine Sichtweite: er darf dich anstarren, ohne dich im " +
                 "Sinne der KI 'gesehen' zu haben.")]
        public float blickWeite = 20f;
        [Tooltip("Wie weit er den Kopf ueberhaupt drehen darf, in Grad. Darueber " +
                 "hinaus sieht es nach Genickbruch aus statt nach Zombie.")]
        [Range(0f, 180f)]
        public float maxWinkel = 110f;

        [Header("Wie stark")]
        [Range(0f, 1f)] public float gewicht = 0.9f;
        [Tooltip("Anteil, den der Oberkoerper mitgeht. Etwas davon macht es " +
                 "lebendig, zu viel laesst ihn tanzen.")]
        [Range(0f, 1f)] public float koerperAnteil = 0.25f;
        [Range(0f, 1f)] public float kopfAnteil = 1f;
        [Tooltip("Wie schnell der Blick auf- und abgeblendet wird.")]
        public float blenden = 4f;

        Animator animator;
        ZombieLeben leben;
        float aktuellesGewicht;

        void Awake()
        {
            animator = GetComponent<Animator>();
            // Sitzt auf der Wurzel, dieses Skript auf dem Modell darunter.
            leben = GetComponentInParent<ZombieLeben>();
        }

        void Start()
        {
            if (ziel != null) return;
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) ziel = p.transform;
        }

        void OnAnimatorIK(int layer)
        {
            if (animator == null || ziel == null) return;

            // Eine Leiche sieht niemandem mehr nach. Ohne das folgt der Kopf
            // weiter, und das ist unheimlich auf die falsche Art.
            if (leben != null && leben.Tot)
            {
                animator.SetLookAtWeight(0f);
                return;
            }

            Vector3 punkt = ziel.position + Vector3.up * zielHoehe;
            float soll = Erwuenscht(punkt) ? gewicht : 0f;

            // Sanft ein- und ausblenden: ein Kopf, der schlagartig herumfaehrt,
            // wirkt wie ein Fehler, nicht wie eine Absicht.
            aktuellesGewicht = Mathf.MoveTowards(aktuellesGewicht, soll,
                                                 Time.deltaTime * blenden);
            if (aktuellesGewicht <= 0.001f)
            {
                animator.SetLookAtWeight(0f);
                return;
            }

            animator.SetLookAtWeight(aktuellesGewicht, koerperAnteil, kopfAnteil);
            animator.SetLookAtPosition(punkt);
        }

        bool Erwuenscht(Vector3 punkt)
        {
            Vector3 hin = punkt - transform.position;
            if (hin.sqrMagnitude > blickWeite * blickWeite) return false;

            hin.y = 0f;
            return Vector3.Angle(transform.forward, hin) <= maxWinkel * 0.5f;
        }
    }
}
