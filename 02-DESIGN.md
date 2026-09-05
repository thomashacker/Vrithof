# 02 — DESIGN

**Mittelalter-Zombie-Survival · Loop, Entscheidungen, Look, Ideen**
*Das ist das Nachschlagedokument. Was konkret zu tun ist, steht in `01-PLAN.md`.*

---

## Inhalt

1. [Die Vision](#1-die-vision)
2. [Der Gameplay-Loop](#2-der-gameplay-loop)
3. [Entscheidungen und warum](#3-entscheidungen-und-warum)
4. [Scope: was rein muss und was nicht](#4-scope)
5. [Wie ich teste, ob es funktioniert](#5-wie-ich-teste-ob-es-funktioniert)
6. [Der Look](#6-der-look)
7. [Asset-Pipeline](#7-asset-pipeline)
8. [Zombies: Technik](#8-zombies-technik)
9. [Ideen für später](#9-ideen-für-später)
10. [Offene Punkte](#10-offene-punkte)

---

## 1. Die Vision

Du bist ein einzelner Überlebender in einem mittelalterlichen Land voller Untoter.
Tagsüber plünderst du verlassene Gehöfte nach Eisen, nachts hält deine verbarrikadierte
Hütte — oder eben nicht.

**Die Referenzen:** Project Zomboid (jede Handlung wirkt sich aus, Bauen braucht echte
Materialien, jederzeit sterben können) · Valheim (Basis als Ausgangspunkt für Reisen,
Vorbereitung bestimmt Reichweite) · 7 Days to Die (Verteidigung gegen Horden).

**Was diese Spiele gemeinsam haben und was ich will:** Überleben in einer Welt, in der
Planung belohnt wird. Je besser vorbereitet, desto weiter komme ich. Je mehr Arbeit ich
in Basis und Wege stecke, desto mehr schaffe ich.

**Die Nische ist real.** Es gibt Grim Realms (Mittelalter-Kolonie gegen Untote, aber
reiner Management-Sim), Rebuild 3 (Wiederaufbau, aber modern und rundenbasiert), They
Are Billions, Going Medieval, Necesse. Was praktisch nicht existiert: Zomboid-artige
*Charakter*-Perspektive plus Mittelalter plus Handelsnetzwerk.

---

## 2. Der Gameplay-Loop

```
        ┌─────────────────────────────────────────┐
        │                                         │
        ▼                                         │
   TAG: Rausgehen                                 │
   Gehöfte plündern, Eisen suchen                 │
        │                                         │
        ▼                                         │
   ABEND: Rückweg                                 │
   Barrikaden setzen, Schäden reparieren          │
        │                                         │
        ▼                                         │
   NACHT: Es hält oder es hält nicht              │
        │                                         │
        ▼                                         │
   Reparatur kostet Eisen ──────────────────────► │
   Umgebung ist leergeplündert                    │
   → du musst weiter raus, riskanter              │
```

### Warum das trägt

Der Loop treibt sich selbst an. Jede überlebte Nacht macht die Umgebung ärmer und
zwingt dich weiter hinaus. Weiter hinaus heißt: länger unterwegs, weniger Zeitpuffer bis
zur Nacht, mehr Risiko, mehr Vorbereitung nötig.

Die Vorausschau, die das ganze Konzept trägt, entsteht dadurch **von allein** — ohne
eine einzige NPC-, Handels- oder Crafting-Mechanik. Das ist der Grund, warum genau
dieser Loop der POC ist und nicht irgendein anderer.

### Wo die Entscheidungen liegen

Der häufigste Denkfehler bei diesem Loop: zu glauben, die Nacht sei das Spiel. Ist sie
nicht. **Ohne echte Knappheit ist Barrikadieren nur Hinlaufen und E drücken.**

Es gibt drei Entscheidungsebenen, und nur zusammen ergeben sie Strategie:

| Ebene | Entscheidung | Woraus sie entsteht |
|---|---|---|
| **Tag** — die wichtigste | Wie weit gehe ich? Eisen für Barrikade oder Werkzeug? Schaffe ich noch ein Haus vor Einbruch der Dunkelheit? | Geplündertes statt geschenktes Eisen (M4) |
| **Abend** | Welche Öffnungen sichere ich, welche gebe ich auf? Äußerer Ring oder Rückzug in den inneren Raum? | Eisen reicht nie für alle Öffnungen; Öffnungen sind unterschiedlich |
| **Nacht** | Welche Barrikade repariere ich, welche opfere ich? Hämmere ich und ziehe damit mehr Zombies an? | Reparieren kostet Eisen und macht Lärm |

**Zwei Regeln, die daraus folgen:**

1. **Eine Ressource, die reicht, ist keine Ressource.** Das Eisen muss immer knapp bei
   etwa der Hälfte der Öffnungen liegen.
2. **Wenn alle Öffnungen gleich sind, ist die Wahl beliebig — und beliebig ist keine
   Strategie.** Tür hält länger, aber ihr Fall öffnet alles. Fenster sind schwach, aber
   einzeln. Ein offen gelassenes Fenster ist ein Fluchtweg.

### Lärm statt Allwissenheit

Zombies dürfen nicht einfach wissen, wo der Spieler ist. Sie laufen aufs **Gebäude** zu
und schlagen auf die nächste Öffnung — aber der lauteste Punkt zieht sie um. Hämmern und
Rennen sind laut, Schleichen ist leise.

Damit reparierst du nie umsonst: Du sicherst die Nordwand und ziehst dadurch die Horde
zur Nordwand. Das verwandelt die Hordennacht von einer Rundlaufstrecke in ein Abwägen —
für etwa zwanzig Zeilen Code.

---

## 3. Entscheidungen und warum

| Frage | Entscheidung | Begründung |
|---|---|---|
| Perspektive | First Person, 3D | Kein Character-Model, keine Animationen, keine Kamera-Probleme. Billigstes 3D. |
| Rolle | Ein Charakter, alles selbst | Keine NPC-KI im POC. |
| Engine | Unity, URP | Vorhandene Erfahrung schlägt jedes andere Argument. HDRP ist zu schwer. |
| Terrain | Unity Terrain (Heightmap) — **kein Voxel** | Voxel ist der Grund, warum 7DTD so aussieht. Siehe Abschnitt 6. |
| Welt | Prozedurales Terrain + handgebaute Prefabs | Die Arbeit steckt in den Prefabs; Terrain ist billig. |
| Bauen | Bestehende Gebäude verbarrikadieren | Interact-Raycast + Model-Swap. Kein Snapping, keine Statik. |
| Zerstörbarkeit | Nur Türen und Fenster, 3 Zustände | Model-Swap statt Physik. Keine zerstörbaren Wände. |
| Zeitstruktur | Fester Rhythmus, Horde in Nacht X | Deadline erzeugt Planung. Der Spieler kann sich vorbereiten. |
| Kampf | Möglich, aber teuer (Ausdauer, Verletzung) | Kampf als Notausgang, nicht als Strategie. |
| Knappe Ressource | **Eisen** | Siehe unten. |
| Assets | Props selbst in Blender, Zombies via Mixamo-Rig | Siehe Abschnitt 7. |
| Multiplayer | **Nicht im POC** | Netcode beantwortet nicht die Frage "macht es Spaß?". |

### Warum ausgerechnet Eisen

Holz ist im Mittelalter kein Engpass — Wald gibt es überall. Der Engpass ist Eisen: Erz
finden, Holzkohle brennen, Schmelzofen, Schmied. Nichts davon macht ein einzelner
Überlebender. **Eisen kann man nicht herstellen, nur plündern.** Es gated Barrikaden
*und* Werkzeuge, und das aus einem Grund, der historisch stimmt statt willkürlich
gesetzt zu sein.

**Eine einzige knappe Ressource im POC. Nicht zwei.** Du willst wissen, ob der Loop
trägt — nicht, ob deine Balance stimmt.

*(Salz kommt später und ist der zweite große Kandidat — siehe Abschnitt 9.)*

---

## 4. Scope

### Muss rein

- FP-Controller: Laufen, Sprinten, Ducken, Ausdauer
- Interact-Raycast (E auf Objekt)
- Inventar: flache Liste, feste Slotzahl, kein Gewicht
- Container: Truhen/Schränke in Prefabs mit Loot-Tabelle
- Ressource: Eisen (Zahl), Nahrung (Zahl, simpel)
- Barrikade: Fenster/Tür verstärken, kostet Eisen, hat HP
- Zombie: läuft zu Geräusch/Spieler, greift Barrikade oder Spieler an
- Tag/Nacht-Zyklus mit sichtbarer Uhr
- Horden-Nacht alle N Tage
- Nahkampf: Schwung, Ausdauerkosten, Verletzung bei Treffer
- Tod = Neustart (kein Save-System nötig)

### Kommt ausdrücklich NICHT rein

**Das ist die wichtigere Liste.** Hier steht fast alles, was an der Idee begeisternd
ist — und genau deshalb sterben solche Projekte: Man baut das Spannende zuerst und
kommt nie zum Loop. Alles hier ist Ausbaustufe 2 oder später.

- ❌ NPCs, Überlebende, Helfer
- ❌ Händler, Handelsnetzwerk, Dörfer
- ❌ Freies Bauen, Wände setzen, Statik
- ❌ Crafting-Bäume, Werkbänke, Rezepte
- ❌ Skills, Level, Fortschrittssystem
- ❌ Salz, Verderben, Konservierung
- ❌ Wetter, Jahreszeiten, Temperatur
- ❌ Multiplayer
- ❌ Eigene Zombie-Modelle und -Rigs
- ❌ Menüs, Optionen, Sound-Slider
- ❌ Story, Tutorial

---

## 5. Wie ich teste, ob es funktioniert

Nach M5: selbst spielen. Nicht "teste ich mal kurz" — richtig spielen, **drei Abende**.
Dann ehrlich beantworten:

1. Habe ich beim Plündern **auf die Uhr geschaut** und mich gefragt, ob ich es zurück schaffe?
2. Habe ich vor dem Rausgehen **überlegt**, was ich mitnehme und wohin ich gehe?
3. Hat es sich mies angefühlt, als eine Barrikade gefallen ist?
4. Habe ich nach einem Tod **sofort nochmal** angefangen?
5. Habe ich beim Spielen an Features gedacht, die ich einbauen will?

**Vier von fünf Ja → weiterbauen.** Ausbaustufe 2, in Ruhe, Feature für Feature.

**Weniger → nicht mehr Features draufwerfen.** Erst die Stellschrauben drehen:
Horden-Rhythmus, Eisen-Knappheit, Laufgeschwindigkeit, Tageslänge, Barrikaden-HP.

Wenn auch das nicht zündet, war der Loop falsch — und du hast es nach ein paar Abenden
gelernt statt nach einem halben Jahr. **Das ist ein Erfolg, kein Scheitern.**

---

## 6. Der Look

**Ziel: Valheim. Ausdrücklich nicht: 7 Days to Die.**

Der Unterschied ist technisch begründet. 7DTD nutzt Voxel-Terrain — Geometrie wird zur
Laufzeit generiert, alles ist zerstörbar. Der Preis dafür: kein gebackenes Licht, keine
sauberen UVs, keine handgebauten Silhouetten. Kantige Klötze mit gekachelten Texturen.
Die Zerstörbarkeit *ist* das Feature, und das Aussehen ist der Preis.

Valheim ist **kein Voxel**: Heightmap-Terrain mit Deformation (deswegen graben und
aufschütten, aber keine Höhlen), dazu modulare Bauteile mit Statik-System.

### Valheims Modelle sind erschreckend simpel

Wenige Polygone, flache Farben, kaum Texturdetail. Was den Look ausmacht, sind
Einstellungen, keine Kunstfertigkeit:

| Mittel | Umsetzung in Unity URP |
|---|---|
| **Dichter, gefärbter Nebel** ← wichtigster Einzelfaktor | Fog: Exponential Squared, Farbe je Tageszeit animiert |
| Harter Licht-Kontrast | Eine Directional Light, tief stehend, warm gegen kalte Schatten |
| Post-Processing | Volume: ACES-Tonemapping, Color Grading (Sättigung runter), dezentes Bloom, Vignette |
| Bewegung | Wind in Vegetation, Rauch aus Kaminen, Staub im Lichtstrahl, Funken am Feuer |

Nebel plus Post-Processing sind zwei Stunden Reglerdrehen und machen optisch mehr aus
als jede Woche Modellierarbeit. In First Person mit echter mittelalterlicher Dunkelheit
ist das noch dankbarer als bei Valheim — eine Fackel in schwarzer Nacht sieht mit fast
keinem Aufwand fantastisch aus.

> **Grenze:** Wenn du anfängst zu modellieren, um es schöner zu machen, hast du sie
> überschritten.

**Nach M3.5 gelernt:** Nebel, ACES und ein tiefer Sonnenstand sind eingebaut und
bringen etwas — aber nicht viel. Der Grund ist geometrisch: **Lichtstrahlen im Nebel
entstehen nur an Verdeckern.** Auf einer flachen Ebene mit vier Würfelhäusern gibt es
nichts, was Licht unterbricht. Bäume, Dachkanten und Fensterrahmen sind die
Voraussetzung, nicht die Kür — der Atmosphären-Abend gehört deshalb ein zweites Mal
wiederholt, sobald die Welt Geometrie hat. Volumetrisches Licht lohnt erst dann.

---

## 7. Asset-Pipeline

### Props — selbst in Blender

Kisten mit Extrudes:

Tür · Fenster · Barrikaden-Bretter · Truhe · Tisch, Bank, Bett · Fass, Sack, Karren ·
Wand-, Dach- und Bodenmodule fürs Gehöft · Fackel, Werkzeuge · Zaun

**Vier Konventionen, die später Ärger sparen:**

1. **1 Blender-Unit = 1 Meter**, vor dem Export immer *Apply Transforms*. Sonst kommt in
   Unity alles falsch skaliert oder verdreht an.
2. **Pivot bewusst setzen.** Tür: am Scharnier. Fenster: Mittelpunkt. Alles andere: am
   Boden.
3. **Ein einziges Farbpaletten-Atlas für alles.** Statt jedes Objekt zu texturieren:
   eine kleine Textur mit Farbfeldern, alle UVs zeigen nur auf ein Farbfeld. Der
   Synty-Trick. Macht handgebaute Assets automatisch stilistisch einheitlich, kostet
   null Texturarbeit und sieht *bewusst* aus statt unfertig.
4. **Modular denken.** Ein Wandstück, das sich wiederholt, ist mehr wert als ein
   fertiges Haus.

### Zerstörbarkeit: nur Türen und Fenster

Keine echte Zerstörungsphysik. Jede Öffnung ist ein **Model-Swap zwischen drei
Zuständen**:

```
   INTAKT  ──[verbarrikadieren, kostet Eisen]──►  VERBARRIKADIERT
      │                                                  │
      │◄──────────[reparieren, kostet Eisen]─────────────┤
      ▼                                                  ▼
   ZERSTÖRT  ◄────────[HP auf 0]──────────────────────────
```

Die Barrikade selbst sind einzelne Bretter-Prefabs, die nacheinander erscheinen — so
sieht man auf einen Blick, wie stark eine Öffnung gesichert ist und wie viel schon
weggeschlagen wurde. **Lesbarkeit ohne UI.** Beim Zerbrechen zwei, drei
Splitter-Partikel und ein Sound; niemand merkt, dass da keine Physik läuft.

---

## 8. Zombies: Technik

### Hybrid statt Selbstbau

**Nicht selbst riggen, jedenfalls nicht am Anfang.** Modellierung für Deformation,
UV-Unwrap, Skelett und Weight Painting sind ein eigenes Fachgebiet — und schlechtes
Weight Painting sieht nicht gruselig aus, sondern kaputt.

Stattdessen **Mixamo-Rig plus Basis-Animationen** (kostenlos, sauber, sofort
einsatzbereit). Ein eigenes Modell kann später drübergezogen werden — das Rig bleibt.

### Prozeduraler Layer: Unity Animation Rigging Package

- **MultiAimConstraint** auf Kopf und Brustwirbel → der Zombie fixiert dich permanent
  mit dem Blick, egal wohin seine Beine laufen. Ein Constraint, sofort unangenehm.
- **TwoBoneIKConstraint** auf beide Arme mit dem Spieler als Ziel → die Arme strecken
  sich nach dir, unabhängig von der Lauf-Animation. Das "Greifen" entsteht von selbst.

### Die Grab-Mechanik ist kein Animations-, sondern ein Zustandsproblem

Zombie wechselt in `Grabbing` → deine Bewegung wird gesperrt oder stark verlangsamt →
Kamera zieht leicht zu ihm → Hand-IK-Ziele rasten auf deine Schultern → Schaden tickt →
du befreist dich durch Tastendrücken.

Die IK verkauft es optisch, dahinter stecken ~50 Zeilen State Machine. **Das billigste
echte Grauen im ganzen Projekt.**

### Gratis-Gruseligkeit

- Pro Zombie zufällige Animationsgeschwindigkeit (0.8–1.2) und zufälliger Startzeitpunkt
  → die Horde läuft nicht im Gleichschritt. **Der Gleichschritt ist es, was billig
  aussieht.**
- Leichte dauerhafte Schräglage im Oberkörper
- Audio und Dunkelheit. Was du nur halb siehst, ist gruseliger als alles, was du gut
  modellieren könntest.

### Nicht machen

Active Ragdoll / vollständig physikgetriebene Zombies. Sieht in Videos grandios aus, ist
ein monatelanges Tuning-Loch.

---

## 9. Ideen für später

*Alles hier erst nach dem GATE. Aufschreiben ist erlaubt, bauen nicht.*

### Ausbaustufe 2 — naheliegend

- **Prozedurale Prefab-Verteilung** über größeres Terrain (7DTD-Ansatz: prozedurales
  Terrain, handgebaute Gebäude darauf gestreut)
- **Salz und Konservierung.** Salz war eine der wertvollsten Handelswaren des
  Mittelalters — ganze Städte und Handelsrouten existierten nur deswegen, weil ohne
  Salz Nahrung nicht haltbar ist. Im Spiel heißt das: **Salz bestimmt direkt, wie weit
  du von der Basis wegkommst.** Frisches Essen verdirbt in Tagen, gepökeltes hält Wochen.
  Das ist "je besser vorbereitet, desto weiter komme ich" als konkrete Mechanik.
- **Karte als findbares Item**, unvollständig. Gegenmittel gegen den
  Übersichtsverlust in First Person.
- **Werkzeug-Haltbarkeit.** Klingen stumpfen ab, Wetzstein, Eisen als Reparaturkosten.
- **Eigener Player-Controller mit Gewicht.** Der Starter-Asset-Controller wird ersetzt.
  Ziel ist Trägheit statt Reaktionsfreude: Beschleunigung und Abbremsen brauchen Zeit,
  Rüstung macht schwerer — und **lauter**, was direkt ans Lärm-System andockt. Damit
  wird Ausrüstung eine Abwägung statt eine Verbesserung. Bis dahin bleibt der
  Starter-Asset-Controller stehen, samt seiner Wandreibung: ihn zu reparieren wäre
  Arbeit an etwas, das sowieso wegfliegt.
- **Licht als Ressource.** Talg und Wachs waren teuer, Nacht im Mittelalter ist
  vollständige Dunkelheit. In FP enorm stark.

### Ausbaustufe 3 — die eigentliche Träumerei

- **Überlebende und NPCs**, die beim Sammeln und Bauen helfen
- **Dörfer wieder aufbauen oder zerstören**
- **Händler und ein echtes Handels- und Rohstoffnetzwerk** — je nach Standort
  exportieren Orte unterschiedliche Waren, und die Motivation besteht darin, das Netz
  auszubauen und zu schützen
- **Mehrere Siedlungen gleichzeitig verteidigen.** Drei gut aufgebaute Dörfer mit
  Wasser- und Nahrungsversorgung, gut gemauert — und dann kommt eine Horde, und eines
  geht verloren. Stakes bleiben permanent hoch.
- **Multiplayer**, koop

---

## 10. Offene Punkte

- **Übersicht in First Person.** Das Konzept lebt von Planung, FP nimmt die räumliche
  Übersicht. Zomboid ist isometrisch, *weil* Vorausschau Übersicht braucht. Gegenmittel
  ab Ausbaustufe 2: Karte als Item, Aussichtsturm, Markierungen setzen. Im POC kein
  Thema, weil die Welt klein ist.
- **Warum verbarrikadieren, wenn Weglaufen reicht?** Im M3-Test gab es keinen
  Anreiz, die Hütte zu halten. Der Spieler ist schneller als die Zombies,
  Wegrennen kostet nichts, und eine Barrikade bringt vor allem das Risiko, sie
  nicht rechtzeitig zu reparieren. **Die Nacht braucht einen Grund, an einem Ort
  zu bleiben.** Kandidaten: Schlaf (ohne Rast ist der nächste Tag schwächer),
  ein Lager (Vorräte liegen in der Basis und gehen verloren), Erschöpfung über
  Nacht. Ungelöst.

  **Nach M4 präzisiert:** Der Loop trägt auch ohne die Barrikaden — Plündern,
  Nahrung und wandernde Zombies erzeugen die Entscheidungen von allein. Damit
  ist die eigentliche Frage nicht mehr "wie mache ich Barrikaden attraktiv",
  sondern: **Eisen hat nur eine Senke, und die macht keinen Spaß.** Statt die
  Senke zu retten, liegt näher, eine zweite danebenzustellen. Daraus ist M5a
  geworden — Gehöfte aufbrechen und die Axt, siehe `01-PLAN.md`.
- **Horden-Rhythmus.** Alle 3 Tage ist ein Startwert, kein Ergebnis. Wird beim Testen
  gedreht.
- **Gehöft-Layout.** Wie viele Öffnungen kann ein Spieler realistisch verteidigen?
  Reicht ein Raum oder braucht es einen Rückzugsraum? Beantwortet das Blockout in
  Session 1, bevor in Blender modelliert wird.
