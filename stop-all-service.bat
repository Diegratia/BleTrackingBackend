@echo off
setlocal EnableDelayedExpansion

echo Menghentikan semua service dengan prefix BLETracking_ ...
echo.

for /f "tokens=2 delims=:" %%S in ('sc query state^=all ^| find "SERVICE_NAME: BleTracking_"') do (
    set SERVICE=%%S
    set SERVICE=!SERVICE: =!
    echo Menghentikan !SERVICE! ...
    sc stop "!SERVICE!" >nul 2>&1
    timeout /t 1 >nul
)

echo.
echo Semua service BLETracking_ telah diproses untuk dihentikan.
pause
