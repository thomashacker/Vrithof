using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

namespace Vrithof.Spieler
{
    /// M3: Strg halten -> ducken. Langsamer, kleiner, und vor allem *leise*.
    ///
    /// Erst damit hat der Laerm drei Stufen statt zwei. Vorher war die Wahl
    /// "schnell und laut" oder "etwas langsamer und laut" — jetzt gibt es einen
    /// Weg, sich gar nicht zu verraten, und der kostet Zeit.
    ///
    /// Der FirstPersonController wird nicht angefasst: gedrueckt wird sein
    /// MoveSpeed, gehoben und gesenkt der CharacterController und das
    /// Kameraziel. Sprinten hebt das Ducken auf — sonst muesste sich diese
    /// Komponente mit der Ausdauer um denselben Wert streiten.
    ///
    /// Auf den Player legen.
    [RequireComponent(typeof(FirstPersonController))]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(StarterAssetsInputs))]
    public class Schleichen : MonoBehaviour
    {
        [Header("Ducken")]
        [Tooltip("Hoehe des Controllers im Ducken. Stehend sind es 2.")]
        public float duckHoehe = 1.2f;
        [Tooltip("Tempo im Ducken. Gehen sind 4.")]
        public float duckTempo = 1.6f;
        [Tooltip("Wie schnell hoch und runter. Hoeher = ruckartiger.")]
        public float uebergang = 9f;

        /// Fuer das Laerm-System: wer geduckt laeuft, macht kaum Geraeusch.
        public bool IstGeduckt { get; private set; }

        FirstPersonController fpc;
        CharacterController controller;
        StarterAssetsInputs eingabe;
        Transform kameraZiel;

        float standHoehe;
        float standKameraY;
        float vollesTempo;

        void Awake()
        {
            fpc = GetComponent<FirstPersonController>();
            controller = GetComponent<CharacterController>();
            eingabe = GetComponent<StarterAssetsInputs>();

            standHoehe = controller.height;
            vollesTempo = fpc.MoveSpeed;

            if (fpc.CinemachineCameraTarget != null)
            {
                kameraZiel = fpc.CinemachineCameraTarget.transform;
                standKameraY = kameraZiel.localPosition.y;
            }
        }

        void Update()
        {
            // Sprinten schlaegt Ducken. Wer rennt, schleicht nicht.
            bool will = Keyboard.current != null
                        && Keyboard.current.leftCtrlKey.isPressed
                        && !eingabe.sprint;

            IstGeduckt = will;

            float zielHoehe = will ? duckHoehe : standHoehe;
            float h = Mathf.Lerp(controller.height, zielHoehe, Time.deltaTime * uebergang);
            controller.height = h;
            // Fuesse bleiben am Boden: der Mittelpunkt sitzt auf halber Hoehe.
            controller.center = new Vector3(controller.center.x, h * 0.5f, controller.center.z);

            if (kameraZiel != null)
            {
                float ziel = standKameraY - (standHoehe - h);
                var p = kameraZiel.localPosition;
                kameraZiel.localPosition = new Vector3(p.x, ziel, p.z);
            }

            fpc.MoveSpeed = will ? duckTempo : vollesTempo;
        }
    }
}
