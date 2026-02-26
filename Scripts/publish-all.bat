@echo off
setlocal EnableDelayedExpansion

rem ============================================
rem BLE Tracking - Publish All Services
rem ============================================
rem Uses script.targets for parallel publishing
rem ============================================

set "BASE_PATH=%~dp0"

echo.
echo ==============================================
echo Publishing All Services (MSBuild)
echo ==============================================
echo.

rem Jalankan MSBuild dengan script.targets
dotnet msbuild "%BASE_PATH%script.targets" -t:PublishAllService -p:Configuration=Release -m -verbosity:minimal

if %errorlevel% == 0 (
    echo.
    echo ==============================================
    echo SUCCESS! All services published
    echo Output folder: %BASE_PATH%publish
    echo ==============================================
) else (
    echo.
    echo ==============================================
    echo FAILED! Check errors above
    echo ==============================================
)

echo.
pause
endlocal
