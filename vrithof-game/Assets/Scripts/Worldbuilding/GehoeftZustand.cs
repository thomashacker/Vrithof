using UnityEngine;

namespace Vrithof.Worldbuilding
{
    /// M5a: verrammelt ein Gehoeft beim Start.
    ///
    /// Bis M4 stand jedes Haus offen — hinein, pluendern, hinaus, ohne Preis.
    /// Verrammelte Gehoefte machen Beute teuer: Aufbrechen kostet Zeit und macht
    /// Laerm, und mit wandernden Zombies ist beides ein Risiko. Damit dreht sich
    /// die Barrikaden-Mechanik vom Pflichtprogramm zum Werkzeug.
    ///
    /// Auf die Wurzel eines Gehoefts legen. Bei der eigenen Basis 'istBasis'
    /// anhaken — ein Zuhause, das man aufbrechen muss, ist keines.
    ///
    /// Laeuft in Start(), nicht in Awake: die Openables muessen ihre Bretter,
    /// Links und Sperren erst erzeugt haben.
    [DisallowMultipleComponent]
    public class GehoeftZustand : MonoBehaviour
    {
        [Header("Rolle")]
        [Tooltip("Die eigene Basis bleibt offen. Genau ein Gehoeft anhaken.")]
        public bool istBasis;

        [Header("Barrikaden")]
        [Tooltip("HP pro Brett fuer alle Oeffnungen dieses Gehoefts. 0 = die Werte " +
                 "am Openable bleiben stehen. Hier zentral, statt in achtzehn " +
                 "einzelnen Inspector-Feldern.")]
        public float brettHP = 120f;

        [Header("Verrammelung (nur wenn keine Basis)")]
        [Tooltip("Anteil der Oeffnungen, die ueberhaupt Bretter bekommen. Unter 1 " +
                 "bleibt immer mal ein Fenster offen — ein geschenkter Einstieg, " +
                 "wenn man ihn findet.")]
        [Range(0f, 1f)]
        public float anteilVerrammelt = 0.75f;
        [Tooltip("Anteil davon, der komplett zugenagelt ist statt nur halb.")]
        [Range(0f, 1f)]
        public float anteilVoll = 0.4f;

        void Start()
        {
            foreach (var o in GetComponentsInChildren<Openable>(true))
            {
                o.SetzeBrettHP(brettHP);   // auch in der Basis — sie wird nur nicht verrammelt

                if (istBasis) continue;
                if (Random.value > anteilVerrammelt) continue;

                int bretter = Random.value < anteilVoll
                    ? o.maxBretter
                    : Random.Range(1, o.maxBretter + 1);

                // Ueber Verstaerken() statt direkt ins Feld: so werden Anzeige,
                // NavMeshLink und Sperre gleich mitgezogen.
                for (int i = 0; i < bretter; i++) o.Verstaerken();
            }
        }
    }
}
