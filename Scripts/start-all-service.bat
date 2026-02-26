@echo off
setlocal EnableDelayedExpansion

rem ============================================
rem BLE Tracking - Start All Services
rem ============================================
rem Starts all BLE Tracking Windows Services
rem ============================================

echo.
echo ==============================================
echo Starting All BLE Tracking Services
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

    echo [!FOUND!] Starting: !SERVICE_NAME!

    rem Cek apakah service sudah running
    sc query "!SERVICE_NAME!" | find "RUNNING" >nul
    if not errorlevel 1 (
        echo     SKIPPED - Already running
    ) else (
        net start "!SERVICE_NAME!" >nul 2>&1
        if not errorlevel 1 (
            echo     OK - Started
            set /a COUNT+=1
        ) else (
            echo     FAILED - Could not start
        )
    )
    echo.
)

if !FOUND! == 0 (
    echo No BLE Tracking services found.
) else (
    echo ==============================================
    echo Start Complete!
    echo Started: !COUNT! / !FOUND!
    echo ==============================================
)

echo.
pause
endlocal
