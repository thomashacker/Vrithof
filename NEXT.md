# NEXT

_Nach jeder Session ausfüllen. Drei Zeilen. Das ist die Datei, die dich nach drei Wochen
Pause zurückholt._

---

**Zuletzt gemacht:**

> **M5b fertig.** Nahkampf mit der Axt (Ausholzeit, Ausdauerkosten, Zombies mit
> Lebenspunkten und Leichen), Türblatt zum Auf- und Zumachen, Loot-Plätze im
> GehoeftBuilder. Das Zombie-Verhalten ist eine Zustandsmaschine geworden und
> `ZombieSinne` steckt in einer eigenen Datei.

**Läuft der Build?**

> Ja, und flüssig — auch mit 150 Zombies im Stresstest.

**Als Nächstes dran:**

> **Das GATE.** Drei Abende richtig spielen, dann die fünf Fragen aus `02-DESIGN.md`
> Abschnitt 5 ehrlich beantworten. Alle Meilensteine sind durch.
> Offen geblieben aus M5b: Arme greifen per `TwoBoneIKConstraint`, Grab-Mechanik.
> Alternativ die **Blender-Spur** (Gehöft-Kit, Verdecker).

**Wo der Hund begraben liegt:**

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

**Aktueller Meilenstein:** GATE
