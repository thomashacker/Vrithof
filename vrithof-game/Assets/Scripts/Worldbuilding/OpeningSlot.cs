using UnityEngine;

namespace Vrithof.Worldbuilding
{
    public enum OpeningKind { Tuer, Fenster }

    /// Marker an einer Wand-Oeffnung. Haelt Art und Groesse, damit spaeter (M3)
    /// das Barrikaden-Prefab genau hier einrastet. Zeichnet die Oeffnung als Gizmo,
    /// damit man sie im Editor sieht.
    public class OpeningSlot : MonoBehaviour
    {
        public OpeningKind kind = OpeningKind.Fenster;
        public float width = 1.2f;
        public float height = 1.2f;

        void OnDrawGizmos()
        {
            Gizmos.color = kind == OpeningKind.Tuer
                ? new Color(1f, 0.6f, 0.1f)   // Tuer: orange
                : new Color(0.2f, 0.7f, 1f);  // Fenster: blau
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(width, height, 0.05f));
        }
    }
}
