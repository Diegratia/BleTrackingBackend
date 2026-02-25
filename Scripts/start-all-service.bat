@echo off
for /f "tokens=2 delims=:" %%a in ('sc query ^| find "BleTracking_"') do (
    set "svc=%%a"
    echo Starting !svc!...
    net start "!svc!" >nul 2>&1
)
echo ✅ All BLE Tracking services started.
pause
