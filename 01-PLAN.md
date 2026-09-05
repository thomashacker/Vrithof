# 01 — PLAN

**Mittelalter-Zombie-Survival · Meilensteine & Fahrplan**
*Das ist das Arbeitsdokument. Warum die Dinge so entschieden sind, steht in `02-DESIGN.md`.*

---

## Wo stehe ich?

- [x] M1 — Es existiert
- [x] M2 — Es ist gefährlich
- [x] M3 — Die Nacht funktioniert
- [ ] M3.5 — Atmosphären-Abend
- [x] M4 — Der Loop schließt sich ⭐ **← der eigentliche POC**
- [x] M5a — Eisen bekommt Sinn
- [ ] M5b — Es fühlt sich gut an
- [ ] **GATE** — die ehrliche Frage
- [ ] Ausbaustufe 2

---

## Meilensteine

Jeder Meilenstein endet mit etwas Spielbarem. Nach jedem kannst du wochenlang Pause
machen und wieder einsteigen.

### M1 — Es existiert
FP-Controller in einer grauen Box-Welt. Ein Gehöft-Blockout mit Fenstern und Tür.
Tag/Nacht-Zyklus läuft sichtbar durch.

> **Fertig, wenn:** du herumlaufen und zusehen kannst, wie es dunkel wird.

### M2 — Es ist gefährlich
Ein Zombie spawnt am Kartenrand und läuft auf dich zu. Er kann dich töten. Du kannst
weglaufen. Nahkampf noch nicht.

> **Fertig, wenn:** du vor etwas wegrennst und beim Tod neu startest.

### M3 — Die Nacht funktioniert
Fenster und Tür verbarrikadierbar. In Nacht 3 kommen zehn Zombies. Barrikaden haben HP
und gehen kaputt.

**Das Eisen darf nicht reichen.** Sechs Öffnungen, Barrikade kostet 5, du startest mit
20. Du kannst drei sichern und musst drei aufgeben. Eine Ressource, die reicht, ist
keine Ressource.

**Die Öffnungen müssen unterschiedlich sein**, sonst ist die Wahl beliebig und beliebig
ist keine Strategie: Tür hält länger, aber wenn sie fällt, kommen alle rein. Fenster
sind schwach, aber einzeln. Ein offen gelassenes Fenster ist dein Fluchtweg. Das Gehöft
hat einen inneren Raum — äußeren Ring sichern oder zurückziehen und den Rest opfern?
Reine Level-Geometrie, kein Code.

**Zombies zielen auf Lärm, nicht auf den Spieler.** Sie laufen aufs Gebäude zu und
schlagen auf die nächste Öffnung, aber der lauteste Punkt zieht sie um. Reparieren ist
Hämmern, also laut — du reparierst die Nordwand und ziehst dadurch mehr Zombies zur
Nordwand. Rennen ist laut, Schleichen langsam. Damit ist die Nacht ein Abwägen statt
eine Rundlaufstrecke. ~20 Zeilen.

*Kampf gibt es bewusst noch nicht: Wenn du Zombies erschlagen kannst, brauchst du
vielleicht keine Barrikaden — dann testest du dein Kampfsystem statt den Loop.*

> **Fertig, wenn:** eine Nacht überstanden werden *kann* — und auch verloren gehen kann.
> **Fertig, wenn:** du während der Nacht eine Öffnung bewusst aufgegeben hast.

### M3.5 — Atmosphären-Abend 🎨
Genau ein Abend, dann Deckel drauf. Nebel, Post-Processing, Sonnenwinkel, eine Fackel in
der Dunkelheit. Motivations-Treibstoff, kein Feature-Kriechen.

> **Fertig, wenn:** ein Screenshot bei Dämmerung gut aussieht.
> **Abgebrochen, wenn:** du anfängst zu modellieren, um es schöner zu machen.

### M4 — Der Loop schließt sich ⭐
Eisen wird nicht mehr geschenkt, sondern aus Containern geplündert. Zweites und drittes
Gehöft, weiter weg. Nahrung tickt und zwingt dich raus.

**Hier entsteht die eigentliche Strategie**, und zwar am Tag, nicht in der Nacht: Wie
weit gehe ich? Nehme ich das Eisen für die Barrikade oder für ein Werkzeug? Schaffe ich
noch ein Haus, bevor es dunkel wird? Was ich nachts verheize, fehlt mir morgen.

> **Fertig, wenn:** du beim Plündern auf die Uhr schaust.
> **Das ist der POC.** Ab hier weißt du, ob der Loop trägt.

### M5 — Es fühlt sich gut an
Zwei Hälften, die erste ist die wichtigere.

**5a — Eisen bekommt Sinn.** Aus M4 gelernt: Der Loop trägt, aber Eisen hat nur eine
Senke, und die macht keinen Spaß. Zwei Änderungen beheben das gemeinsam:

- **Fremde Gehöfte starten verbarrikadiert.** Beute kostet damit Zeit und Lärm statt
  nichts. Barrikaden werden vom Pflichtprogramm zum Werkzeug.
- **Axt aus Eisen.** Bricht schnell auf, wo bloße Hände lange brauchen. Wird stumpf,
  kostet Eisen zur Reparatur. Damit schließt sich der Kreis: Eisen verschafft Zugang
  zu Beute, Beute bringt Eisen. **Ohne Werkbank, ohne Rezeptbaum** — beides bleibt auf
  der Nein-Liste.

