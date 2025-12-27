@echo off
setlocal EnableDelayedExpansion

rem ============================================
rem BLE Tracking - Windows Service Installer (v3)
rem Struktur baru: publish\<ServiceName>.exe
rem ============================================

set "BASE_PATH=%~dp0"
set "PUBLISH_PATH=%BASE_PATH%publish"

echo.
echo ==============================================
echo 🚀 Scanning for services in %PUBLISH_PATH%
echo ==============================================
echo.

if not exist "%PUBLISH_PATH%" (
    echo [ERROR] Folder publish tidak ditemukan di %PUBLISH_PATH%.
    echo Jalankan: dotnet msbuild build-all.targets -t:PublishAll
    echo.
    pause
    exit /b
)

for %%F in ("%PUBLISH_PATH%\*.exe") do (
    set "EXE_PATH=%%~fF"
    set "SERVICE_NAME=%%~nF"
    set "WIN_SERVICE_NAME=BleTracking_!SERVICE_NAME!"
    set "DISPLAY_NAME=BLE Tracking - !SERVICE_NAME!"

    echo Installing service: !WIN_SERVICE_NAME!

    rem cek apakah service sudah ada
    sc query "!WIN_SERVICE_NAME!" >nul 2>&1
    if not errorlevel 1 (
        echo Service !WIN_SERVICE_NAME! already exists. Stopping and deleting...
        net stop "!WIN_SERVICE_NAME!" >nul 2>&1
        sc delete "!WIN_SERVICE_NAME!" >nul 2>&1
        timeout /t 2 >nul
    )

    rem buat ulang service
    sc create "!WIN_SERVICE_NAME!" binPath= "\"!EXE_PATH!\"" DisplayName= "!DISPLAY_NAME!" start= auto
    if !errorlevel! == 0 (
        echo Service !WIN_SERVICE_NAME! created successfully.
        net start "!WIN_SERVICE_NAME!" >nul 2>&1
        echo Service !WIN_SERVICE_NAME! started.
    ) else (
        echo ❌ Failed to create service !WIN_SERVICE_NAME!.
    )
    echo.
)

echo ==============================================
echo ✅ All services processed.
echo ==============================================
pause
endlocal
