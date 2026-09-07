# Changelog

Neuester Eintrag oben. Pro Eintrag: Datum, Meilenstein, ein bis drei Stichpunkte.
Nur was im Spiel gelandet ist.

---

## 2026-09-07 — GATE bestanden
- Fünf Tage am Stück gespielt, der Loop trägt; Antworten in `02-DESIGN.md` Abschnitt 5
- Neuer Schwerpunkt festgehalten: der Zombie (Abschnitt 11), Fahrplan A1 im Plan
- Politur: kein Klettern durch Türen, Bretter nur von innen, Zombies kommen vom Weltrand, längere Dämmerung, Schlagen bremst statt zurückzuwerfen

## 2026-09-07 — M5b: Nahkampf, Türen, Zustandsmaschine
- Nahkampf mit Axt: Ausholzeit statt Sofortschaden, Ausdauerkosten, Zombies mit Lebenspunkten, Rückstoß und Leichen
- Türblatt zum Öffnen und Schließen; Zombies wissen nichts mehr über offene Fenster auf der Rückseite
- Zombie-Verhalten als Zustandsmaschine (`ZombieSinne` getrennt), Loot-Plätze im GehoeftBuilder, Log-Spam im Animator behoben

## 2026-09-06 — M5b: Der Zombie
- Mixamo-Modell statt Kapsel: Animator mit Blend Tree, Tempo und Schrittgeschwindigkeit leiten sich aus einer einzigen Zahl ab
- Schlag mit Ausholzeit statt Sofortschaden — Ausweichen ist möglich —, Starre aus der Cliplänge und Vorwärtsschub beim Treffer
- Kopf folgt dem Spieler über Unitys eingebaute Blick-IK; Git LFS für FBX eingerichtet

## 2026-09-05 — Nacht spielbar machen
- Schlafsystem: Bett, Zeitraffer statt Warten, Aufwachen wenn ein Zombie nah kommt — damit haben die Barrikaden endlich einen Zweck
- Orientierung im Dunkeln: anzündbare Feuerstellen (Wegmarke und Lockmittel zugleich), `Wegenetz` zwischen den Gehöften, `Weltgrenze` gegen das Herunterfallen
- Zombie-Schritte und Feuer-Knistern; Nahrungsbalance neu gerechnet, Nachtnebel ausgedünnt

## 2026-09-05 — Eigener Player-Controller
- `SpielerController` ersetzt das Starter Asset und `Schleichen`: Wandreibung weg, Bodenkontakt ohne LayerMask, Ausdauer und Lärm hängen an sauberen Schaltern statt an verbogenen Fremdwerten
- Debug-Gizmos: Hörweite als Bodenkreis, Sichtkegel je Zombie mit echter Reichweite
- Treffer-Feedback (roter Schleier und Schlag); Sicht koppelt an die Tageszeit — nachts sehen Zombies schlechter, die Fackel holt es teilweise zurück

## 2026-09-05 — M3.5: Atmosphären-Abend
- Nebel im Tag/Nacht-Zyklus: drei Farben (Tag, Dämmerung, Nacht), nachts dichter
- Sonne steht nie senkrecht (max. 55°) — lange Schatten statt platter Draufsicht
- Alle Lärmquellen klingen jetzt: Wühlen, Klettern, und Zombies an den Brettern

## 2026-09-05 — M5a: Eisen bekommt Sinn
- Fremde Gehöfte starten verrammelt (`GehoeftZustand`, zufällig verteilt); Aufbrechen kostet Zeit und macht Lärm
- Axt aus Eisen am Amboss in der Basis: bricht 2,5× schneller auf, wird stumpf, wird dort wieder geschärft
- Barrikaden dreimal zäher, Nageln und Abbauen deutlich langsamer — die Arbeit gehört an den Abend

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
