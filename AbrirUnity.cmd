@echo off
REM Abre Unity Hub (licencia) y luego el proyecto LA MYPIME GAME con la escena demo.
start "" "C:\Program Files\WindowsApps\UnityTechnologies.UnityHub_4.0.1.65535_x64__2vrhnee42bhxm\app\Unity Hub.exe"
timeout /t 12 /nobreak >nul
start "" "C:\Program Files\Unity\Hub\Editor\6000.6.9f1\Editor\Unity.exe" -projectPath "%~dp0" -licensingClientPath "C:\Program Files\WindowsApps\UnityTechnologies.UnityHub_4.0.1.65535_x64__2vrhnee42bhxm\app\UnityLicensingClient_V1\Unity.Licensing.Client.exe"
exit /b 0
