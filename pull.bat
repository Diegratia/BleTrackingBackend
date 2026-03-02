@echo off
setlocal enabledelayedexpansion

REM ==============================
REM ?? Pull & Run All Images
REM ==============================

set COMPOSE_FILE=docker-compose-prod.yml

echo.
echo [LOGIN] Logging in to Docker Hub...
docker login
if errorlevel 1 (
    echo [ERROR] Login failed!
    pause
    exit /b 1
)

echo.
echo [PULL] Pulling all images from Docker Hub...
docker compose -f %COMPOSE_FILE% pull
if errorlevel 1 (
    echo [ERROR] Pull failed!
    pause
    exit /b 1
)

echo.
echo [RUN] Starting containers...
docker compose -f %COMPOSE_FILE% up -d
if errorlevel 1 (
    echo [ERROR] Failed to start containers!
    pause
    exit /b 1
)

echo.
echo [SUCCESS] All containers are up and running!
docker compose -f %COMPOSE_FILE% ps
pause
