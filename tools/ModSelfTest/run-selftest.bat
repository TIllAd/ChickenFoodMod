@echo off
rem Doppelklick-Start fuer run-selftest.ps1 (Parameter werden durchgereicht).
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-selftest.ps1" %*
pause
