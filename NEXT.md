# NEXT

_Nach jeder Session ausfüllen. Drei Zeilen. Das ist die Datei, die dich nach drei Wochen
Pause zurückholt._

---

**Zuletzt gemacht:**

> M3 fertig: Öffnungen mit Brettern (nageln, ausbessern, abbauen), Eisen als knappe
> Ressource, Lärm-System mit drei Stufen (schleichen/gehen/sprinten) plus Hämmern,
> Zombies folgen Lärm und Sicht statt dem Spieler, Nacht-Zyklus mit Spawner und
> Horde alle 3 Nächte. Dazu Schleichen, Fenster-Durchsteigen und erzeugte Platzhalter-Sounds.

**Läuft der Build?**

> Ja. Abwimmeln und Schleichen funktionieren und machen Spaß.

**Als Nächstes dran:**

> M4 / Session 4 ⭐ **der eigentliche POC**: Loot-Container mit Loot-Tabelle (Eisen wird
> geplündert statt geschenkt), zweites und drittes Gehöft in Laufentfernung, Nahrung tickt.

**Wo der Hund begraben liegt:**

> **Die Barrikaden haben keinen Anreiz.** Weglaufen ist billiger als verteidigen — im
> M3-Test gab es keinen Grund, die Hütte zu halten. Ausführlich in `02-DESIGN.md`
> Abschnitt 10. Die wichtigste offene Designfrage, wird mit M4 dringend.
>
> `TageszeitZyklus` stellt `RenderSettings` global um — nur zur Laufzeit, revertet nach
> Play. Zum Beurteilen **Game-View im Play**, nicht Scene-View.
>
> Der `FirstPersonController` schleift an Wänden (`_controller.velocity` in Move(), ~Zeile
> 166). **Wird nicht repariert** — er fliegt später ganz raus, siehe `02-DESIGN.md`
> Abschnitt 9 (eigener Controller mit Gewicht).
>
> Durchsteigen ist ein kontrolliertes Verschieben, kein echtes Klettern. Zombies wandern
> nicht: ohne Geräusch bleiben sie am letzten stehen. Agent Radius 0.5 gegen 1.2 m
> Türloch ist knapp — nach jedem Bake prüfen.
>
> **Balance ist ungeprüft.** Startvorrat (40 Eisen), Hordenrhythmus (jede 3. Nacht),
> Barrikaden-HP, Zombie-Tempo (1.8) sind Startwerte, keine Ergebnisse.

---

**Aktueller Meilenstein:** M4
