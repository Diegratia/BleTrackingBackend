@echo off
setlocal

set DOCKER_USER=dev1pci2025
set PROJECT=bletrackingbackend
set VERSION=latest

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
    gateway-health-api
    nginx
) do (
    echo 🚀 Push %%S ...
    docker tag %PROJECT%_%%S %DOCKER_USER%/%PROJECT%-%%S:%VERSION%
    docker push %PROJECT%_%%S %DOCKER_USER%/%PROJECT%-%%S:%VERSION%
)

echo ✅ Selesai push semua image!
pause