**5b — Es fühlt sich gut an.** Nahkampf mit Ausdauer und Verletzung (die Axt taugt
dafür ohnehin). Zombies mit Mixamo-Rig statt Kapseln, plus IK-Layer (Blick folgt dir,
Arme greifen nach dir). Grab-Mechanik: Zombie hält dich fest, du musst dich befreien.

*Fackeln und "Geräusche ziehen Zombies an" standen ursprünglich hier — beides ist in
M3 und M4 schon entstanden.*

> **Fertig, wenn:** du losziehst, um Eisen für die Axt zu holen.
> **Fertig, wenn:** dir ein Zombie im Dunkeln einen Schreck einjagt.

### GATE — die ehrliche Frage
Drei Abende richtig spielen, dann die fünf Fragen in `02-DESIGN.md` beantworten.

---

## Fahrplan

Zeitangaben sind grob. Wichtig ist die **Reihenfolge**, nicht die Dauer.

### Session 1 — ~3h → M1
- Unity-Projekt, URP-Template
- **Starter Assets: First Person Controller** importieren (spart einen ganzen Abend)
- Flaches Terrain, ein paar Hügel, fertig
- **Gehöft-Blockout aus Unity-Würfeln.** Noch kein Blender. Vier Wände, ein Türloch,
  zwei Fensterlöcher, ein Dach
- Tag/Nacht: ein Script, das die Directional Light rotiert, plus eine Uhr auf dem Screen

### Session 2 — ~3h → M2
- NavMesh backen
- Zombie = Kapsel mit NavMeshAgent, `SetDestination(player)`
- Berührung → Schaden → Tod → `SceneManager.LoadScene(current)`
- Ausdauer an den Controller hängen: Sprinten kostet, Regeneration im Stehen

### Session 3 — ~4h → M3
- `Openable`-Component: Enum `Intakt / Verbarrikadiert / Zerstört`, HP, Model-Swap
- Interact-Raycast: E auf Öffnung → verbarrikadieren, zieht Eisen ab
- **Reparieren derselben Öffnung, auch während der Nacht** ← das Nacht-Gameplay
- Eisen startet bei 20, sechs Öffnungen à 5 — **es darf nicht reichen**
- Tür und Fenster mit unterschiedlichen HP-Werten, Gehöft mit innerem Raum
- **Lärm-System:** Zombies zielen auf den lautesten Punkt, nicht auf den Spieler.
  Hämmern und Rennen sind laut, Schleichen leise
- Nacht 3: zehn Zombies spawnen am Kartenrand

### Session 4 — ~3h → M4 ⭐
- Loot-Container mit Loot-Tabelle, Eisen wird geplündert statt geschenkt
- Zweites und drittes Gehöft in Laufentfernung
- Nahrung tickt herunter

**Jetzt ist der POC spielbar.**

### Session 5 — Spielen und drehen
Nicht bauen. Spielen. Horden-Größe, Eisen-Kosten, Tageslänge, Laufgeschwindigkeit,
Barrikaden-HP, Entfernung zum nächsten Gehöft. Alles davon sind Zahlen im Inspector.

### Session 6 — ~2h → M3.5
Nebel, Post-Processing-Volume, Sonnenwinkel, Fackel. Deckel drauf.

### Session 7–8 — Blender: das Gehöft
Jetzt weißt du aus dem Blockout, was das Gehöft können muss. Modulares Kit:

> Wand massiv · Wand mit Fenster · Wand mit Tür · Boden · Dachsegment · Eckpfosten ·
> Tür (3 Zustände) · Fenster (3 Zustände) · Barrikaden-Brett · Truhe

Die Unity-Würfel werden gegen die Modelle getauscht. Bei sauberer Prefab-Struktur: ein
Nachmittag.

### Session 9+ — M5 (erst 5a, dann 5b), dann das GATE

---

## Arbeitsregeln

Deine Motivation schwankt — das ist die wichtigste Rahmenbedingung des Projekts.
Der Plan ist darauf gebaut:

1. **Jede Session endet mit einem lauffähigen Build.** Nie mit einem halben Refactor.
2. **`NEXT.md` pflegen.** Drei Zeilen: was zuletzt lief, was als Nächstes dran ist, wo
   der Hund begraben liegt. Rettet dich nach drei Wochen Pause.
3. **Keine Optimierung, kein Refactoring vor dem GATE.** Der Code darf hässlich sein.
   Er wird sowieso weggeworfen, wenn der Loop nicht trägt.
4. **Alles faken, was nicht getestet wird.** Kein Save-System, kein Menü, keine
   Inventar-UI. Drei Zahlen auf dem Bildschirm reichen. Tod = Szene neu laden.
5. **Meilensteine nicht überspringen.** Die Reihenfolge existiert, damit immer etwas
   Spielbares dasteht.

### Die Zwei-Spuren-Regel

**Code-Abende** brauchen Konzentration und einen Kopf, der Lust hat.
**Blender-Abende** brauchen das nicht.

Wenn die Motivation für Code fehlt: Props modellieren. Fass, Karren, Bett, Werkzeuge,
Fackel, Zaun. Das Projekt kommt voran, ohne dass du dich zwingst — und genau daran
sterben solche Projekte sonst.

**Einzige Bedingung:** Session 1–4 laufen auf der Code-Spur, ohne Blender-Umweg. Erst
wenn der Loop steht, ist die zweite Spur offen.
