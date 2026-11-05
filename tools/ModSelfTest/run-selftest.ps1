# ModSelfTest starten: Test-Mod installieren, PlateUp starten, auf Bericht warten, Bericht oeffnen.
#
#   .\run-selftest.ps1                         # alle Mods mit "TillAd.PlateUp." testen
#   .\run-selftest.ps1 -Prefix "TillAd.PlateUp.ChickenAndRice"
#   .\run-selftest.ps1 -NoQuit                 # Spiel nach dem Test offen lassen
#
# Getestet werden die Mods, die gerade installiert sind (Workshop-Abo oder PlateUp\Mods).

param(
    [string[]]$Prefix = @("TillAd.PlateUp."),
    [switch]$NoQuit,
    [int]$TimeoutSec = 300,
    [string]$GameDir = "C:\Program Files (x86)\Steam\steamapps\common\PlateUp\PlateUp"
)

$ErrorActionPreference = "Stop"
$dll      = Join-Path $PSScriptRoot "dist\ModSelfTest.dll"
$modDir   = Join-Path $GameDir "Mods\ModSelfTest\content"
$dataDir  = Join-Path $env:USERPROFILE "AppData\LocalLow\It's Happening\PlateUp\ModSelfTest"
$flag     = Join-Path $dataDir "run.flag"
$report   = Join-Path $dataDir "report"
$done     = Join-Path $report "done.txt"

if (-not (Test-Path $dll))     { throw "ModSelfTest.dll fehlt: $dll" }
if (-not (Test-Path $GameDir)) { throw "PlateUp nicht gefunden: $GameDir (mit -GameDir angeben)" }
if (Get-Process -Name PlateUp -ErrorAction SilentlyContinue) {
    throw "PlateUp laeuft schon. Bitte erst beenden."
}

# 1. Test-Mod installieren (macht ohne run.flag nichts, darf also liegen bleiben)
New-Item -ItemType Directory -Force -Path $modDir | Out-Null
Copy-Item $dll (Join-Path $modDir "ModSelfTest.dll") -Force

# 2. Auftrag hinterlegen
New-Item -ItemType Directory -Force -Path $dataDir | Out-Null
if (Test-Path $report) { Remove-Item $report -Recurse -Force }
$lines = @($Prefix)
if ($NoQuit) { $lines += "noquit" }
Set-Content -Path $flag -Value $lines -Encoding UTF8

# 3. Spiel ueber Steam starten
Write-Host "Starte PlateUp ..." -ForegroundColor Cyan
$startTime = Get-Date
Start-Process "steam://rungameid/1599600"

# 4. Warten; Lebenszeichen des Test-Mods aus dem Spiel-Log anzeigen
$log = Join-Path $env:USERPROFILE "AppData\LocalLow\It's Happening\PlateUp\Player.log"
$shown = 0
$injected = $false
$lastKey = 0
$wsh = New-Object -ComObject WScript.Shell
$sw = [Diagnostics.Stopwatch]::StartNew()
while (-not (Test-Path $done)) {
    # Titelbildschirm "Press any button": PlateUp laedt Mods erst nach einem Tastendruck.
    if (-not $injected -and (Test-Path $log) -and (Get-Item $log).LastWriteTime -gt $startTime) {
        $injected = [bool](Select-String -Path $log -SimpleMatch "Injecting" -Quiet -ErrorAction SilentlyContinue)
        $proc = Get-Process -Name PlateUp -ErrorAction SilentlyContinue | Select-Object -First 1
        if (-not $injected -and $proc -and $proc.MainWindowHandle -ne 0 -and $sw.Elapsed.TotalSeconds - $lastKey -gt 4 -and $sw.Elapsed.TotalSeconds -gt 15) {
            $lastKey = $sw.Elapsed.TotalSeconds
            if ($wsh.AppActivate($proc.Id)) { Start-Sleep -Milliseconds 300; $wsh.SendKeys("{ENTER}") }
        }
    }
    if ($sw.Elapsed.TotalSeconds -gt $TimeoutSec) {
        Remove-Item $flag -ErrorAction SilentlyContinue
        throw "Kein Ergebnis nach $TimeoutSec s. Player.log pruefen: $log"
    }
    Start-Sleep -Seconds 2
    if ((Test-Path $log) -and (Get-Item $log).LastWriteTime -gt $startTime) {
        $lines = @(Get-Content $log -ErrorAction SilentlyContinue | Select-String -SimpleMatch "[ModSelfTest]")
        if ($lines.Count -lt $shown) { $shown = 0 }
        for ($i = $shown; $i -lt $lines.Count; $i++) { Write-Host ("  " + $lines[$i].Line) }
        $shown = $lines.Count
    }
}
Write-Host ("Fertig nach {0:0} s" -f $sw.Elapsed.TotalSeconds) -ForegroundColor Green

# 5. Ergebnis
$crash = Join-Path $report "crash.txt"
if (Test-Path $crash) { Write-Host "Selbsttest abgestuerzt:" -ForegroundColor Red; Get-Content $crash | Select-Object -First 20 }
$summary = Join-Path $report "summary.txt"
if (Test-Path $summary) { Get-Content $summary }
$html = Join-Path $report "report.html"
if (Test-Path $html) { Invoke-Item $html }
