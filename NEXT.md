# NEXT

_Nach jeder Session ausfüllen. Drei Zeilen. Das ist die Datei, die dich nach drei Wochen
Pause zurückholt._

---

**Zuletzt gemacht:**

> M1 fertig: FP-Controller läuft, Gehöft-Blockout via `GehoeftBuilder` (Tür + 2 Fenster,
> Öffnungs-Marker), Tag/Nacht-Zyklus mit Uhr — Nacht wird stockdunkel (Sonne, Ambient,
> Reflexion, Skybox-Exposure faden gemeinsam).

**Läuft der Build?**

> Ja — in Play/Game-View. Dunkelheit nur zur Laufzeit sichtbar.

**Als Nächstes dran:**

> M2 / Session 2: NavMesh backen, Zombie = Kapsel mit NavMeshAgent + `SetDestination(player)`.
> Berührung → Schaden → Tod → Szene neu laden.

**Wo der Hund begraben liegt:**

> `TageszeitZyklus` stellt `RenderSettings` global um (Ambient → Flat, eigene Skybox-Instanz)
> — nur zur Laufzeit, revertet nach Play. Zum Beurteilen **Game-View im Play**, nicht Scene-View.

---

**Aktueller Meilenstein:** M2
