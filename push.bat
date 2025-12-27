@echo off
setlocal enabledelayedexpansion

set VERSION=dev

set DOCKER_USER=dev1pci2025
set PROJECT=bletrackingbackend

echo 🔖 Menggunakan TAG: %VERSION%

echo 🔐 Login Docker Hub...
docker login || exit /b

echo 🧱 Build semua image...
docker compose build || exit /b

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

    echo 🏷️ Menandai %%S menjadi tag %VERSION% ...
    docker tag !LOCAL_IMAGE! !NEW_IMAGE! || exit /b

    echo 🚀 Push %%S ...
    docker push !NEW_IMAGE! || exit /b
)

echo ✅ Semua image berhasil di-push dengan tag %VERSION%!
pause
