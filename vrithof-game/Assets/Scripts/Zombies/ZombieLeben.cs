using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Vrithof.Zombies
{
    /// M5b: Lebenspunkte und Sterben.
    ///
    /// Getrennt vom Zombie-Verhalten, damit beide fuer sich lesbar bleiben —
    /// dasselbe Muster wie SpielerLeben.
    ///
    /// Eine Leiche bleibt liegen und verschwindet erst nach einer Weile. Das
    /// ist billige Atmosphaere: man sieht, wo man gekaempft hat. Und es haelt
    /// die Szene trotzdem sauber, weil der Spawner sonst irgendwann nur noch
    /// Leichen zaehlt.
    ///
    /// Auf dieselbe Wurzel wie Zombie legen.
    [RequireComponent(typeof(Zombie))]
    public class ZombieLeben : MonoBehaviour
    {
        [Header("Leben")]
        public float maxLeben = 100f;

        [Header("Rueckstoss")]
        [Tooltip("Wie weit ein Treffer ihn zurueckwirft. Verschafft Luft — und " +
                 "ist der Grund, warum ein einzelner Zombie zu schaffen ist.")]
        public float rueckstoss = 0.6f;
        public float rueckstossDauer = 0.15f;

        [Header("Leiche")]
        [Tooltip("Sekunden, bis sie verschwindet. 0 = bleibt liegen.")]
        public float verfallSekunden = 45f;

        [Header("Klang")]
        [Tooltip("Leer lassen — dann werden Platzhalter erzeugt.")]
        public AudioClip trefferKlang;
        public AudioClip todesKlang;
        [Range(0f, 1f)] public float lautstaerke = 0.7f;

        /// Feuert einmal, wenn er faellt. Daran haengt die Todes-Animation.
        public event Action Gestorben;

        public bool Tot { get; private set; }
        public float Leben => leben;

        float leben;
        Zombie zombie;
        NavMeshAgent agent;
        AudioSource stimme;

        void Awake()
        {
            leben = maxLeben;
            zombie = GetComponent<Zombie>();
            agent = GetComponent<NavMeshAgent>();

            if (trefferKlang == null) trefferKlang = Welt.Klangwerkstatt.Fleischtreffer();
            if (todesKlang == null) todesKlang = Welt.Klangwerkstatt.Stoehnen();

            stimme = gameObject.AddComponent<AudioSource>();
            stimme.playOnAwake = false;
            stimme.spatialBlend = 1f;
            stimme.rolloffMode = AudioRolloffMode.Linear;
            stimme.minDistance = 2f;
            stimme.maxDistance = 30f;
        }

        /// Schaden einstecken. Die Richtung dient nur dem Rueckstoss.
        public void Schaden(float menge, Vector3 ausRichtung)
        {
            if (Tot) return;

            leben -= menge;
            Klang(trefferKlang, 0.85f, 1.15f);

            if (leben > 0f)
            {
                if (rueckstoss > 0f) StartCoroutine(Zurueck(ausRichtung));
                return;
            }

            Sterben();
        }

        System.Collections.IEnumerator Zurueck(Vector3 richtung)
        {
            richtung.y = 0f;
            if (richtung.sqrMagnitude < 0.01f) yield break;
            richtung.Normalize();

            // Ueber den Agent, damit er auf dem NavMesh bleibt und nicht durch
            // Waende geschoben wird.
            for (float t = 0f; t < rueckstossDauer && !Tot; t += Time.deltaTime)
            {
                if (agent != null && agent.isOnNavMesh)
                    agent.Move(richtung * (rueckstoss / rueckstossDauer * Time.deltaTime));
                yield return null;
            }
        }

        void Sterben()
        {
            Tot = true;
            leben = 0f;
            Klang(todesKlang, 0.7f, 0.9f);
            Gestorben?.Invoke();

            // Verhalten aus, aber das Objekt bleibt: die Animation soll noch
            // zu Ende laufen und die Leiche liegen bleiben.
            if (zombie != null) zombie.enabled = false;
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }

            // Der Collider muss weg, sonst steht man vor einer unsichtbaren Wand
            // und Sichtlinien brechen sich an Leichen.
            foreach (var c in GetComponentsInChildren<Collider>())
                c.enabled = false;

            if (verfallSekunden > 0f) Destroy(gameObject, verfallSekunden);
        }

        void Klang(AudioClip clip, float min, float max)
        {
            if (stimme == null || clip == null) return;
            stimme.pitch = Random.Range(min, max);
            stimme.PlayOneShot(clip, lautstaerke);
        }
    }
}
