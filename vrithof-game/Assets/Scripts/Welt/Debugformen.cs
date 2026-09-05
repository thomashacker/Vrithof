using UnityEngine;

namespace Vrithof.Welt
{
    /// Gizmo-Formen fuers Einstellen. Unity zeichnet Gizmos auch im Play-Modus —
    /// in der Scene-View immer, in der Game-View ueber den "Gizmos"-Schalter oben
    /// rechts in der Leiste. Damit lassen sich Reichweiten drehen, waehrend man
    /// spielt, statt sie zu erraten.
    public static class Debugformen
    {
        /// Flacher Kreis auf Bodenhoehe. Lesbarer als eine Drahtkugel, weil man
        /// Entfernungen in der Ebene abschaetzt, nicht im Raum.
        public static void Bodenkreis(Vector3 mitte, float radius, int segmente = 48)
        {
            if (radius <= 0.01f) return;

            Vector3 vorher = mitte + new Vector3(radius, 0f, 0f);
            for (int i = 1; i <= segmente; i++)
            {
                float w = i / (float)segmente * Mathf.PI * 2f;
                Vector3 jetzt = mitte + new Vector3(Mathf.Cos(w), 0f, Mathf.Sin(w)) * radius;
                Gizmos.DrawLine(vorher, jetzt);
                vorher = jetzt;
            }
        }

        /// Blickfeld als Kuchenstueck: zwei Schenkel und ein Bogen.
        public static void Sichtkegel(Vector3 mitte, Vector3 richtung, float winkel,
                                      float weite, int segmente = 24)
        {
            if (weite <= 0.01f) return;

            richtung.y = 0f;
            richtung.Normalize();
            float halb = winkel * 0.5f;

            Vector3 links = Quaternion.Euler(0f, -halb, 0f) * richtung;
            Vector3 rechts = Quaternion.Euler(0f, halb, 0f) * richtung;
            Gizmos.DrawLine(mitte, mitte + links * weite);
            Gizmos.DrawLine(mitte, mitte + rechts * weite);

            Vector3 vorher = mitte + links * weite;
            for (int i = 1; i <= segmente; i++)
            {
                float w = -halb + winkel * (i / (float)segmente);
                Vector3 jetzt = mitte + (Quaternion.Euler(0f, w, 0f) * richtung) * weite;
                Gizmos.DrawLine(vorher, jetzt);
                vorher = jetzt;
            }
        }
    }
}
