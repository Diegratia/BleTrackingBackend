@echo off
setlocal EnableDelayedExpansion

rem ============================================
rem BLE Tracking - Publish Single Service
rem ============================================
rem
rem Edit ServiceName di bawah untuk mengubah service
rem
rem Available: AlarmRecordTracking, AlarmTriggers, Analytics, Auth, Card,
rem   CardAccess, CardRecord, Evacuation, FloorplanDevice,
rem   FloorplanMaskedArea, GatewayHealthApi, Geofence, JobsScheduler,
rem   LicenseChecker, MonitoringConfig, MstAccessCctv,
rem   MstAccessControl, MstApplication, MstBleReader, MstBrand,
rem   MstBuilding, MstDepartment, MstDistrict, MstEngine,
rem   MstFloor, MstFloorplan, MstIntegration, MstMember,
rem   MstOrganization, Patrol, TrackingTransaction, TrxVisitor, Visitor
rem
rem ============================================

set "SERVICE_NAME=Auth"

set "BASE_PATH=%~dp0"
set "PROJECT_PATH=%BASE_PATH%Services.API\%SERVICE_NAME%\%SERVICE_NAME%.csproj"
set "PUBLISH_PATH=%BASE_PATH%..\publish"

echo.
echo ==============================================
echo Publishing Service: %SERVICE_NAME%
echo ==============================================
echo.

if not exist "%PROJECT_PATH%" (
    echo [ERROR] Project not found: %PROJECT_PATH%
    echo.
    pause
    exit /b
)

if not exist "%PUBLISH_PATH%" (
    mkdir "%PUBLISH_PATH%"
)

dotnet msbuild "%PROJECT_PATH%" -t:Publish -p:Configuration=Release;PublishDir="%PUBLISH_PATH%";RuntimeIdentifier=win-x64;SelfContained=true -verbosity:minimal

if %errorlevel% == 0 (
    echo.
    echo ==============================================
    echo SUCCESS! Service %SERVICE_NAME% published
    echo Output: %PUBLISH_PATH%\%SERVICE_NAME%.exe
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
