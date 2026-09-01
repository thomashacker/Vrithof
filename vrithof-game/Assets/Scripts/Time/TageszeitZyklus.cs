using System;
using UnityEngine;

namespace Vrithof.Zeit
{
    /// Dreht die Directional Light ueber eine einstellbare Tageslaenge und zeigt
    /// eine simple Uhr (Tag + Uhrzeit) oben links. M1-Ziel: sichtbarer Tag/Nacht-
    /// Bogen, "zusehen wie es dunkel wird". Auf die Directional Light legen.
    [RequireComponent(typeof(Light))]
    public class TageszeitZyklus : MonoBehaviour
    {
        [Header("Zeit")]
        [Tooltip("Sekunden fuer einen vollen 24h-Zyklus.")]
        public float tagLaengeSekunden = 120f;
        [Range(0f, 24f)]
        [Tooltip("Uhrzeit beim Start.")]
        public float startStunde = 6f;

        [Header("Sonne")]
        [Tooltip("Himmelsrichtung der Sonne (Grad um die Y-Achse).")]
        public float azimut = 30f;

        [Header("Dunkelheit")]
        [Tooltip("Umgebungslicht am Tag.")]
        public Color tagAmbient = new Color(0.45f, 0.47f, 0.5f);
        [Tooltip("Umgebungslicht in der Nacht. Fast schwarz = stockdunkel.")]
        public Color nachtAmbient = new Color(0.02f, 0.02f, 0.03f);

        [Header("Nacht")]
        [Range(0f, 24f)]
        [Tooltip("Ab dieser Stunde kommen sie.")]
        public float nachtBeginn = 20f;
        [Range(0f, 24f)]
        [Tooltip("Ab dieser Stunde ist es ueberstanden.")]
        public float nachtEnde = 6f;

        [Header("Uhr-Anzeige")]
        public bool uhrZeigen = true;

        /// Feuern beim Uebergang, mit der Nummer des angebrochenen Tages.
        /// Daran haengt der Spawner — der Zyklus selbst weiss nichts von Zombies.
        public event Action<int> NachtBeginnt;
        public event Action<int> TagBeginnt;

        public float Stunde => stunde;
        public int Tag => tag;
        public bool IstNacht => stunde >= nachtBeginn || stunde < nachtEnde;

        float stunde;   // 0..24
        int tag = 1;
        bool warNacht;
        Light sonne;
        float maxIntensitaet;
        Material himmel;         // eigene Instanz der Skybox, damit wir sie faden duerfen
        float tagExposure = 1f;

        void Awake()
        {
            sonne = GetComponent<Light>();
            maxIntensitaet = sonne.intensity;   // respektiert die im Inspector gesetzte Intensitaet
            // Ambient selbst steuern, damit die Nacht wirklich dunkel wird.
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            // Eigene Skybox-Instanz, damit wir die Exposure runterfahren koennen,
            // ohne das geteilte Original-Material zu veraendern.
            if (RenderSettings.skybox != null)
            {
                himmel = new Material(RenderSettings.skybox);
                if (himmel.HasProperty("_Exposure"))
                    tagExposure = himmel.GetFloat("_Exposure");
                RenderSettings.skybox = himmel;
            }
            stunde = startStunde;
            warNacht = IstNacht;
            Anwenden();
        }

        void Update()
        {
            float stundenProSekunde = 24f / Mathf.Max(1f, tagLaengeSekunden);
            stunde += stundenProSekunde * Time.deltaTime;
            while (stunde >= 24f) { stunde -= 24f; tag++; }
            Anwenden();
            UebergangPruefen();
        }

        void UebergangPruefen()
        {
            bool jetztNacht = IstNacht;
            if (jetztNacht == warNacht) return;
            warNacht = jetztNacht;
            if (jetztNacht) NachtBeginnt?.Invoke(tag);
            else TagBeginnt?.Invoke(tag);
        }

        void Anwenden()
        {
            // Sonnenstand: 6:00 Horizont (Aufgang), 12:00 Zenit, 18:00 Horizont (Untergang),
            // 0:00 unter dem Horizont (Nacht).
            float elevation = (stunde / 24f) * 360f - 90f;
            transform.rotation = Quaternion.Euler(elevation, azimut, 0f);

            // Tagesfaktor: 1 tagsueber, weicher Uebergang in der Daemmerung, 0 nachts.
            float hoehe = Mathf.Sin(elevation * Mathf.Deg2Rad);
            float licht = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-0.15f, 0.25f, hoehe));

            sonne.intensity = licht * maxIntensitaet;   // Licht bleibt aktiv -> Skybox behaelt Sonnenrichtung
            RenderSettings.ambientLight = Color.Lerp(nachtAmbient, tagAmbient, licht);
            RenderSettings.reflectionIntensity = licht;
            if (himmel != null && himmel.HasProperty("_Exposure"))
                himmel.SetFloat("_Exposure", licht * tagExposure);   // Himmel faded auf schwarz
        }

        void OnGUI()
        {
            if (!uhrZeigen) return;
            int h = Mathf.FloorToInt(stunde);
            int m = Mathf.FloorToInt((stunde - h) * 60f);
            var style = new GUIStyle(GUI.skin.label) { fontSize = 20 };
            style.normal.textColor = Color.white;
            if (IstNacht) style.normal.textColor = new Color(0.6f, 0.75f, 1f);
            GUI.Label(new Rect(12, 8, 300, 30),
                      $"Tag {tag} · {h:00}:{m:00}{(IstNacht ? "  ☾" : "")}", style);
        }
    }
}
