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
        [Tooltip("Minuten fuer einen vollen 24h-Zyklus. Sobald es Entfernungen " +
                 "gibt, muss hier Luft sein: bei 6 Minuten dauert der helle Teil " +
                 "des Tages rund dreieinhalb Minuten.")]
        public float tagLaengeMinuten = 6f;
        [Range(0f, 24f)]
        [Tooltip("Uhrzeit beim Start.")]
        public float startStunde = 6f;

        [Header("Sonne")]
        [Tooltip("Himmelsrichtung der Sonne (Grad um die Y-Achse).")]
        public float azimut = 30f;
        [Tooltip("Hoechster Sonnenstand in Grad. Unter 90 steht sie nie senkrecht — " +
                 "das gibt lange Schatten statt platter Draufsicht und ist laut " +
                 "Design die halbe Miete am Valheim-Look.")]
        [Range(15f, 90f)]
        public float maxSonnenhoehe = 55f;

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

        [Header("Nebel")]
        [Tooltip("Laut Design der wichtigste Einzelfaktor fuer den Look — und " +
                 "zugleich das, was der Fackel nachts ueberhaupt erst einen " +
                 "Lichtkegel gibt.")]
        public bool nebel = true;
        public Color tagNebel = new Color(0.62f, 0.68f, 0.72f);
        [Tooltip("Daemmerung: warm gegen die kalten Schatten.")]
        public Color daemmerNebel = new Color(0.55f, 0.38f, 0.28f);
        public Color nachtNebel = new Color(0.03f, 0.04f, 0.06f);
        [Tooltip("Dichte am Tag. Hoeher heisst weniger Weitsicht — und weniger " +
                 "Ueberblick beim Planen der Route.")]
        public float tagDichte = 0.014f;
        [Tooltip("Dichte in der Nacht. Hoeher als am Tag, aber nicht blind: bei " +
                 "ExponentialSquared sieht man grob 2/Dichte weit — 0.025 sind also " +
                 "rund 80 Meter. Wer weiter sehen will, muss hier runter.")]
        public float nachtDichte = 0.025f;

        [Header("Uhr-Anzeige")]
        public bool uhrZeigen = true;

        /// Feuern beim Uebergang, mit der Nummer des angebrochenen Tages.
        /// Daran haengt der Spawner — der Zyklus selbst weiss nichts von Zombies.
        public event Action<int> NachtBeginnt;
        public event Action<int> TagBeginnt;

        public float Stunde => stunde;
        public int Tag => tag;
        /// 1 bei vollem Tag, 0 in tiefer Nacht, dazwischen die Daemmerung.
        /// Wer sich danach richten will (die Fackel tut es), fragt hier.
        public float Tageslicht { get; private set; } = 1f;
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
            float stundenProSekunde = 24f / (Mathf.Max(0.1f, tagLaengeMinuten) * 60f);
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
            // Tageskurve: 6:00 Horizont (Aufgang), 12:00 hoechster Stand,
            // 18:00 Horizont (Untergang), 0:00 tief darunter.
            float bogen = (stunde / 24f) * 360f - 90f;
            float hoehe = Mathf.Sin(bogen * Mathf.Deg2Rad);   // -1 .. 1

            // Nicht der rohe Bogen, sondern die begrenzte Hoehe: so steht die
            // Sonne auch mittags schraeg und wirft lange Schatten.
            transform.rotation = Quaternion.Euler(hoehe * maxSonnenhoehe, azimut, 0f);

            // Tagesfaktor: 1 tagsueber, weicher Uebergang in der Daemmerung, 0 nachts.
            float licht = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-0.15f, 0.25f, hoehe));
            Tageslicht = licht;

            sonne.intensity = licht * maxIntensitaet;   // Licht bleibt aktiv -> Skybox behaelt Sonnenrichtung
            RenderSettings.ambientLight = Color.Lerp(nachtAmbient, tagAmbient, licht);
            RenderSettings.reflectionIntensity = licht;
            if (himmel != null && himmel.HasProperty("_Exposure"))
                himmel.SetFloat("_Exposure", licht * tagExposure);   // Himmel faded auf schwarz

            Nebeln(licht);
        }

        // Zwei Abschnitte statt eines Verlaufs: Nacht -> Daemmerung -> Tag.
        // Ein einziges Lerp von Nacht nach Tag wuerde die warme Stunde
        // verschlucken, und genau die traegt das Bild.
        void Nebeln(float licht)
        {
            RenderSettings.fog = nebel;
            if (!nebel) return;

            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = licht < 0.5f
                ? Color.Lerp(nachtNebel, daemmerNebel, licht * 2f)
                : Color.Lerp(daemmerNebel, tagNebel, (licht - 0.5f) * 2f);
            RenderSettings.fogDensity = Mathf.Lerp(nachtDichte, tagDichte, licht);
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
