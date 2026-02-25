@echo off
setlocal EnableDelayedExpansion

rem ============================================
rem BLE Tracking - Install All Services
rem ============================================
rem Installs all services from publish/ folder
rem ============================================

set "BASE_PATH=%~dp0"
set "PUBLISH_PATH=%BASE_PATH%publish"

echo.
echo ==============================================
echo Installing All BLE Tracking Services
echo ==============================================
echo.

if not exist "%PUBLISH_PATH%" (
    echo [ERROR] Folder publish tidak ditemukan: %PUBLISH_PATH%
    echo Jalankan publish-all.bat terlebih dahulu.
    echo.
    pause
    exit /b
)

echo Scanning for services in %PUBLISH_PATH%...
echo.

set "COUNT=0"
for %%F in ("%PUBLISH_PATH%\*.exe") do (
    set "EXE_PATH=%%~fF"
    set "SERVICE_NAME=%%~nF"
    set "WIN_SERVICE_NAME=BleTracking_!SERVICE_NAME!"
    set "DISPLAY_NAME=BLE Tracking - !SERVICE_NAME!"

    set /a COUNT+=1
    echo [!COUNT!] Installing: !SERVICE_NAME!

    rem cek apakah service sudah ada
    sc query "!WIN_SERVICE_NAME!" >nul 2>&1
    if not errorlevel 1 (
        echo     Service exists, removing...
        net stop "!WIN_SERVICE_NAME!" >nul 2>&1
        sc delete "!WIN_SERVICE_NAME!" >nul 2>&1
        timeout /t 1 >nul
    )

    rem buat service
    sc create "!WIN_SERVICE_NAME!" binPath= "\"!EXE_PATH!\"" DisplayName= "!DISPLAY_NAME!" start= auto >nul 2>&1
    if !errorlevel! == 0 (
        echo     Created, starting...
        net start "!WIN_SERVICE_NAME!" >nul 2>&1
        echo     OK - !WIN_SERVICE_NAME!
    ) else (
        echo     FAILED - !WIN_SERVICE_NAME!
    )
    echo.
)

echo ==============================================
echo Install Complete!
echo Total services: !COUNT!
echo ==============================================
echo.
pause
endlocal
