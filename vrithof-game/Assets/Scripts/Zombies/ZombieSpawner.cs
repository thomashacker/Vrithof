using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Vrithof.Zeit;

namespace Vrithof.Zombies
{
    /// M3: setzt bei Einbruch der Dunkelheit Zombies am Kartenrand ab und raeumt
    /// sie im Morgengrauen wieder weg.
    ///
    /// Das Wegraeumen ist der Punkt. Erst dadurch hat die Nacht ein *Ende* —
    /// und erst dann ist Reparieren eine Investition in etwas Erreichbares statt
    /// ein Hinauszoegern des Unvermeidlichen.
    ///
    /// Alle drei Naechte kommt eine Horde. Der Rhythmus ist ein Startwert, kein
    /// Ergebnis: er wird beim Spielen gedreht.
    ///
    /// Auf ein leeres GameObject legen.
    public class ZombieSpawner : MonoBehaviour
    {
        [Header("Referenzen")]
        public GameObject zombiePrefab;
        [Tooltip("Leer lassen — dann wird der Zyklus in der Szene gesucht.")]
        public TageszeitZyklus zyklus;

        [Header("Menge")]
        [Tooltip("Zombies in einer gewoehnlichen Nacht.")]
        public int normaleNacht = 3;
        [Tooltip("Jede wievielte Nacht eine Horde bringt.")]
        public int hordenRhythmus = 3;
        public int hordenGroesse = 10;

        [Header("Wo")]
        [Tooltip("Sie kommen von draussen — Abstand vom Spieler.")]
        public float minAbstand = 30f;
        public float maxAbstand = 45f;
        [Tooltip("Wie weit vom Zufallspunkt aus nach begehbarem Boden gesucht wird.")]
        public float navSuchradius = 8f;

        [Header("Morgengrauen")]
        [Tooltip("An: sie loesen sich im Morgengrauen auf. Aus: sie bleiben in " +
                 "der Welt, vergessen aber, wo sie dich zuletzt gehoert haben — " +
                 "dann steht der Tag nicht leer, gehoert aber wieder dir.")]
        public bool beiTagesanbruchEntfernen = false;
        [Tooltip("Obergrenze, damit sich ueber die Naechte keine Armee ansammelt. " +
                 "Ist sie erreicht, kommt niemand mehr dazu.")]
        public int maxGleichzeitig = 25;

        readonly List<GameObject> gespawnt = new List<GameObject>();
        Transform spieler;

        void Start()
        {
            if (zyklus == null) zyklus = FindAnyObjectByType<TageszeitZyklus>();
            if (zyklus == null)
            {
                Debug.LogWarning("ZombieSpawner findet keinen TageszeitZyklus.", this);
                return;
            }

            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) spieler = p.transform;

            zyklus.NachtBeginnt += NachtBeginnt;
            zyklus.TagBeginnt += TagBeginnt;
        }

        void OnDestroy()
        {
            if (zyklus == null) return;
            zyklus.NachtBeginnt -= NachtBeginnt;
            zyklus.TagBeginnt -= TagBeginnt;
        }

        void NachtBeginnt(int tag)
        {
            Aufraeumen();
            int anzahl = tag % hordenRhythmus == 0 ? hordenGroesse : normaleNacht;
            for (int i = 0; i < anzahl && gespawnt.Count < maxGleichzeitig; i++)
                Absetzen();
        }

        void TagBeginnt(int tag)
        {
            Aufraeumen();

            if (beiTagesanbruchEntfernen)
            {
                foreach (var z in gespawnt)
                    if (z != null) Destroy(z);
                gespawnt.Clear();
                return;
            }

            // Sie bleiben — verlieren aber die Spur. Ohne das wuerden sie den
            // ganzen Tag auf die Stelle zulaufen, an der sie dich nachts
            // zuletzt gehoert haben.
            foreach (var z in gespawnt)
            {
                if (z == null) continue;
                var sinne = z.GetComponent<ZombieSinne>();
                if (sinne != null) sinne.Vergessen();
            }
        }

        void Aufraeumen()
        {
            gespawnt.RemoveAll(z => z == null);
        }

        void Absetzen()
        {
            if (zombiePrefab == null) return;

            Vector3 mitte = spieler != null ? spieler.position : transform.position;
            Vector2 richtung = Random.insideUnitCircle.normalized;
            float weite = Random.Range(minAbstand, maxAbstand);
            Vector3 wunsch = mitte + new Vector3(richtung.x, 0f, richtung.y) * weite;

            // Der Zufallspunkt liegt selten genau auf begehbarem Boden.
            if (!NavMesh.SamplePosition(wunsch, out var treffer, navSuchradius, NavMesh.AllAreas))
                return;

            var z = Instantiate(zombiePrefab, treffer.position, Quaternion.identity);
            gespawnt.Add(z);

            // Sie kommen ja, weil sie etwas gehoert haben. Ohne diesen Startpunkt
            // stuenden sie am Kartenrand und warteten auf ein Geraeusch, das sie
            // aus 40 Metern nie hoeren wuerden.
            var sinne = z.GetComponent<ZombieSinne>();
            if (sinne != null) sinne.Merken(mitte, "gemeldet");
        }
    }
}
