# Chicken & Rice – Ideen

Sammlung für spätere Updates. Stand: November 2026.
Status: 💡 Idee · 🤔 unsicher · 🛠️ in Arbeit · ✅ drin · ❌ verworfen

---

## Fitness-Thema (passt zu Bulking Season / Low Carb)

### 💡 Protein Shake
- Milch + Proteinpulver in einen Shaker, an einer Station **shaken/mixen** (Interaktion wie Schneiden, eigenes Symbol).
- Servieren als **Beilage**, **Starter** oder sogar eigener **Main** – noch offen, was am besten passt.
- Braucht: Shaker-Becher-Modell, Proteinpulver-Quelle, evtl. Geschmacksrichtungen (Schoko/Vanille) als Varianten.

### 💡 Cheat Day (Dessert)
- Klassische „Fitness-Nachspeise“ als Dessert-Karte, z. B. Protein-Pancakes, Skyr mit Beeren, Protein-Brownie.
- Alternativ als Kundenkarte: ab und zu bestellt jemand zusätzlich ein Dessert.

### 💡 Gym-Deko
- Hantelablage, Protein-Dose, Spiegel.
- Motivationsposter (Sprüche wie „No Pain No Grain“).
- Bild einer „Fitness-Ikone“ – **Achtung Urheber-/Persönlichkeitsrecht**: keine echte Person abbilden, lieber eine erfundene Figur / Silhouette.

### 💡 Restaurantnamen (StartingNameSet)
Ideen zum Ergänzen der aktuellen Liste („Rice Rice Baby“, „Wing It“, …):
- Swole Food · Gains Kitchen · Rice & Shine · The Protein Pit · Macro Kitchen · Chicken Pump · Bro Bowl · Lift & Eat

### 💡 Weitere Kundenkarten
- **Gym Rats:** essen sehr schnell, sind aber ungeduldig (nur Werte, wenig Aufwand).
- **Cutting Season:** 2× Chicken **ohne** Reis (Gegenstück zu Bulking; Bulking bestellt bewusst immer Reis).
- **Leg Day:** Kunden laufen langsamer – witzig, spielerisch fragwürdig. 🤔

### 🤔 Kleidung / Kosmetik – verschoben (Branch `feature/cosmetics`)
Erster Versuch Nov 2026, für 1.1.0 rausgenommen. Stand auf dem Branch:
- Funktioniert/ok: **Chicken Chef Hat** (hohe Kochmütze in reinem Weiß + Hühnchen-Anstecker), **Sweatband** (offener Ring), **Chicken Shack Apron** (weißes Hemd, rote Schürze), **Chicken Costume** (Astronauten-Anzug weiß, gelbe Stiefel – zum Gockel-Hut aus dem Spiel kombinieren).
- Hat nicht geklappt: **Gym Outfit / Tanktop**, **Retro Tracksuit** (Ladefehler bzw. sah nicht richtig aus).
- Gestrichen: eigene Hühnerkopf-Mütze – der **Gockel-Hut aus dem Spiel** ist besser.

Gelernt:
- Spiel-Modelle sind **gesperrt** (nicht lesbar) → Ärmel o. Ä. lassen sich zur Laufzeit nicht wegschneiden.
- Umfärben geht gut (Material durch „Simple Flat“ ersetzen); Teile verschiedener Outfits lassen sich kombinieren (gleiches Skelett).
- Brauchbare Vorlagen: **Pilz-Kostüm** = ärmelloses Oberteil + kurze Hose (→ Tanktop/Gym), **Skelett-Kostüm** = Ganzkörperanzug (Muster ist nur Textur → einfarbiger Einteiler), **Kellner** = Weste.
- Achtung: Unterteile hängen am Elternteil (Pilzhut → Stern) – beim Löschen mitgelöscht.
- Echte neue Formen (Tanktop, Tracksuit mit Streifen) → Blender-Weg: AssetRipper (FBX mit Skelett) → Blender → Unity-Projekt → `mod.assets`, Mesh im Code gegen Spiel-Mesh tauschen (Knochen per Namen umrechnen).
- Selbsttest rendert alle Spiel-Outfits als Vorlage (Abschnitt „Spiel (Vorlagen)“).

---

## Reis & Beilagen

### 💡 Egg Fried Rice
- Reis aufwerten statt ersetzen: Reis + Ei (+ Karotten / Mais / Erbsen) in Pfanne/Wok → Fried Rice.
- Als Variante/Upgrade des Reis-Platzes, nicht als eigenes Gericht.
- ⚠️ Wok-Zutaten des Spiels nicht wiederverwenden (siehe Stir-Fry-Bug in v1.0.1) – eigene Items, Selbsttest laufen lassen.

