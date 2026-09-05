using UnityEngine;

namespace Vrithof.Worldbuilding
{
    /// M5: das Bett. Reine Markierung mit ein paar Zahlen — die Mechanik steckt
    /// in Schlafen.cs am Spieler.
    ///
    /// Ortsgebunden wie der Amboss, und aus demselben Grund: es gibt der Basis
    /// einen Zweck. Und es beantwortet nebenbei die Frage, die seit M3 offen
    /// steht — warum man sich ueberhaupt verbarrikadieren sollte. Nicht um der
    /// Barrikade willen, sondern um schlafen zu koennen.
    ///
    /// Auf einen Wuerfel in der Basis legen.
    public class Bett : MonoBehaviour
    {
        [Tooltip("Wie stark die Zeit beim Schlafen laeuft. Die Welt laeuft mit — " +
                 "Zombies bewegen sich also wirklich auf einen zu, statt dass die " +
                 "Nacht einfach uebersprungen wuerde.")]
        [Range(2f, 20f)]
        public float zeitraffer = 8f;

        [Tooltip("Ab dieser Entfernung weckt ein Zombie den Spieler.")]
        public float weckDistanz = 14f;
    }
}
