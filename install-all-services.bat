@echo off
setlocal EnableDelayedExpansion

rem ============================================
rem BLE Tracking - Windows Service Installer (v2)
rem Struktur: Services.API\<Service>\publish\<Service>\*.exe
rem ============================================
set "BASE_PATH=%~dp0Services.API"

echo.
echo ==============================================
echo Scanning for services in %BASE_PATH%
echo ==============================================
echo.

for /d %%S in ("%BASE_PATH%\*") do (
    set "SERVICE_DIR=%%S"
    set "SERVICE_NAME=%%~nS"
    set "PUBLISH_DIR=!SERVICE_DIR!\publish\!SERVICE_NAME!"
    set "EXE_PATH=!PUBLISH_DIR!\!SERVICE_NAME!.exe"
    set "WIN_SERVICE_NAME=BleTracking_!SERVICE_NAME!"
    set "DISPLAY_NAME=BLE Tracking - !SERVICE_NAME!"

    if exist "!EXE_PATH!" (
        echo Installing service: !WIN_SERVICE_NAME!

        rem check if service exists
        sc query "!WIN_SERVICE_NAME!" >nul 2>&1
        if not errorlevel 1 (
            echo Service !WIN_SERVICE_NAME! already exists. Stopping and deleting...
            net stop "!WIN_SERVICE_NAME!" >nul 2>&1
            sc delete "!WIN_SERVICE_NAME!" >nul 2>&1
            timeout /t 2 >nul
        )

        rem create new Windows service
        sc create "!WIN_SERVICE_NAME!" binPath= "\"!EXE_PATH!\"" DisplayName= "!DISPLAY_NAME!" start= auto
        if !errorlevel! == 0 (
            echo Service !WIN_SERVICE_NAME! created successfully.
            net start "!WIN_SERVICE_NAME!" >nul 2>&1
            echo Service !WIN_SERVICE_NAME! started.
        ) else (
            echo Failed to create service !WIN_SERVICE_NAME!.
        )
        echo.
    ) else (
        echo [WARN] No publish exe found for !SERVICE_NAME!. Expected at !EXE_PATH!.
    )
)

echo ==============================================
echo All services processed.
echo ==============================================
pause
endlocal
