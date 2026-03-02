@echo off
setlocal EnableDelayedExpansion

rem ============================================
rem BLE Tracking - Uninstall All Services
rem ============================================
rem Removes all BLE Tracking Windows Services
rem ============================================

echo.
echo ==============================================
echo Uninstalling All BLE Tracking Services
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
    echo [!FOUND!] Removing: !SERVICE_NAME!

    rem Stop service dulu
    sc query "!SERVICE_NAME!" | find "RUNNING" >nul 2>&1
    if not errorlevel 1 (
        echo     Stopping...
        net stop "!SERVICE_NAME!" >nul 2>&1
        rem Tunggu lebih lama untuk service stop
        timeout /t 2 /nobreak >nul
    )

    rem Delete service
    sc delete "!SERVICE_NAME!" >nul 2>&1
    if not errorlevel 1 (
        echo     OK - Removed
        set /a COUNT+=1
    ) else (
        echo     FAILED - Run as Administrator?
    )
    echo.
)

if !FOUND! == 0 (
    echo No BLE Tracking services found.
) else (
    echo ==============================================
    echo Uninstall Complete!
    echo Removed: !COUNT! / !FOUND!
    echo ==============================================
)

echo.
pause
endlocal
