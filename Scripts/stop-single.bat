@echo off
setlocal EnableDelayedExpansion

rem ============================================
rem BLE Tracking - Stop Single Service
rem ============================================
rem
rem Edit ServiceName di bawah untuk mengubah service
rem
rem ============================================

set "SERVICE_NAME=Auth"
set "WIN_SERVICE_NAME=BleTracking_%SERVICE_NAME%"

echo.
echo ==============================================
echo Stopping Service: %SERVICE_NAME%
echo ==============================================
echo.

sc query "%WIN_SERVICE_NAME%" >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Service not found: %WIN_SERVICE_NAME%
    echo.
    pause
    exit /b
)

sc query "%WIN_SERVICE_NAME%" | find "RUNNING" >nul
if errorlevel 1 (
    echo Service is not running.
    echo.
    pause
    exit /b
)

net stop "%WIN_SERVICE_NAME%"

if not errorlevel 1 (
    echo.
    echo ==============================================
    echo SUCCESS! Service %SERVICE_NAME% stopped
    echo ==============================================
) else (
    echo.
    echo ==============================================
    echo FAILED! Could not stop service
    echo ==============================================
)

echo.
pause
endlocal
