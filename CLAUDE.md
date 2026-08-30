# CLAUDE.md

Arbeitsregeln für dieses Projekt. Kurz halten — auch diese Datei.

## Projekt

**Vrîthof** — Mittelalter-Zombie-Survival, First Person, Unity URP.

Tagsüber Gehöfte nach **Eisen** plündern, abends Türen und Fenster verbarrikadieren,
nachts hält es oder nicht. Eisen ist immer knapp, Zombies zielen auf Lärm.

Details in `02-DESIGN.md`, aktueller Stand in `01-PLAN.md`, Wiedereinstieg in `NEXT.md`.

## Fokus

**PoC vor allem anderen.** M4 ist die erste spielbare Demo. Danach Feature für Feature.

Feature-Filter — immer beide Fragen stellen:

- Wie viel **Gameplay** bringt es?
- Wie **komplex** ist es?

> Viel Gameplay, wenig Komplexität → bauen.
> Wenig Gameplay, viel Komplexität → nicht bauen.
> _(Beispiel für die zweite Kategorie: prozedurale Beinanimation.)_

## Regeln

**Git**

- Committen und pushen **nur auf ausdrückliche Ansage**. Nie von selbst.
- Beim Commit `NEXT.md` und `CHANGELOG.md` mit aktualisieren.

**Changelog**

- `CHANGELOG.md` fortlaufend führen: neuester Eintrag oben.
- Pro Eintrag Datum, Meilenstein, ein bis drei Stichpunkte. Keine Prosa.
- Nur was tatsächlich im Spiel gelandet ist, keine Absichten.

**Dokumente**

- `01-PLAN.md` und `02-DESIGN.md` nur nach Rückfrage ändern.
- Nichts überschreiben, was nicht zur Änderung gehört.
- Alles kurz und aufgeräumt halten. Keine Textwände, keine Redundanz.
  Lieber eine Zeile streichen als drei hinzufügen.

**Neue Ideen**

- Erst Gegenfragen stellen, so weit eingrenzen wie möglich.
- Dann erst bewerten und einordnen.
- Nichts stillschweigend in den Scope aufnehmen.

**Vor der Umsetzung**

- Immer zuerst prüfen, ob es das schon gibt: Unity-Docs, Packages, Asset Store,
  bestehende Lösungen.
- Nichts selbst bauen, was Unity mitbringt.

**Überblick**

- Den Gesamtstand mitdenken, nicht nur die aktuelle Frage.
- Auf Scope Creep und Widersprüche zum Plan hinweisen.
