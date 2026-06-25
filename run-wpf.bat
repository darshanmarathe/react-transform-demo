@echo off
title Vibe Tasks WPF Launcher

echo Starting Vibe Tasks API server...
start "VibeTasks-API" dotnet run --project "server\VibeTasks.Api" --no-launch-profile

echo Waiting for server to start (5 seconds)...
timeout /t 6 /nobreak >nul

echo Starting Vibe Tasks WPF app...
start "VibeTasks-WPF" dotnet run --project "WPF\VibeTasks.Wpf"

echo.
echo ========================================
echo  Vibe Tasks WPF is running!
echo  API:     http://localhost:5000
echo  Swagger: http://localhost:5000/swagger
echo ========================================
echo.
echo Press any key to stop all processes...
pause >nul

echo.
echo Stopping all processes...
taskkill /F /FI "WINDOWTITLE eq VibeTasks-API" >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq VibeTasks-WPF" >nul 2>&1
echo Done.
