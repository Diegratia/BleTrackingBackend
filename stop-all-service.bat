@echo off
setlocal EnableDelayedExpansion
echo ==========================================
echo 🛑 Stopping all BLE Tracking services
echo ==========================================

rem Loop semua service yang diawali BleTracking_
for /f "tokens=1,2 delims=:" %%A in ('sc query state^= all ^| findstr /R /C:"SERVICE_NAME: BleTracking_"') do (
    for /f "tokens=2" %%S in ("%%A %%B") do (
        set "SERVICE=%%S"
        set "SERVICE=!SERVICE: =!"
        echo Stopping !SERVICE!...
        net stop "!SERVICE!" >nul 2>&1
        timeout /t 2 >nul
        taskkill /f /im "!SERVICE!.exe" >nul 2>&1
    )
)

echo ==========================================
echo ✅ All BLE Tracking services processed.
echo ==========================================
pause
endlocal
