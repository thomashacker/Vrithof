using UnityEngine;

namespace Vrithof.Welt
{
    /// Gemeinsame Stile fuer die Bildschirmanzeigen.
    ///
    /// Der Grund ist nicht Ordnung, sondern Muell: `new GUIStyle(...)` in einer
    /// OnGUI-Methode legt bei *jedem* Aufruf ein Objekt an, und OnGUI laeuft
    /// mehrfach pro Frame. Bei einem Dutzend Anzeigen kamen so ueber hundert
    /// Kilobyte pro Frame zusammen — der Garbage Collector raeumt das in
    /// Schueben auf, und diese Schuebe sind genau das, was man als Ruckeln
    /// wahrnimmt.
    ///
    /// Hier wird jeder Stil einmal gebaut und danach nur noch eingefaerbt.
    public static class Anzeige
    {
        static GUIStyle zahl;
        static GUIStyle klein;
        static GUIStyle mitte;
        static GUIStyle mitteGross;

        /// Zahlenanzeige oben links (20 pt).
        public static GUIStyle Zahl(Color farbe) => Faerben(ref zahl, 20, TextAnchor.UpperLeft, farbe);

        /// Kleine Beschriftung (13 pt).
        public static GUIStyle Klein(Color farbe) => Faerben(ref klein, 13, TextAnchor.UpperLeft, farbe);

        /// Zentrierter Hinweis am Fadenkreuz (16 pt).
        public static GUIStyle Mitte(Color farbe) => Faerben(ref mitte, 16, TextAnchor.MiddleCenter, farbe);

        /// Zentrierte Meldung, groesser (20 pt).
        public static GUIStyle MitteGross(Color farbe) => Faerben(ref mitteGross, 20, TextAnchor.MiddleCenter, farbe);

        static GUIStyle Faerben(ref GUIStyle stil, int groesse, TextAnchor lage, Color farbe)
        {
            // Erst beim ersten Zeichnen bauen: GUI.skin gibt es ausserhalb von
            // OnGUI nicht, ein Feldinitialisierer wuerde also fehlschlagen.
            if (stil == null)
                stil = new GUIStyle(GUI.skin.label)
                {
                    fontSize = groesse,
                    alignment = lage,
                    wordWrap = false
                };

            stil.normal.textColor = farbe;
            return stil;
        }

        /// Balken mit dunklem Hintergrund. Spart in jeder Anzeige vier Zeilen
        /// und dieselbe Farbjonglage.
        public static void Balken(Rect rahmen, float anteil, Color farbe)
        {
            var alt = GUI.color;

            GUI.color = new Color(0f, 0f, 0f, 0.5f);
            GUI.DrawTexture(rahmen, Texture2D.whiteTexture);

            GUI.color = farbe;
            rahmen.width *= Mathf.Clamp01(anteil);
            GUI.DrawTexture(rahmen, Texture2D.whiteTexture);

            GUI.color = alt;
        }
    }
}
