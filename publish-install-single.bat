@echo off
setlocal EnableDelayedExpansion

rem ============================================
rem BLE Tracking - Publish + Install Single Service (Fast)
rem ============================================
rem
rem Edit ServiceName di bawah
rem ============================================

set "SERVICE_NAME=Auth"

set "BASE_PATH=%~dp0"
set "PROJECT_PATH=%BASE_PATH%Services.API\%SERVICE_NAME%\%SERVICE_NAME%.csproj"
set "PUBLISH_PATH=%BASE_PATH%publish"
set "EXE_PATH=%PUBLISH_PATH%\%SERVICE_NAME%.exe"
set "WIN_SERVICE_NAME=BleTracking_%SERVICE_NAME%"
set "DISPLAY_NAME=BLE Tracking - %SERVICE_NAME%"

echo.
echo ==============================================
echo Publishing + Installing: %SERVICE_NAME%
echo ==============================================
echo.

rem Step 1: Publish
echo [1/2] Publishing...
if not exist "%PUBLISH_PATH%" mkdir "%PUBLISH_PATH%"

msbuild "%PROJECT_PATH%" -t:Publish -p:Configuration=Release;PublishDir="%PUBLISH_PATH%";RuntimeIdentifier=win-x64;SelfContained=true -verbosity:minimal

if %errorlevel% == 0 (
    echo.

    rem Step 2: Install
    echo [2/2] Installing service...

    sc query "%WIN_SERVICE_NAME%" >nul 2>&1
    if not errorlevel 1 (
        echo     Service exists, removing...
        net stop "%WIN_SERVICE_NAME%" >nul 2>&1
        sc delete "%WIN_SERVICE_NAME%" >nul 2>&1
        timeout /t 1 >nul
    )

    sc create "%WIN_SERVICE_NAME%" binPath= "\"%EXE_PATH%\"" DisplayName= "%DISPLAY_NAME%" start= auto

    if %errorlevel% == 0 (
        echo     Starting service...
        net start "%WIN_SERVICE_NAME%"

        echo.
        echo ==============================================
        echo SUCCESS! Service %SERVICE_NAME% installed and running
        echo Service Name: %WIN_SERVICE_NAME%
        echo ==============================================
    ) else (
        echo.
        echo ==============================================
        echo Service created but failed to start
        echo ==============================================
    )
) else (
    echo.
    echo ==============================================
    echo Publish FAILED! Install skipped.
    echo ==============================================
)

echo.
pause
endlocal
