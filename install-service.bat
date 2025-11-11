@echo off
setlocal EnableDelayedExpansion

rem ============================================
rem BLE Tracking - Windows Service Installer (v4)
rem Untuk install / reinstall 1 service tertentu
rem ============================================

set "BASE_PATH=%~dp0"
set "PUBLISH_PATH=%BASE_PATH%publish"

echo.
echo ==============================================
echo 🚀 BLE Tracking Service Installer (Single)
echo ==============================================
echo.

rem Pastikan parameter service diberikan
if "%~1"=="" (
    echo ❌ Error: Harap masukkan nama service (tanpa .exe)
    echo.
    echo Contoh:
    echo    install-service Visitor
    echo    install-service FloorplanDevice
    echo.
    pause
    exit /b 1
)

set "SERVICE_NAME=%~1"
set "EXE_PATH=%PUBLISH_PATH%\%SERVICE_NAME%.exe"
set "WIN_SERVICE_NAME=BleTracking_%SERVICE_NAME%"
set "DISPLAY_NAME=BLE Tracking - %SERVICE_NAME%"

if not exist "%EXE_PATH%" (
    echo ❌ File tidak ditemukan: %EXE_PATH%
    echo Pastikan sudah menjalankan publish.
    echo.
    pause
    exit /b 1
)

echo Installing service: %WIN_SERVICE_NAME%
echo ----------------------------------------------

rem cek apakah service sudah ada
sc query "%WIN_SERVICE_NAME%" >nul 2>&1
if not errorlevel 1 (
    echo 🔄 Service %WIN_SERVICE_NAME% sudah ada. Menghentikan dan menghapus...
    net stop "%WIN_SERVICE_NAME%" >nul 2>&1
    sc delete "%WIN_SERVICE_NAME%" >nul 2>&1
    timeout /t 2 >nul
)

rem buat ulang service
sc create "%WIN_SERVICE_NAME%" binPath= "\"%EXE_PATH%\"" DisplayName= "%DISPLAY_NAME%" start= auto

if !errorlevel! == 0 (
    echo ✅ Service %WIN_SERVICE_NAME% berhasil dibuat.
    echo ▶️  Memulai service...
    net start "%WIN_SERVICE_NAME%" >nul 2>&1
    echo 🚀 Service %WIN_SERVICE_NAME% berjalan.
) else (
    echo ❌ Gagal membuat service %WIN_SERVICE_NAME%.
)

echo.
echo ==============================================
echo ✅ Selesai.
echo ==============================================
pause
endlocal
