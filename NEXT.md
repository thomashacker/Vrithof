# NEXT

_Nach jeder Session ausfüllen. Drei Zeilen. Das ist die Datei, die dich nach drei Wochen
Pause zurückholt._

---

**Zuletzt gemacht:**

> **Das GATE ist bestanden.** Fünf Tage am Stück gespielt, der Loop trägt.
> Antworten in `02-DESIGN.md` Abschnitt 5. Dazu eine Runde Politur: kein Klettern
> durch Türen, Bretter nur von innen abbaubar, Zombies kommen vom Weltrand,
> längere Dämmerung, Schlagen bremst das Tempo statt Zombies zurückzuwerfen.

**Läuft der Build?**

> Ja. Fünf Tage überlebt, ohne dass es langweilig wurde.

**Als Nächstes dran:**

> **A1 — Der Zombie.** Neuer Schwerpunkt, siehe `02-DESIGN.md` Abschnitt 11 und
> den Fahrplan in `01-PLAN.md`. Erster Schritt: **A1.1 Blender-Modell** im
> Valheim-Stil, humanoid-taugliche Proportionen für ein Mixamo-Rig, Trennstellen
> für Kopf und Arme in der Topologie vorsehen. **Das vorhandene geriggte Skelett
> aus Blender ist der bessere Start** — siehe Begründung in `01-PLAN.md`.

**Wo der Hund begraben liegt:**

> **Balance ist noch offen, drei Punkte aus dem GATE-Test:** Zombies sind allein
> zu harmlos (Tempo 0.45 gegen Gehen 4 — Vorschlag 2.0–2.6, dann `animationsTempo`
> mitziehen), mit der Axt zu leicht zu töten (`maxLeben` 150 statt 100), und sie
> treten zu selten in Gruppen auf (`normaleNacht` 5, `hordenGroesse` 12).
>
> **Barrikaden-HP** stehen am `GehoeftZustand` jedes Gehöfts (`brettHP`, 120).
>
> **Debug-Anzeigen kosten Leistung.** `zustandZeigen` am Zombie-Prefab und die
> Gizmos in der Game-View spürbar ausschalten, wenn es flüssig laufen soll —
> `Handles.Label` ist teuer. Beim Messen daran denken: `EditorLoop` war 68 % der
> Frame-Zeit und fällt im Build komplett weg.
>
> **Fehlende Animator-Parameter loggen jeden Frame** — das kostete mehr als die
> gesamte übrige Spiellogik. `ZombieAnimation` prüft sie jetzt beim Start und
> warnt einmal. Wenn eine Warnung kommt: Parameter im Controller nachtragen.
>
> **Editor-Setup am Zombie, das man nach einer Pause nicht mehr weiß:**
> Am Animator-Layer muss **IK Pass** an sein, sonst dreht sich der Kopf nicht.
> Blend-Tree-Thresholds sind **0 und 1** (der Parameter ist der Anteil am Grundtempo,
> nicht m/s). `Schrittfaktor` hängt als **Speed Multiplier am Lauf-State**, nicht global.
> Mixamo-Animationen brauchen **Bake Into Pose** für Rotation, Y und XZ (Based Upon:
> Center of Mass) — oder gleich „In Place" beim Download.
> Tempo wird **nur** am `NavMeshAgent → Speed` eingestellt, alles andere leitet sich ab.
>
> **Git LFS** ist für `*.fbx` und `*.blend` eingerichtet. GitHub nimmt keine Dateien
> über 100 MB direkt an. `Zombie Walk.fbx` ist versehentlich *mit* Skin geladen (106 MB
> statt 0,7 MB) — bei Gelegenheit „Without Skin" neu holen.
>
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

**Aktueller Meilenstein:** A1.1 — Blender-Modell
