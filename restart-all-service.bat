@echo off
setlocal EnableDelayedExpansion
echo ==========================================
echo 🔄 Restarting all BLE Tracking services
echo ==========================================

rem Cari semua service dengan prefix BleTracking_
for /f "tokens=2 delims=:" %%S in ('sc query state^=all ^| findstr /R /C:"SERVICE_NAME: BleTracking_"') do (
    set "SERVICE=%%S"
    set "SERVICE=!SERVICE: =!"

    echo.
    echo ▶️  Restarting !SERVICE! ...

    rem Stop service
    net stop "!SERVICE!" >nul 2>&1

    rem Tunggu sampai benar-benar stopped
    :WAIT_STOP_!SERVICE!
    sc query "!SERVICE!" | findstr /I "STOPPED" >nul
    if errorlevel 1 (
        echo ⏳ Menunggu !SERVICE! stop...
        timeout /t 1 >nul
        goto WAIT_STOP_!SERVICE!
    )

    echo 🛠  Service stopped, starting again...

    rem Start kembali
    net start "!SERVICE!" >nul 2>&1

    if !errorlevel! == 0 (
        echo ✅ !SERVICE! berhasil direstart.
    ) else (
        echo ❌ Gagal start !SERVICE!.
    )
)

echo.
echo ==========================================
echo ✅ Semua service BleTracking_ selesai diproses.
echo ==========================================
pause
endlocal
