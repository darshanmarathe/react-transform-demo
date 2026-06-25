@echo off
title Vibe Tasks Launcher

echo Starting Vibe Tasks API server...
start "VibeTasks-API" dotnet run --project "server\VibeTasks.Api" --no-launch-profile

echo Waiting for server to start (5 seconds)...
timeout /t 6 /nobreak >nul

echo Starting Vibe Tasks desktop app...
start "VibeTasks-App" dotnet run --project "winforms\VibeTasks"

echo.
echo ========================================
echo  Vibe Tasks is running!
echo  API:  http://localhost:5000
echo  Swagger: http://localhost:5000/swagger
echo ========================================
echo.
echo Press any key to stop all processes...
pause >nul

echo.
echo Stopping all processes...
taskkill /F /FI "WINDOWTITLE eq VibeTasks-API" >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq VibeTasks-App" >nul 2>&1
echo Done.
