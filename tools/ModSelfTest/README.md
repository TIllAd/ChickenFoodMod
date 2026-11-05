# ModSelfTest

Kleiner Test-Mod für PlateUp-Mods (Chicken & Rice, Breaking Plates, …).
Er startet das Spiel, prüft die Spieldaten der eigenen Mods und rendert alles,
was sie hinzufügen. Danach beendet er das Spiel und öffnet einen Bericht.

## Starten

Doppelklick auf `run-selftest.bat` (oder `.\run-selftest.ps1` in PowerShell).

Optionen:

- `-Prefix "TillAd.PlateUp.ChickenAndRice"`: nur einen Mod testen (Standard: alle `TillAd.PlateUp.*`)
- `-NoQuit`: Spiel nach dem Test offen lassen

Getestet werden die Mods, die gerade installiert sind (Workshop-Abo oder lokaler `PlateUp\Mods`-Ordner).
Nicht beide gleichzeitig, sonst lädt der Mod doppelt.

## Was geprüft wird

- **Kombinationen:** Jede Teilmenge der Zutaten jedes eigenen Gerichts/Topfs wird dem Spiel vorgelegt
  (dieselbe Funktion, die das Spiel beim Kombinieren nutzt). FAIL, wenn daraus ein fremdes Gericht wird
  (z. B. der Stir-Fry-Bug) oder eine fertige Kombination nicht als fertig erkannt wird.
- **Bestellungen:** Alle Bestellungen, die Kunden mit beliebigen Kombinationen eigener Karten aufgeben
  können, plus Prüfung, dass jede davon baubar ist. Bilder zeigen die Bestellung so, wie das Spiel sie rendert.
- **Items:** Prefab vorhanden, Prozesse haben Ergebnisse, Portionen aus Töpfen sind eigene Items
  (WARN, wenn Spiel-Items rauskommen, die in Spiel-Gerichten landen können).
- **Geräte, Karten, Prozesse:** Liste mit Bildern bzw. Texten.

## Ergebnis

`%USERPROFILE%\AppData\LocalLow\It's Happening\PlateUp\ModSelfTest\report\`

- `report.html`: Bericht mit Bildern (öffnet sich automatisch)
- `summary.txt`: nur FAIL/WARN als Text
- `crash.txt`: falls der Test selbst abgestürzt ist

## Bauen

Quellcode: `SelfTest.cs`. Die fertige DLL liegt in `dist/ModSelfTest.dll`.
Ohne `run.flag` (legt das Skript an) macht der Mod nichts, er darf also installiert bleiben.
