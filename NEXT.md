# NEXT

_Nach jeder Session ausfüllen. Drei Zeilen. Das ist die Datei, die dich nach drei Wochen
Pause zurückholt._

---

**Zuletzt gemacht:**

> M5a fertig: Fremde Gehöfte starten verrammelt (`GehoeftZustand` würfelt das aus),
> Aufbrechen kostet Zeit und Lärm. Axt aus Eisen am Amboss in der Basis — bricht 2,5×
> schneller auf, wird stumpf, wird dort geschärft. Barrikaden dreimal zäher, Nageln
> und Abbauen langsamer.

**Läuft der Build?**

> Ja. Verrammelte Häuser und die Axt machen das Plündern spürbar besser.

**Als Nächstes dran:**

> **M3.5 — Atmosphären-Abend.** Genau ein Abend: Nebel, Post-Processing-Volume,
> Sonnenwinkel. Kein Modellieren. Die Fackel gibt es schon, sie kommt ohne Nebel nur
> nicht zur Geltung. Danach M5b (Nahkampf, Mixamo-Rig, IK, Grab), dann das GATE.

**Wo der Hund begraben liegt:**

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

**Aktueller Meilenstein:** M3.5
