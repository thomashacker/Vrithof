# NEXT

_Nach jeder Session ausfüllen. Drei Zeilen. Das ist die Datei, die dich nach drei Wochen
Pause zurückholt._

---

**Zuletzt gemacht:**

> M2 fertig: NavMesh gebacken, Zombie (Kapsel + `NavMeshAgent`, Speed 1.8) verfolgt und
> schlägt zu, Tod lädt die Szene neu. Ausdauer im Zomboid-Stil — Regeneration praktisch
> nur im Stehen, Ausruhen kostet also Tageszeit.

**Läuft der Build?**

> Ja.

**Als Nächstes dran:**

> M3 / Session 3: `Openable` (Intakt/Verbarrikadiert/Zerstört), Interact-Raycast, Eisen
> (Start 20, sechs Öffnungen à 5), Lärm-System, Horde in Nacht 3.
> Auftakt: Zombie-Spawner am Kartenrand — aus M2 bewusst verschoben, weil M3 ihn für
> zehn Zombies sowieso braucht.

**Wo der Hund begraben liegt:**

> `TageszeitZyklus` stellt `RenderSettings` global um (Ambient → Flat, eigene Skybox-Instanz)
> — nur zur Laufzeit, revertet nach Play. Zum Beurteilen **Game-View im Play**, nicht Scene-View.
>
> Der `FirstPersonController` schleift an Wänden: er leitet seine Beschleunigung aus
> `_controller.velocity` ab (Move(), ~Zeile 166), und die ist an der Wand fast null.
> Skin Width 0.05 hat es nicht behoben. Fix ist eine Zeile, aber ein Eingriff ins
> Starter Asset — **bewusst vertagt**, spätestens vor M3 fällig (M3 spielt an Wänden).
>
> Agent Radius 0.5 gegen 1.2 m Türloch lässt nur ~0.2 m NavMesh in der Tür. Läuft, ist
> aber knapp — nach jedem Bake prüfen, ob der Zombie noch reinkommt. Sonst Radius 0.35.

---

**Aktueller Meilenstein:** M3
