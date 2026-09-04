# NEXT

_Nach jeder Session ausfüllen. Drei Zeilen. Das ist die Datei, die dich nach drei Wochen
Pause zurückholt._

---

**Zuletzt gemacht:**

> M4 fertig ⭐ **der POC steht.** Eisen wird aus Truhen geplündert statt geschenkt,
> drei Gehöfte als Prefabs verteilt, Nahrung tickt an der Spielzeit. Dazu Fackel mit
> Brenndauer (macht dich sichtbarer), wandernde Zombies (ziehen von Gehöft zu Gehöft),
> Ausdauer für Sprung und Klettern. Tageslänge jetzt in Minuten.

**Läuft der Build?**

> Ja. Deutlich lebendiger als M3 — Plündern, Nahrung und wandernde Zombies tragen den Loop.

**Als Nächstes dran:**

> Zwei Wege, beide legitim:
> **Session 5 — spielen und drehen.** Nichts bauen. Zahlen: Tageslänge, Loot-Menge,
> Nahrungsverbrauch, Entfernungen, Hordenrhythmus.
> **Oder die Werkzeug-Idee** aus `02-DESIGN.md` Abschnitt 9 — Eisen braucht eine zweite
> Senke, weil die Barrikaden als einzige nicht tragen.

**Wo der Hund begraben liegt:**

> **Verbarrikadieren macht keinen Spaß, und das ist nach M4 klarer geworden:** Der Loop
> trägt auch ohne. Das Problem ist nicht die Nacht, sondern dass **Eisen nur eine Senke
> hat**. Zwei Vorschläge in `02-DESIGN.md` Abschnitt 9 (Gehöfte aufbrechen, Axt aus Eisen),
> Befund in Abschnitt 10.
>
> **Offen aus dem M4-Test:** Schaust du beim Plündern auf die Uhr? Das ist das
> Fertig-Kriterium des POC und noch unbeantwortet.
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

**Aktueller Meilenstein:** Session 5 (spielen und drehen) — danach M3.5 oder M5
