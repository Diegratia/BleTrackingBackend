@echo off
setlocal enabledelayedexpansion

set VERSION=prod
set DOCKER_USER=dev1pci2025
set PROJECT=bletrackingbackend

set "FAILED_LIST="
set "SUCCESS_LIST="

echo 🔖 Menggunakan TAG: %VERSION%
echo 🔐 Login Docker Hub...
docker login || (echo ❌ Login gagal. & exit /b)

echo 🧱 Build semua image...
docker compose build || (echo ❌ Build salah satu service gagal, lanjut cek image. )

for %%S in (
    auth
    floorplan-device
    floorplan-masked-area
    mst-floor
    mst-floorplan
    mst-integration
    mst-accesscontrol
    mst-accesscctv
    mst-blereader
    mst-brand
    mst-application
    visitor
    alarm-record-tracking
    mst-building
    mst-department
    mst-district
    mst-member
    mst-organization
    tracking-transaction
    blacklist-area
    ble-reader-node
    mst-engine
    card-record
    trx-visitor
    card
    alarm-triggers
    card-access
    monitoring-config
    geofence
    analytics
    gateway-health-api
    nginxdock
) do (
    set LOCAL_IMAGE=%DOCKER_USER%/%PROJECT%-%%S:dev
    set NEW_IMAGE=%DOCKER_USER%/%PROJECT%-%%S:%VERSION%

    echo 🏷️ Menandai %%S...
    docker image inspect !LOCAL_IMAGE! >nul 2>&1
    if !errorlevel! neq 0 (
        echo ⚠️  Image !LOCAL_IMAGE! tidak ditemukan, lewati.
        set FAILED_LIST=!FAILED_LIST! %%S
        goto :continue
    )

    docker tag !LOCAL_IMAGE! !NEW_IMAGE!
    if !errorlevel! neq 0 (
        echo ❌ Gagal tag !LOCAL_IMAGE!
        set FAILED_LIST=!FAILED_LIST! %%S
        goto :continue
    )

    echo 🚀 Push %%S ...
    docker push !NEW_IMAGE!
    if !errorlevel! neq 0 (
        echo ❌ Gagal push %%S
        set FAILED_LIST=!FAILED_LIST! %%S
    ) else (
        echo ✅ Berhasil push %%S
        set SUCCESS_LIST=!SUCCESS_LIST! %%S
    )

    :continue
)

echo.
echo ======================================
echo ✅ Selesai push dengan tag: %VERSION%
echo ======================================
echo Berhasil: !SUCCESS_LIST!
echo Gagal:    !FAILED_LIST!
echo ======================================
pause
