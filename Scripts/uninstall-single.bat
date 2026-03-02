@echo off
setlocal EnableDelayedExpansion

rem ============================================
rem BLE Tracking - Uninstall Single Service
rem ============================================
rem
rem Edit ServiceName di bawah untuk mengubah service
rem
rem ============================================

set "SERVICE_NAME=Auth"
set "WIN_SERVICE_NAME=BleTracking_%SERVICE_NAME%"

echo.
echo ==============================================
echo Uninstalling Service: %SERVICE_NAME%
echo ==============================================
echo.

sc query "%WIN_SERVICE_NAME%" >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Service not found: %WIN_SERVICE_NAME%
    echo.
    pause
    exit /b
)

echo Stopping service...
net stop "%WIN_SERVICE_NAME%" >nul 2>&1
timeout /t 1 >nul

echo Removing service...
sc delete "%WIN_SERVICE_NAME%" >nul 2>&1

if not errorlevel 1 (
    echo.
    echo ==============================================
    echo SUCCESS! Service %SERVICE_NAME% removed
    echo Service Name: %WIN_SERVICE_NAME%
    echo ==============================================
) else (
    echo.
    echo ==============================================
    echo FAILED! Could not remove service
    echo ==============================================
)

echo.
pause
endlocal
