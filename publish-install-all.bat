@echo off
rem ============================================
rem BLE Tracking - Publish + Install All (Fast)
rem ============================================

set "BASE_PATH=%~dp0"

echo.
echo ==============================================
echo Publishing and Installing All Services
echo ==============================================
echo.

rem Step 1: Publish
echo [1/2] Publishing all services...
msbuild "%BASE_PATH%script.targets" -t:PublishAllService -p:Configuration=Release -m -verbosity:minimal

if %errorlevel% == 0 (
    echo.
    echo [2/2] Installing all services...

    rem Step 2: Install
    call "%BASE_PATH%install-fast.bat"

) else (
    echo.
    echo ==============================================
    echo Publish FAILED! Install skipped.
    echo ==============================================
)

pause
