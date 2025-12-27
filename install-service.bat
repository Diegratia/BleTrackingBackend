@echo off
setlocal EnableDelayedExpansion

rem =====================================================
rem BLE Tracking Service Installer - Single (Auto Name)
rem =====================================================

rem UBAH INI SESUAI SERVICE MU (tanpa .exe)
set "SERVICE_NAME=CardAccess"

set "BASE_PATH=%~dp0"
set "PUBLISH_PATH=%BASE_PATH%publish"
set "EXE_PATH=%PUBLISH_PATH%\%SERVICE_NAME%.exe"
set "WIN_SERVICE_NAME=BleTracking_%SERVICE_NAME%"
set "DISPLAY_NAME=BLE Tracking - %SERVICE_NAME%"

echo.
echo ==============================================
echo 🚀 BLE Tracking Service Installer (%SERVICE_NAME%)
echo ==============================================
echo.

rem ----------------------------------
rem VALIDASI FILE EXE
rem ----------------------------------
if not exist "%EXE_PATH%" (
    echo ❌ File tidak ditemukan: %EXE_PATH%
    echo Pastikan sudah menjalankan publish.
    goto END
)

echo Installing service: %WIN_SERVICE_NAME%
echo ----------------------------------------------

rem ----------------------------------
rem CEK JIKA SERVICE SUDAH ADA
rem ----------------------------------
sc query "%WIN_SERVICE_NAME%" >nul 2>&1
if !errorlevel! == 0 (
    echo 🔄 Service ditemukan. Menghapus...
    net stop "%WIN_SERVICE_NAME%" >nul 2>&1
    sc delete "%WIN_SERVICE_NAME%" >nul 2>&1
    timeout /t 2 >nul
)

rem ----------------------------------
rem CREATE SERVICE
rem ----------------------------------
sc create "%WIN_SERVICE_NAME%" binPath= "\"%EXE_PATH%\"" DisplayName= "%DISPLAY_NAME%" start= auto || goto END

echo ✅ Service berhasil dibuat.
echo ▶️  Memulai service...

net start "%WIN_SERVICE_NAME%" || goto END

echo 🚀 Service berjalan dengan normal.

goto END

:END
echo.
echo ==============================================
echo ⏹  Script selesai (window tidak akan menutup)
echo ==============================================
pause >nul

endlocal
