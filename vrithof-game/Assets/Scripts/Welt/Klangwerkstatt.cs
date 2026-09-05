using UnityEngine;

namespace Vrithof.Welt
{
    /// Platzhalter-Klaenge, im Code erzeugt. Das Projekt hat keine Audio-Dateien,
    /// und fuer ein Blockout muss es die auch nicht haben: es geht um Feedback,
    /// nicht um Klangqualitaet. Dieselbe Logik wie bei den grauen Wuerfeln —
    /// spaeter werden echte Clips in die Inspector-Slots gezogen, der Code
    /// bleibt.
    public static class Klangwerkstatt
    {
        const int Rate = 44100;

        /// Dumpfer Aufschlag. Schwer = Sprint, leicht = Gehen.
        public static AudioClip Schritt(bool schwer)
        {
            int n = Rate / 8;                       // 0.125 s
            var d = new float[n];
            float glatt = 0f;
            float traegheit = schwer ? 0.30f : 0.45f;   // je traeger, desto dumpfer
            float abfall = schwer ? 14f : 22f;

            for (int i = 0; i < n; i++)
            {
                float t = i / (float)n;
                glatt = Mathf.Lerp(glatt, Random.Range(-1f, 1f), traegheit);
                d[i] = glatt * Mathf.Exp(-t * abfall) * (schwer ? 0.9f : 0.55f);
            }
            return Aus(d, schwer ? "SchrittSchwer" : "SchrittLeicht");
        }

        /// Kurzer harter Schlag mit etwas Ton drin — Hammer auf Nagel.
        public static AudioClip Hammer()
        {
            int n = Rate / 12;                      // 0.083 s
            var d = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)n;
                float sek = i / (float)Rate;
                float ton = Mathf.Sin(2f * Mathf.PI * 850f * sek);
                d[i] = (Random.Range(-1f, 1f) * 0.45f + ton * 0.55f) * Mathf.Exp(-t * 45f);
            }
            return Aus(d, "Hammer");
        }

        /// Rascheln in einer Truhe. Heller und unruhiger als ein Schritt —
        /// mehrere kleine Stoesse statt eines Schlags.
        public static AudioClip Wuehlen()
        {
            int n = Rate / 4;                       // 0.25 s
            var d = new float[n];
            float glatt = 0f;

            for (int i = 0; i < n; i++)
            {
                float t = i / (float)n;
                glatt = Mathf.Lerp(glatt, Random.Range(-1f, 1f), 0.6f);
                float stoesse = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 5f));
                d[i] = glatt * stoesse * (1f - t) * 0.45f;
            }
            return Aus(d, "Wuehlen");
        }

        /// Scharren beim Hochziehen durchs Fenster. Rau, mit Anlauf und Ende.
        public static AudioClip Scharren()
        {
            int n = Rate * 2 / 5;                   // 0.4 s
            var d = new float[n];
            float glatt = 0f;

            for (int i = 0; i < n; i++)
            {
                float t = i / (float)n;
                glatt = Mathf.Lerp(glatt, Random.Range(-1f, 1f), 0.32f);
                d[i] = glatt * Mathf.Sin(Mathf.PI * t) * 0.5f;
            }
            return Aus(d, "Scharren");
        }

        /// Dumpfer Schlag auf Holz. Was man hoert, wenn sie an den Brettern
        /// arbeiten — und damit die einzige Information darueber, an welcher
        /// Wand sie gerade stehen.
        public static AudioClip HolzSchlag()
        {
            int n = Rate / 6;                       // 0.17 s
            var d = new float[n];
            float glatt = 0f;

            for (int i = 0; i < n; i++)
            {
                float t = i / (float)n;
                float sek = i / (float)Rate;
                glatt = Mathf.Lerp(glatt, Random.Range(-1f, 1f), 0.22f);   // sehr dumpf
                float ton = Mathf.Sin(2f * Mathf.PI * 150f * sek);
                d[i] = (glatt * 0.6f + ton * 0.4f) * Mathf.Exp(-t * 16f) * 0.8f;
            }
            return Aus(d, "HolzSchlag");
        }

        /// Tiefes Stoehnen. Bewusst lang und leise — es soll Richtung verraten,
        /// nicht erschrecken.
        public static AudioClip Stoehnen()
        {
            int n = Rate * 5 / 4;                   // 1.25 s
            var d = new float[n];
            float phase = 0f;

            for (int i = 0; i < n; i++)
            {
                float t = i / (float)n;
                float sek = i / (float)Rate;
                // Grundton wandert leicht — sonst klingt es wie ein Signalton.
                float hz = 96f + Mathf.Sin(sek * 5.5f) * 9f;
                phase += 2f * Mathf.PI * hz / Rate;

                float huelle = Mathf.Sin(Mathf.PI * t);          // sanft rein und raus
                d[i] = (Mathf.Sin(phase) * 0.7f + Random.Range(-1f, 1f) * 0.3f)
                       * huelle * 0.5f;
            }
            return Aus(d, "Stoehnen");
        }

        /// Treffer am eigenen Leib. Tief und kurz — mehr Wucht als Klang.
        public static AudioClip Treffer()
        {
            int n = Rate / 5;                       // 0.2 s
            var d = new float[n];
            float glatt = 0f;

            for (int i = 0; i < n; i++)
            {
                float t = i / (float)n;
                float sek = i / (float)Rate;
                glatt = Mathf.Lerp(glatt, Random.Range(-1f, 1f), 0.18f);
                float wumms = Mathf.Sin(2f * Mathf.PI * 70f * sek);
                d[i] = (glatt * 0.45f + wumms * 0.55f) * Mathf.Exp(-t * 12f) * 0.9f;
            }
            return Aus(d, "Treffer");
        }

        static AudioClip Aus(float[] daten, string name)
        {
            var clip = AudioClip.Create(name, daten.Length, 1, Rate, false);
            clip.SetData(daten, 0);
            return clip;
        }
    }
}
