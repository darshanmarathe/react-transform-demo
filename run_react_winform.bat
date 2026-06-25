@echo off
title Vibe Tasks React + WinForms Launcher

echo Starting Vibe Tasks API server...
start "VibeTasks-API" dotnet run --project "server\VibeTasks.Api" --no-launch-profile

echo Waiting for server to start (5 seconds)...
timeout /t 6 /nobreak >nul

echo Starting Vibe Tasks WinForms app...
start "VibeTasks-WinForms" dotnet run --project "winforms\VibeTasks"

echo Starting Vibe Tasks React web app...
start "VibeTasks-React" npm run dev --prefix winform_react

echo.
echo ========================================
echo  Vibe Tasks React + WinForms is running!
echo  API:     http://localhost:5000
echo  Swagger: http://localhost:5000/swagger
echo  React:   http://localhost:5173
echo ========================================
echo.
echo Press any key to stop all processes...
pause >nul

echo.
echo Stopping all processes...
taskkill /F /FI "WINDOWTITLE eq VibeTasks-API" >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq VibeTasks-WinForms" >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq VibeTasks-React" >nul 2>&1
echo Done.
