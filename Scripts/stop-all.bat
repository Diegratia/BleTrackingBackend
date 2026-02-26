@echo off
setlocal EnableDelayedExpansion

rem ============================================
rem BLE Tracking - Stop All Services
rem ============================================
rem Stops all BLE Tracking Windows Services
rem ============================================

set "BASE_PATH=%~dp0"

echo.
echo ==============================================
echo Stopping All BLE Tracking Services
echo ==============================================
echo.

set "COUNT=0"
set "FOUND=0"

rem Cari semua service dengan prefix BleTracking_
for /f "tokens=2 delims=:" %%a in ('sc query state^= all ^| findstr /i "SERVICE_NAME.*BleTracking_"') do (
    rem Trim spasi di depan
    for /f "tokens=* delims= " %%s in ("%%a") do (
        set "SERVICE_NAME=%%s"
    )
    set /a FOUND+=1

    echo [!FOUND!] Stopping: !SERVICE_NAME!

    rem Cek apakah service sedang running
    sc query "!SERVICE_NAME!" | find "RUNNING" >nul
    if not errorlevel 1 (
        net stop "!SERVICE_NAME!" >nul 2>&1
        if not errorlevel 1 (
            echo     OK - Stopped
            set /a COUNT+=1
        ) else (
            echo     FAILED - Could not stop
        )
    ) else (
        echo     SKIPPED - Not running
    )
    echo.
)

if !FOUND! == 0 (
    echo No BLE Tracking services found.
) else (
    echo ==============================================
    echo Stop Complete!
    echo Stopped: !COUNT! / !FOUND!
    echo ==============================================
)

echo.
pause
endlocal
