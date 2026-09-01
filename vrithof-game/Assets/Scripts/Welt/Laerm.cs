using System.Collections.Generic;
using UnityEngine;

namespace Vrithof.Welt
{
    /// Das Gedaechtnis der Welt fuer Geraeusche. Kein MonoBehaviour, kein
    /// Manager im Szenenbaum — wer Laerm macht, meldet ihn hier an; wer hoert,
    /// fragt nach. Sonst muesste ein Manager Spieler, Zombies und Barrikaden
    /// gleichzeitig kennen, und genau solche Knoten machen Projekte unbeweglich.
    ///
    /// Der Kern des Nacht-Gameplays: Zombies wissen nicht, wo du bist. Sie
    /// wissen nur, wo es zuletzt laut war. Rennen verraet dich, Haemmern zieht
    /// sie an die Wand, an der du arbeitest — und Stillstehen macht dich
    /// unsichtbar.
    public static class Laerm
    {
        struct Quelle
        {
            public Vector3 ort;
            public float staerke;   // zugleich die Reichweite in Metern
            public float bis;       // Zeitpunkt, ab dem es verhallt ist
        }

        static readonly List<Quelle> quellen = new List<Quelle>();

        /// Laerm anmelden. Die Staerke ist zugleich die Reichweite: ein Geraeusch
        /// der Staerke 20 ist 20 Meter weit zu hoeren.
        public static void Machen(Vector3 ort, float staerke, float dauer = 0.6f)
        {
            if (staerke <= 0f) return;
            quellen.Add(new Quelle { ort = ort, staerke = staerke, bis = Time.time + dauer });
        }

        /// Was hoert jemand an dieser Stelle am deutlichsten? Naeher und lauter
        /// gewinnt — ein leises Geraeusch direkt nebenan schlaegt ein lautes
        /// weit weg.
        public static bool Lautestes(Vector3 hoerer, out Vector3 ort)
        {
            Aufraeumen();

            ort = Vector3.zero;
            float beste = 0f;

            foreach (var q in quellen)
            {
                float d = Vector3.Distance(hoerer, q.ort);
                float wahrnehmung = q.staerke - d;   // ausserhalb der Reichweite negativ
                if (wahrnehmung <= beste) continue;
                beste = wahrnehmung;
                ort = q.ort;
            }

            return beste > 0f;
        }

        static void Aufraeumen()
        {
            for (int i = quellen.Count - 1; i >= 0; i--)
                if (quellen[i].bis <= Time.time)
                    quellen.RemoveAt(i);
        }

        /// Beim Szenenwechsel mitraeumen — statische Listen ueberleben sonst
        /// den Neustart und Zombies jagen Geraeusche von letzter Runde.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Zuruecksetzen() => quellen.Clear();
    }
}
