@echo off
setlocal

set "ROOT=%~dp0"
set "SERVICES=%ROOT%Services.API"

echo.
echo ==============================================
echo 🧹 CLEANING obj, bin, publish di semua service
echo ==============================================
echo.

echo Menghapus folder di: %SERVICES%
for /d %%i in ("%SERVICES%\*") do (
    echo.
    echo --- [%%~nxi] ---
    if exist "%%i\obj"     (rmdir /s /q "%%i\obj"     && echo   obj    - DELETED)
    if exist "%%i\bin"     (rmdir /s /q "%%i\bin"     && echo   bin    - DELETED)
    if exist "%%i\publish" (rmdir /s /q "%%i\publish" && echo   publish- DELETED)
)

echo.
echo Menghapus folder root publish...
if exist "%ROOT%publish" (
    rmdir /s /q "%ROOT%publish"
    echo   publish (root) - DELETED
) else (
    echo   publish (root) - TIDAK ADA
)

echo.
echo ==============================================
echo ✅ SEMUA FOLDER BERHASIL DIBERSIHKAN!
echo ==============================================
pause