# NEXT

_Nach jeder Session ausfüllen. Drei Zeilen. Das ist die Datei, die dich nach drei Wochen
Pause zurückholt._

---

**Zuletzt gemacht:**

> Nacht-Runde: Schlafsystem (Bett, Zeitraffer, Aufwachen bei Zombie-Nähe), Feuerstellen
> zum Anzünden (Wegmarke *und* Lockmittel), `Wegenetz` als Autoren-Werkzeug, `Weltgrenze`
> auf der Boden-Plane. Dazu Zombie-Schritte, Knistern, und die Nahrungsbalance neu
> gerechnet — der Tagesbedarf war fast so groß wie die Magenkapazität.

**Läuft der Build?**

> Ja. Deutlich besser spielbar — Orientierung nachts über Feuerstellen und Wege,
> Schlafen nimmt die Wartezeit.

**Als Nächstes dran:**

> **M5b, und darin zuerst der Mixamo-Rig.** Package `com.unity.animation.rigging`
> installieren, Zombie aus Kapsel zu echtem Modell machen (Idle/Walk/Attack als
> Humanoid), dann IK-Layer. Der Nahkampf kommt danach — Kampf zuerst zu bauen kann
> den Loop kaputtmachen.
> Alternativ die **Blender-Spur** (Gehöft-Kit, Verdecker) oder das **GATE**.

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
> **Nahrung hat keine erneuerbare Quelle** — Truhen sind einmalig, irgendwann ist
> Schluss. Im POC ist das der Loop, danach braucht es Jagd oder ein Feld.
> Siehe `02-DESIGN.md` Abschnitt 10.
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