### 🤔 Andere Beilage statt Reis
- Grundsätzlich schräg, weil der Mod „Chicken & Rice“ heißt. Ausnahme: Low Carb (Brokkoli statt Reis) ✅.
- Süßkartoffel o. Ä. eher nicht, höchstens als eigene Karte mit gutem Grund.

### 💡 Reiskocher (Gerät)
- Kaufbares Gerät für Reis (und evtl. andere Reisgerichte).
- Idee: hält Reis **warm, aber nicht zu warm** – kann nicht anbrennen (Burnt Rice) und nicht abkühlen (Cold Rice).
- Offene Frage: Was ist der Vorteil gegenüber dem Safety Hob (brennt auch nicht an)? Mögliche Antworten:
  - kocht automatisch / schneller,
  - hält zusätzlich warm (nur relevant, wenn Cold Rice zurückkommt),
  - mehr Portionen pro Durchgang.

### 🤔 Cold Rice (Karte) – verschoben aus 1.1.0
- Reis kühlt abseits vom Herd ab, muss aufgewärmt werden.
- Problem in 1.1.0: automatischer Prozess ohne Gerät zeigt **keinen Fortschrittsbalken**, Portionier-Sperre wirkte verbuggt.
- Idee: Abkühl-Animation/-Anzeige aus dem Spiel nutzen (es gibt eine „Cold/Abkühl“-Animation, z. B. bei gefrierenden/abkühlenden Items) – prüfen, wo das Spiel die anzeigt.
- Zusammen mit dem Reiskocher denken (Reiskocher = Gegenmittel).
- Code der ersten Version liegt im Git-Verlauf vor Commit `50f8cbe`.

---

## Vorspeisen & Gerichte

### 💡 Hühnersuppe (Starter)
- Brühe im Topf kochen, Chicken (Shredded?) rein, portionieren.
- Gibt es als Starter wohl noch nicht – passt gut zum Thema.

### 💡 Asiatische Ergänzungen
- Teriyaki-Glasur (Sojasoße + Zucker einkochen), Sesam / Frühlingszwiebeln als Toppings, Onigiri als Starter, Curry als großer Main.

### 💡 Tofu / Veggie
- Tofu statt Chicken (Sojabohnen → kochen → pressen → schneiden → braten), Veggie-Kunden lehnen Chicken ab.
- Tofu gibt es schon in anderen Mods → als Referenz anschauen.

---

## Eigener Mod / größer

### 💡 Takeaway / To-Go
- Kein bestehender PlateUp-Takeaway-Mod gefunden (Stand Nov 2026).
- Idee: To-Go-Menü bzw. ganz eigener Mod – Bestellung an einer Theke/Ausgabe, Essen in Box/Tüte statt Teller, Kunde nimmt mit.
- Groß genug für einen eigenen Mod statt Karte in Chicken & Rice.

---

## Verworfen
- ❌ Meal Prep Sunday – es gibt keine Wochentage im Spiel.
- ❌ Milchreis – gibt es schon als Mod-Karte.
- ❌ Air Fryer – genug Anbratmöglichkeiten im Spiel.
- ❌ Gekochtes Ei als Topping – steht allein im Raum; Ei eher im Fried Rice.
- ❌ Salat statt Reis (Low Carb) – kollidiert mit dem Spiel-Salat; jetzt Brokkoli.

---

## Erledigt
- ✅ v1.0.1: eigene Reis-/Brokkoli-Portionen (Stir-Fry-Bug).
- ✅ (1.1.0, Branch `feature/new-cards`): Low Carb, Bulking Season, Burnt Rice, keine Kundenreduktion beim Grundgericht.

---

## Hinweise für später
- **Selbsttest:** `tools/ModSelfTest/run-selftest.bat` – prüft Kombinationen/Bestellungen und rendert alles. Vor jedem Release laufen lassen.
- **Gleiche Zutaten wie Spiel-Gerichte vermeiden** (Teller + Spiel-Item kann zu einem Spiel-Gericht werden). Lieber eigene Items mit gleicher Optik.
- **Pflicht-Plätze schalten ihre Zutaten automatisch frei** → für optionale Extra-Portionen (Bulking) eine eigene Freischalt-Markierung nutzen.
- **Workshop:** Seit dem Malware-Vorfall (Okt 2026) lädt PlateUp keine neu hochgeladenen Workshop-Mods (ID > 3820868888). Bis das aufgehoben ist, nur manuelle Installation.
