using UnityEngine;
using Vrithof.Spieler;
using Vrithof.Welt;

namespace Vrithof.Zombies
{
    /// Was ein Zombie mitbekommt — und was er sich davon merkt.
    ///
    /// Beantwortet genau eine Frage: *Wo habe ich zuletzt etwas wahrgenommen?*
    /// Was daraus folgt, entscheidet der Zombie, nicht diese Komponente.
    ///
    /// Sehen und Hoeren speisen dasselbe Gedaechtnis. Es gibt keine zwei
    /// Zielsysteme — er weiss immer nur von einer Stelle, und die ist entweder
    /// frisch oder alt.
    ///
    /// Auf dieselbe Wurzel wie Zombie legen.
    [RequireComponent(typeof(Zombie))]
    public class ZombieSinne : MonoBehaviour
    {
        [Header("Augen")]
        [Tooltip("Wie weit er am hellen Tag sieht.")]
        public float sichtWeite = 25f;
        [Tooltip("Oeffnungswinkel des Blickfelds in Grad.")]
        public float sichtWinkel = 100f;
        [Tooltip("Anteil der Sichtweite, der in tiefer Nacht uebrig bleibt. " +
                 "Damit wird die Fackel zum Handel: man sieht etwas — und macht " +
                 "sich zu dem, was sie nachts ueberhaupt noch sehen koennen.")]
        [Range(0.05f, 1f)]
        public float nachtSicht = 0.4f;
        [Tooltip("Er nimmt nur Bewegung wahr. Wer stillsteht, ist fuer ihn nicht da.")]
        public float bewegungsSchwelle = 0.05f;
        [Tooltip("Augenhoehe ueber dem eigenen Pivot. Der Agent schwebt schon " +
                 "einen Meter ueber dem Boden, deshalb ist der Wert klein.")]
        public float augenHoehe = 0.6f;

        [Header("Takt")]
        [Tooltip("Sekunden zwischen zwei Wahrnehmungen. Jeden Frame waere Verschwendung.")]
        public float taktung = 0.2f;

        /// Nimmt er den Spieler gerade wahr — auf welchem Weg auch immer?
        public bool NimmtWahr => SiehtGerade || HoertGerade;
        public bool SiehtGerade { get; private set; }
        public bool HoertGerade { get; private set; }

        /// Weiss er von einer Stelle, an der etwas war?
        public bool HatSpur { get; private set; }
        public Vector3 Spur { get; private set; }
        /// "gesehen" oder "gehoert" — nur fuer die Debug-Anzeige.
        public string SpurArt { get; private set; } = "";
        /// Wann die Spur zuletzt aufgefrischt wurde. Negativ heisst: noch nie.
        public float SpurZeit { get; private set; } = -99f;

        Transform ziel;
        CharacterController zielKoerper;
        Fackel fackel;
        Zeit.TageszeitZyklus zyklus;
        float naechsterTakt;
        // Einmal belegt statt bei jedem Blick neu: RaycastAll erzeugt sonst
        // Muell, den der Garbage Collector in Schueben einsammelt — und genau
        // diese Schuebe fuehlen sich wie Ruckeln an.
        readonly RaycastHit[] treffer = new RaycastHit[8];
        Vector3 vorherigeZielPosition;

        public Transform Ziel => ziel;

        void Start()
        {
            zyklus = FindAnyObjectByType<Zeit.TageszeitZyklus>();

            var p = GameObject.FindGameObjectWithTag("Player");
            if (p == null) return;

            ziel = p.transform;
            zielKoerper = p.GetComponent<CharacterController>();
            fackel = p.GetComponentInParent<Fackel>();
            vorherigeZielPosition = ziel.position;
        }

        void Update()
        {
            if (ziel == null || Time.time < naechsterTakt) return;
            naechsterTakt = Time.time + taktung;

            HoertGerade = Laerm.Lautestes(transform.position, out var gehoert);
            if (HoertGerade) Merken(gehoert, "gehört");

            // Sehen zuletzt: was man sieht, ist genauer als was man hoert.
            SiehtGerade = Sieht();
            if (SiehtGerade) Merken(ziel.position, "gesehen");
        }

        /// Eine Stelle merken. Der Spawner nutzt das, um frisch abgesetzte
        /// Zombies ueberhaupt erst loszuschicken.
        public void Merken(Vector3 ort, string art = "gemeldet")
        {
            Spur = ort;
            SpurArt = art;
            SpurZeit = Time.time;
            HatSpur = true;
        }

        /// Alles vergessen. Danach zieht der Zombie wieder umher.
        public void Vergessen()
        {
            HatSpur = false;
        }

        /// Wie weit er gerade wirklich sieht: nachts weniger, mit Fackel des
        /// Spielers wieder mehr.
        public float Reichweite()
        {
            float hell = zyklus != null ? zyklus.Tageslicht : 1f;
            float grund = sichtWeite * Mathf.Lerp(nachtSicht, 1f, hell);
            return grund * (fackel != null ? fackel.SichtFaktor : 1f);
        }

        // Zombies sehen schlecht: kurze Reichweite, breiter aber stumpfer Blick,
        // und nur Bewegung. Wer stillsteht, ist fuer sie nicht vorhanden.
        bool Sieht()
        {
            float weg = (ziel.position - vorherigeZielPosition).magnitude;
            vorherigeZielPosition = ziel.position;
            if (weg < bewegungsSchwelle) return false;

            Vector3 hin = ziel.position - transform.position;
            float weite = Reichweite();
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

            int wieViele = Physics.RaycastNonAlloc(auge, strecke / weit, treffer, weit,
                                                   ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < wieViele; i++)
            {
                var c = treffer[i].collider;
                if (c.GetComponentInParent<Zombie>() != null) continue;
                if (c.GetComponentInParent<SpielerLeben>() != null) continue;
                return false;
            }
            return true;
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            Vector3 auge = transform.position + Vector3.up * augenHoehe;
            Gizmos.color = SiehtGerade ? new Color(1f, 0.25f, 0.2f, 0.9f)
                                       : new Color(1f, 0.9f, 0.3f, 0.35f);
            Debugformen.Sichtkegel(auge, transform.forward, sichtWinkel, Reichweite());

            if (!HatSpur) return;
            Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.8f);
            Gizmos.DrawLine(auge, Spur);
            Debugformen.Bodenkreis(Spur, 0.6f, 16);
        }
    }
}
