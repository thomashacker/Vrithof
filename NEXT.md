# NEXT

_Nach jeder Session ausfüllen. Drei Zeilen. Das ist die Datei, die dich nach drei Wochen
Pause zurückholt._

---

**Zuletzt gemacht:**

> Eigener `SpielerController` ersetzt den Starter-Asset-Controller und `Schleichen`.
> Damit sind die Wandreibung und drei Umwege weg (Ausdauer und Schleichen haben fremde
> Werte verbogen, die Sprungsperre hing an der Skript-Reihenfolge). Dazu: Debug-Gizmos
> für Hörweite und Sichtkegel, Treffer-Feedback, und Sicht koppelt an die Tageszeit.

**Läuft der Build?**

> Ja. Nach dem Controller-Umbau: `FirstPersonController` und `Schleichen` müssen vom
> Player-Prefab runter, `SpielerController` drauf, `kameraZiel` auf `PlayerCameraRoot`.

**Als Nächstes dran:**

> Drei Wege, alle plan-konform:
> **Blender-Spur** (Session 7–8) — modulares Gehöft-Kit, und Blockout-Bäume als
> Verdecker. Die zweite Spur ist offen, seit der Loop steht.
> **M5b** — Nahkampf, Mixamo-Rig, IK-Layer, Grab-Mechanik.
> **Das GATE** — drei Abende spielen, die fünf Fragen beantworten.

**Wo der Hund begraben liegt:**

> **Debug-Ansicht:** Player wählen, Maus über Scene-View, `Shift+F` heftet die Kamera an
> ihn. Gizmos zeigen Hörweite (Kreis) und Sichtkegel — in der Game-View über den
> `Gizmos`-Schalter.
>
> **Die Atmosphäre wartet auf Verdecker.** Nebel und Sonnenstand sind drin, bringen
> aber wenig: Lichtstrahlen entstehen nur, wo etwas das Licht unterbricht, und die Welt
> ist flach und leer. Der Atmosphären-Abend gehört wiederholt, sobald Bäume und echte
> Geometrie stehen — dann lohnt auch volumetrisches Licht. Details in `02-DESIGN.md`
> Abschnitt 6.
>
> **Offen seit M4:** Schaust du beim Plündern auf die Uhr? Das ist das Fertig-Kriterium
> des POC und noch unbeantwortet. Ebenso ungeprüft: ob die Nacht mit den zäheren
> Barrikaden (120 HP statt 40) jetzt überstehbar *und* verlierbar ist.
>
> Ein Brett kostet weiter 4 Eisen, hält aber dreimal so lange — Eisen ist dadurch
> weniger knapp geworden. Falls es zu üppig wirkt: `eisenProBrett`, nicht die HP.
>
> `TageszeitZyklus` stellt `RenderSettings` global um — Game-View im Play beurteilen.
> Feld heißt jetzt `tagLaengeMinuten`, alter Sekundenwert ging beim Umbenennen verloren.
>
> Durchsteigen ist ein kontrolliertes Verschieben, kein echtes Klettern.
> `Assets/_Recovery/` ist ein Unity-Absturz-Artefakt, nicht eingecheckt — prüfen und löschen.
>
> **Balance ist ungeprüft.** Tageslänge (6 min), Loot-Mengen, Nahrungsverbrauch (3.5/h),
> Fackel-Brenndauer (6 h), Zombie-Tempo (1.8) sind Startwerte, keine Ergebnisse.

---

**Aktueller Meilenstein:** offen — Blender-Spur, M5b oder GATE
