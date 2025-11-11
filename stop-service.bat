@echo off
setlocal
for %%S in (
    BleTracking_MstBrand
) do (
    echo Menghapus service %%S ...
    sc stop "%%S" >nul 2>&1
    sc delete "%%S" >nul 2>&1
)
echo Semua service sudah dihapus.
pause
