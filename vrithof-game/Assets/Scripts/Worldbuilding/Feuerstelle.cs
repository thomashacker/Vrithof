using UnityEngine;
using Vrithof.Welt;

namespace Vrithof.Worldbuilding
{
    /// Eine Feuerstelle, die man mit brennender Fackel entzuenden kann.
    ///
    /// Drei Dinge auf einmal, und sie hangen zusammen:
    ///
    ///   ORIENTIERUNG — der Schein ist weit zu sehen. Nachts findet man ohne so
    ///   etwas nicht einmal nach Hause. Und weil man sie selbst anzuenden muss,
    ///   markiert sie nur die Orte, an denen man schon war.
    ///
    ///   LOCKMITTEL — ein Feuer knistert und leuchtet, und Zombies gehen darauf
    ///   zu. Das laeuft ueber dasselbe Laerm-System wie alles andere, es braucht
    ///   also keine zweite Mechanik. Ein Feuer weit weg zieht sie von der Basis
    ///   fort; das eigene Lagerfeuer zieht sie an.
    ///
    ///   PREIS — man braucht eine brennende Fackel. Damit hat die Fackel einen
    ///   zweiten Zweck, und Licht bleibt eine Entscheidung statt Deko.
    ///
    /// Auf einen Wuerfel legen und als Prefab ablegen.
    public class Feuerstelle : MonoBehaviour
    {
        [Header("Zustand")]
        public bool brennt;
        [Tooltip("Sekunden Anzuenden. Waehrenddessen ist man gebunden.")]
        public float anzuendZeit = 2f;
        [Tooltip("Brenndauer in Spielstunden. 0 = brennt weiter, bis jemand sie " +
                 "loescht — als Wegmarke ist das der Sinn.")]
        public float brenndauerStunden;

        [Header("Lockt an")]
        [Tooltip("Wie weit das Feuer zu hoeren und zu sehen ist. Laeuft ueber " +
                 "das Laerm-System — die Zahl ist die Reichweite in Metern.")]
        public float laerm = 12f;
        [Tooltip("Sekunden zwischen zwei Knistern. Selten genug, dass ein Feuer " +
                 "weniger zieht als ein haemmernder Spieler.")]
        public float laermIntervall = 1.5f;

        [Header("Klang")]
        [Tooltip("Leer lassen — dann wird ein Platzhalter erzeugt.")]
        public AudioClip knisterKlang;
        [Range(0f, 1f)] public float lautstaerke = 0.35f;
        [Tooltip("Ab dieser Entfernung ist das Feuer nicht mehr zu hoeren. " +
                 "Etwas weiter als seine Laerm-Reichweite, damit man es bemerkt, " +
                 "bevor man drinsteht.")]
        public float hoerweite = 22f;

        [Header("Schein")]
        [Tooltip("Hoehe der leuchtenden Kugel. Das ist der Teil, den man wirklich " +
                 "von weitem sieht — ein Licht sieht man nicht, nur was es anstrahlt.")]
        public float punktHoehe = 1.6f;
        public float punktGroesse = 0.9f;
        public Color farbe = new Color(1f, 0.62f, 0.26f);
        public float reichweite = 18f;
        public float staerke = 3f;
        public float flackern = 0.3f;

        Light licht;
        AudioSource stimme;
        GameObject schein;
        Zeit.TageszeitZyklus zyklus;
        float rest;
        float naechstesKnistern;
        float rauschen;

        public bool Brennt => brennt;

        void Start()
        {
            zyklus = FindAnyObjectByType<Zeit.TageszeitZyklus>();
            ScheinBauen();
            KlangBauen();
            if (brennt) rest = brenndauerStunden;
            Anzeigen();
        }

        void ScheinBauen()
        {
            schein = new GameObject("Schein");
            schein.transform.SetParent(transform, false);
            // Weltmassstab, nicht der des Wuerfels: sonst zerrt eine flach
            // gedrueckte Feuerstelle die Kugel mit in die Breite.
            schein.transform.localPosition = Vector3.zero;
            schein.transform.position = transform.position + Vector3.up * punktHoehe;
            schein.transform.localScale = Vector3.one;

            var kugel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            kugel.name = "Glut";
            kugel.transform.SetParent(schein.transform, true);
            kugel.transform.position = schein.transform.position;
            kugel.transform.localScale = Vector3.one * punktGroesse;
            Destroy(kugel.GetComponent<Collider>());

            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader != null)
                kugel.GetComponent<Renderer>().sharedMaterial =
                    new Material(shader) { color = farbe };

            var lichtObjekt = new GameObject("Licht");
            lichtObjekt.transform.SetParent(schein.transform, true);
            lichtObjekt.transform.position = schein.transform.position;
            licht = lichtObjekt.AddComponent<Light>();
            licht.type = LightType.Point;
            licht.color = farbe;
            licht.range = reichweite;
            licht.shadows = LightShadows.None;
        }

        // Als Schleife statt als Einzelschuss: ein Feuer knistert durchgehend,
        // und man soll es hoeren, bevor man es sieht.
        void KlangBauen()
        {
            if (knisterKlang == null) knisterKlang = Klangwerkstatt.Knistern();

            stimme = gameObject.AddComponent<AudioSource>();
            stimme.clip = knisterKlang;
            stimme.loop = true;
            stimme.playOnAwake = false;
            stimme.spatialBlend = 1f;
            stimme.rolloffMode = AudioRolloffMode.Linear;
            stimme.minDistance = 1.5f;
            stimme.maxDistance = hoerweite;
            stimme.volume = lautstaerke;
        }

        /// Anzuenden. Gibt false zurueck, wenn sie schon brennt.
        public bool Anzuenden()
        {
            if (brennt) return false;
            brennt = true;
            rest = brenndauerStunden;
            Anzeigen();
            return true;
        }

        void Anzeigen()
        {
            if (schein != null) schein.SetActive(brennt);
            if (stimme == null) return;

            if (brennt && !stimme.isPlaying) stimme.Play();
            else if (!brennt && stimme.isPlaying) stimme.Stop();
        }

        void Update()
        {
            if (!brennt) return;

            if (brenndauerStunden > 0f)
            {
                float stundenProSekunde = zyklus != null
                    ? 24f / (Mathf.Max(0.1f, zyklus.tagLaengeMinuten) * 60f)
                    : 24f / 360f;
                rest -= stundenProSekunde * Time.deltaTime;
                if (rest <= 0f)
                {
                    brennt = false;
                    Anzeigen();
                    return;
                }
            }

            Knistern();
            Flackern();
        }

        void Knistern()
        {
            if (Time.time < naechstesKnistern) return;
            naechstesKnistern = Time.time + laermIntervall;
            Laerm.Machen(transform.position, laerm);
        }

        void Flackern()
        {
            if (licht == null) return;
            licht.range = reichweite;
            licht.color = farbe;

            rauschen += Time.deltaTime * 7f;
            float zappeln = Mathf.Sin(rauschen) * 0.5f + Mathf.Sin(rauschen * 2.3f) * 0.3f
                          + Random.Range(-0.15f, 0.15f);
            licht.intensity = staerke * (1f + zappeln * flackern);
        }
    }
}
