# Changelog

Neuester Eintrag oben. Pro Eintrag: Datum, Meilenstein, ein bis drei Stichpunkte.
Nur was im Spiel gelandet ist.

---

## 2026-09-04 — M4: Der Loop schließt sich ⭐
- Eisen, Nahrung und Fackeln werden aus Truhen geplündert statt geschenkt; drei Gehöfte als Prefabs verteilt
- Nahrung tickt an der Spielzeit und treibt nach draußen; Fackel mit Brenndauer macht sichtbar, aber auch sichtbarer
- Zombies wandern von Gehöft zu Gehöft, wenn sie nichts hören; Sprung und Klettern kosten Ausdauer

## 2026-09-02 — M3: Die Nacht funktioniert
- Öffnungen verbarrikadierbar: Bretter nageln, ausbessern, abbauen — Eisen ist knapp
- Lärm- und Sichtsystem: Zombies folgen Geräuschen statt dem Spieler, Schleichen und Abwimmeln funktionieren
- Nacht-Zyklus mit Spawner und Horde alle 3 Nächte, Fenster-Durchsteigen, erzeugte Platzhalter-Sounds

## 2026-09-01 — M2: Es ist gefährlich
- NavMesh gebacken, Zombie (Kapsel + NavMeshAgent) verfolgt den Spieler und schlägt zu
- Spielerleben mit Anzeige, Tod lädt die Szene neu
- Ausdauer: Sprinten kostet, Regeneration im Wesentlichen nur im Stehen

## 2026-08-31 — M1: Es existiert
- FP-Controller (Starter Assets) in begehbarer Box-Welt
- Gehöft-Blockout aus Würfeln via `GehoeftBuilder` (Tür + 2 Fenster, Öffnungs-Marker)
- Tag/Nacht-Zyklus mit Bildschirm-Uhr, Nacht wird stockdunkel

## 2026-08-30 — Setup
- Projekt aufgesetzt (Unity 6, URP), Repo initialisiert
- Plan, Design und Arbeitsregeln festgehalten
