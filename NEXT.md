# NEXT

_Nach jeder Session ausfüllen. Drei Zeilen. Das ist die Datei, die dich nach drei Wochen
Pause zurückholt._

---

**Zuletzt gemacht:**

> M3.5 fertig: Nebel im Tag/Nacht-Zyklus (Tag, Dämmerung, Nacht — nachts dichter),
> Sonne steht nie senkrecht, ACES und Color Adjustments im Volume. Dazu klingen jetzt
> alle Lärmquellen: Wühlen, Klettern, und Zombies, die an den Brettern arbeiten.

**Läuft der Build?**

> Ja. Die Schlaggeräusche an den Brettern sind der größte Gewinn — man hört im Dunkeln,
> an welcher Wand sie stehen.

**Als Nächstes dran:**

> Drei Wege, alle plan-konform:
> **Blender-Spur** (Session 7–8) — modulares Gehöft-Kit, und Blockout-Bäume als
> Verdecker. Die zweite Spur ist offen, seit der Loop steht.
> **M5b** — Nahkampf, Mixamo-Rig, IK-Layer, Grab-Mechanik.
> **Das GATE** — drei Abende spielen, die fünf Fragen beantworten.

**Wo der Hund begraben liegt:**

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
> Der `FirstPersonController` schleift an Wänden. **Wird nicht repariert** — eigener
> Controller mit Gewicht kommt später, siehe `02-DESIGN.md` Abschnitt 9.
>
> Ausdauer blockiert den Sprung, indem sie die Taste löscht. Bei ungünstiger
> Skript-Reihenfolge kann ein einzelner Sprung durchrutschen — bewusst in Kauf genommen,
> um das Starter Asset nicht zu forken.
>
> Durchsteigen ist ein kontrolliertes Verschieben, kein echtes Klettern.
> `Assets/_Recovery/` ist ein Unity-Absturz-Artefakt, nicht eingecheckt — prüfen und löschen.
>
> **Balance ist ungeprüft.** Tageslänge (6 min), Loot-Mengen, Nahrungsverbrauch (3.5/h),
> Fackel-Brenndauer (6 h), Zombie-Tempo (1.8) sind Startwerte, keine Ergebnisse.

---

**Aktueller Meilenstein:** offen — Blender-Spur, M5b oder GATE
