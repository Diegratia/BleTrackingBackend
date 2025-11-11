@echo off
setlocal EnableDelayedExpansion

rem ============================================
rem BLE Tracking - Windows Service Installer (v4, NSSM)
rem Struktur: publish\<ServiceName>.exe
rem Pastikan nssm.exe ada di folder root yang sama
rem ============================================

set "BASE_PATH=%~dp0"
set "PUBLISH_PATH=%BASE_PATH%publish"
set "NSSM_EXE=%BASE_PATH%nssm.exe"
set "LOG_DIR=%BASE_PATH%logs"

echo.
echo ==============================================
echo ⚙️  BLE Tracking - NSSM Installer
echo ==============================================
echo.

if not exist "%PUBLISH_PATH%" (
    echo [ERROR] Folder publish tidak ditemukan di %PUBLISH_PATH%.
    echo Jalankan: dotnet msbuild BuildAndPublishAll.proj -t:PublishAll
    pause
    exit /b
)

if not exist "%NSSM_EXE%" (
    echo [ERROR] nssm.exe tidak ditemukan di %BASE_PATH%.
    echo Unduh dari https://nssm.cc/download dan taruh di folder root project.
    pause
    exit /b
)

if not exist "%LOG_DIR%" mkdir "%LOG_DIR%"

for %%F in ("%PUBLISH_PATH%\*.exe") do (
    set "EXE_PATH=%%~fF"
    set "SERVICE_NAME=%%~nF"
    set "WIN_SERVICE_NAME=BleTracking_!SERVICE_NAME!"
    set "DISPLAY_NAME=BLE Tracking - !SERVICE_NAME!"
    set "LOG_OUT=%LOG_DIR%\!SERVICE_NAME!.log"
    set "LOG_ERR=%LOG_DIR%\!SERVICE_NAME!.err.log"

    echo Installing service: !WIN_SERVICE_NAME!

    rem cek apakah service sudah ada
    sc query "!WIN_SERVICE_NAME!" >nul 2>&1
    if not errorlevel 1 (
        echo Service !WIN_SERVICE_NAME! sudah ada, menghentikan dan menghapus...
        net stop "!WIN_SERVICE_NAME!" >nul 2>&1
        "%NSSM_EXE%" remove "!WIN_SERVICE_NAME!" confirm >nul 2>&1
        timeout /t 2 >nul
    )

    rem buat service baru via NSSM
    "%NSSM_EXE%" install "!WIN_SERVICE_NAME!" "!EXE_PATH!"
    "%NSSM_EXE%" set "!WIN_SERVICE_NAME!" DisplayName "!DISPLAY_NAME!"
    "%NSSM_EXE%" set "!WIN_SERVICE_NAME!" Start SERVICE_AUTO_START
    "%NSSM_EXE%" set "!WIN_SERVICE_NAME!" AppDirectory "%PUBLISH_PATH%"
    "%NSSM_EXE%" set "!WIN_SERVICE_NAME!" AppStdout "!LOG_OUT!"
    "%NSSM_EXE%" set "!WIN_SERVICE_NAME!" AppStderr "!LOG_ERR!"
    "%NSSM_EXE%" set "!WIN_SERVICE_NAME!" AppRotateFiles 1
    "%NSSM_EXE%" set "!WIN_SERVICE_NAME!" AppRotateOnline 1
    "%NSSM_EXE%" set "!WIN_SERVICE_NAME!" AppRotateBytes 1048576
    "%NSSM_EXE%" set "!WIN_SERVICE_NAME!" AppRestartDelay 3000
    "%NSSM_EXE%" set "!WIN_SERVICE_NAME!" Description "BLE Tracking microservice: !SERVICE_NAME!"

    rem start service
    "%NSSM_EXE%" start "!WIN_SERVICE_NAME!" >nul 2>&1
    echo Service !WIN_SERVICE_NAME! installed and started successfully.
    echo.
)

echo ==============================================
echo ✅ Semua service berhasil diproses.
echo Log output ada di: %LOG_DIR%
echo ==============================================
pause
endlocal
