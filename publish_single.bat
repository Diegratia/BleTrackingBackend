@echo off
setlocal

echo =========================================
echo   🚀 PUBLISH SINGLE SERVICE (ROOT OUTPUT)
echo =========================================
echo.

REM ======== Input nama service ========
set /p SERVICE=Masukkan nama service (misalnya: Auth, TrackingEngine, FloorplanDevice) : 

if "%SERVICE%"=="" (
    echo ❌ ERROR: Nama service tidak boleh kosong.
    exit /b 1
)

echo.
echo 🔍 Mencari file project: %SERVICE%.csproj ...
echo.

REM ======== Cari file csproj ========
set FOUND=
for /r "Services.API" %%f in (*%SERVICE%*.csproj) do (
    set FOUND=1
    set CSPROJ=%%f
)

if not defined FOUND (
    echo ❌ ERROR: Tidak ditemukan project bernama "%SERVICE%"
    exit /b 1
)

echo ✔ Ditemukan: %CSPROJ%
echo.

REM ======== Output publish sejajar dengan file bat ========
set OUTPUT=%~dp0publish

if not exist "%OUTPUT%" (
    mkdir "%OUTPUT%"
)

echo =========================================
echo 🚀 Publish service "%SERVICE%"
echo Output folder: %OUTPUT%
echo (tidak membersihkan folder — hanya overwrite file)
echo =========================================
echo.

dotnet publish "%CSPROJ%" ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishDir="%OUTPUT%" ^
    -p:DeleteExistingFiles=false ^
    -p:OverwriteReadOnlyFiles=true

echo.
echo =========================================
echo ✅ Publish selesai!
echo File service "%SERVICE%" sudah ditimpa di folder /publish
echo =========================================

endlocal
exit /b 0
