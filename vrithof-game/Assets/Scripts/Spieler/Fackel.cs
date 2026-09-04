using UnityEngine;
using UnityEngine.InputSystem;

namespace Vrithof.Spieler
{
    /// M4: Licht, das ausgeht.
    ///
    /// Die Nacht ist stockdunkel — ohne Fackel siehst du draussen nichts. Mit
    /// Fackel siehst du etwas, wirst aber selbst weiter gesehen: Zombies
    /// erkennen dich im Schein aus groesserer Entfernung. Das ist der ganze
    /// Handel, und er kostet keine einzige neue Ressource-Entscheidung — nur
    /// die Frage, wie lange du dich noch draussen traust.
    ///
    /// Die Brenndauer laeuft in Spielstunden, nicht in Sekunden. Sonst haengt
    /// sie an der Tageslaenge, und die drehen wir beim Balancing staendig.
    ///
    /// Auf den Player legen.
    public class Fackel : MonoBehaviour
    {
        [Header("Vorrat")]
        [Tooltip("Fackeln im Gepaeck. Die brennende zaehlt nicht mit.")]
        public int vorrat = 1;
        [Tooltip("Wie lange eine Fackel haelt, in Spielstunden. Eine Nacht " +
                 "dauert zehn — eine Fackel bringt dich also nicht durch.")]
        public float brenndauerStunden = 6f;

        [Header("Licht")]
        public Color flammenFarbe = new Color(1f, 0.72f, 0.36f);
        public float reichweite = 12f;
        public float staerke = 3.5f;
        [Tooltip("Wie stark die Flamme zappelt. 0 = ruhiges Licht.")]
        public float flackern = 0.35f;

        [Header("Verraeterisch")]
        [Tooltip("Faktor auf die Sichtweite der Zombies, solange sie brennt. " +
                 "Wer Licht traegt, ist weiter zu sehen.")]
        public float sichtFaktor = 2f;

        [Header("Anzeige")]
        public bool anzeigen = true;

        /// Fuer die Zombies: brennt gerade eine?
        public bool Brennt { get; private set; }
        /// Faktor, den die Sichtweite abbekommt.
        public float SichtFaktor => Brennt ? sichtFaktor : 1f;

        float rest;              // Spielstunden der brennenden Fackel
        Light flamme;
        Transform holz;
        Zeit.TageszeitZyklus zyklus;
        float rauschen;

        void Start()
        {
            zyklus = FindAnyObjectByType<Zeit.TageszeitZyklus>();
            BauenAnDerKamera();
            Zeigen(false);
        }

        // Fackel und Licht haengen an der Kamera, damit sie dem Blick folgen.
        // Etwas nach rechts und unten versetzt — man haelt sie ja in der Hand.
        void BauenAnDerKamera()
        {
            var kamera = Camera.main;
            if (kamera == null) return;

            var wurzel = new GameObject("Fackel");
            wurzel.transform.SetParent(kamera.transform, false);
            wurzel.transform.localPosition = new Vector3(0.45f, -0.3f, 0.55f);
            wurzel.transform.localRotation = Quaternion.Euler(-15f, 0f, 12f);

            // Der Stiel: ein Klotz, wie alles andere im Blockout.
            var stab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stab.transform.SetParent(wurzel.transform, false);
            stab.transform.localPosition = new Vector3(0f, -0.2f, 0f);
            stab.transform.localScale = new Vector3(0.05f, 0.5f, 0.05f);
            Destroy(stab.GetComponent<Collider>());

            var glut = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glut.transform.SetParent(wurzel.transform, false);
            glut.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            glut.transform.localScale = Vector3.one * 0.13f;
            Destroy(glut.GetComponent<Collider>());
            var r = glut.GetComponent<Renderer>();
            if (r != null) r.material.color = flammenFarbe;

            var lichtObjekt = new GameObject("Flamme");
            lichtObjekt.transform.SetParent(wurzel.transform, false);
            lichtObjekt.transform.localPosition = new Vector3(0f, 0.12f, 0f);
            flamme = lichtObjekt.AddComponent<Light>();
            flamme.type = LightType.Point;
            flamme.color = flammenFarbe;
            flamme.range = reichweite;
            flamme.intensity = staerke;
            flamme.shadows = LightShadows.None;   // Punktlicht-Schatten sind teuer

            holz = wurzel.transform;
        }

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
                Umschalten();

            if (!Brennt) return;

            // In Spielstunden abbrennen, damit die Fackel an der Tageslaenge haengt.
            float stundenProSekunde = zyklus != null
                ? 24f / (Mathf.Max(0.1f, zyklus.tagLaengeMinuten) * 60f)
                : 24f / 360f;

            rest -= stundenProSekunde * Time.deltaTime;
            if (rest <= 0f)
            {
                rest = 0f;
                Zeigen(false);   // sie ist heruntergebrannt
                return;
            }

            Flackern();
        }

        void Umschalten()
        {
            if (Brennt)
            {
                // Ausmachen heisst nicht wegwerfen: der Rest bleibt erhalten.
                Zeigen(false);
                return;
            }

            if (rest > 0f) { Zeigen(true); return; }   // angefangene weiterbrennen
            if (vorrat <= 0) return;

            vorrat--;
            rest = brenndauerStunden;
            Zeigen(true);
        }

        void Zeigen(bool an)
        {
            Brennt = an;
            if (holz != null) holz.gameObject.SetActive(an);
        }

        // Zwei ueberlagerte Sinuswellen plus etwas Zufall — billiger als eine
        // Partikelflamme und in dunkler Umgebung fast so wirksam.
        void Flackern()
        {
            if (flamme == null) return;
            rauschen += Time.deltaTime * 9f;
            float zappeln = Mathf.Sin(rauschen) * 0.5f + Mathf.Sin(rauschen * 2.7f) * 0.3f
                          + Random.Range(-0.2f, 0.2f);
            flamme.intensity = staerke * (1f + zappeln * flackern);
        }

        void OnGUI()
        {
            if (!anzeigen) return;
            // Unter Nahrung (146).
            var style = new GUIStyle(GUI.skin.label) { fontSize = 20 };
            style.normal.textColor = Brennt ? new Color(1f, 0.8f, 0.45f)
                                            : new Color(0.7f, 0.7f, 0.7f);
            string zustand = Brennt ? $"brennt {rest:0.0} h"
                           : rest > 0f ? $"aus ({rest:0.0} h)"
                           : "—";
            GUI.Label(new Rect(12, 172, 300, 30), $"Fackel {vorrat}  [T] {zustand}", style);
        }
    }
}
