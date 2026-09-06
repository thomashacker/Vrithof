using UnityEngine;

namespace Vrithof.Worldbuilding
{
    /// Ein moeglicher Standort fuer eine Truhe. Der GehoeftBuilder verteilt
    /// diese Marker, beim Start wuerfelt jeder fuer sich.
    ///
    /// Zwei Ebenen Zufall, und beide zaehlen: hier faellt, *ob* ueberhaupt eine
    /// Truhe steht, im LootBehaelter dann, *was* drin ist. Ohne die erste Ebene
    /// wuesste man nach dem zweiten Gehoeft, wo man suchen muss.
    ///
    /// Wird vom Builder erzeugt, kann aber auch von Hand gesetzt werden.
    public class LootPlatz : MonoBehaviour
    {
        [Header("Was")]
        [Tooltip("Truhen-Prefab. Leer heisst: hier passiert nichts.")]
        public GameObject truhenPrefab;

        [Header("Wie wahrscheinlich")]
        [Range(0f, 1f)]
        public float chance = 0.6f;

        [Header("Streuung")]
        [Tooltip("Wie weit die Truhe vom Marker abweichen darf. Etwas Unordnung " +
                 "laesst die Gehoefte weniger nach Raster aussehen.")]
        public float versatz = 0.4f;
        [Tooltip("Zufaellige Drehung um die Hochachse.")]
        public bool drehen = true;

        void Start()
        {
            if (truhenPrefab == null || Random.value > chance) return;

            Vector2 ab = Random.insideUnitCircle * versatz;
            Vector3 wo = transform.position + new Vector3(ab.x, 0f, ab.y);
            Quaternion wie = drehen
                ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
                : transform.rotation;

            Instantiate(truhenPrefab, wo, wie, transform.parent);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.4f, 0.9f, 0.5f, 0.9f);
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.3f,
                                new Vector3(0.8f, 0.6f, 0.8f));
            Welt.Debugformen.Bodenkreis(transform.position, versatz, 20);
        }
    }
}
