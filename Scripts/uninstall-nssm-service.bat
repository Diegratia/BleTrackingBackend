@echo off
setlocal
set "PREFIX=BleTracking_"
for /f "tokens=1" %%s in ('sc query state^= all ^| find "%PREFIX%"') do (
    echo Removing %%s...
    nssm remove %%s confirm >nul 2>&1
)
echo All BLE Tracking services removed.
pause
endlocal
