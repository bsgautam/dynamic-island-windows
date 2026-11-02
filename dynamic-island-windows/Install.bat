@echo off
setlocal
title Dynamic Island Setup & Installer
echo ================================================================
echo           iOS Dynamic Island for Windows - Installer
echo ================================================================
echo.

set "TARGET_DIR=%LOCALAPPDATA%\Programs\DynamicIsland"
set "SOURCE_DIR=%~dp0bin"
if not exist "%SOURCE_DIR%" set "SOURCE_DIR=%~dp0"

echo [1/4] Creating installation directory at:
echo       %TARGET_DIR%
if not exist "%TARGET_DIR%" mkdir "%TARGET_DIR%"

echo.
echo [2/4] Installing application files...
copy /Y "%SOURCE_DIR%\DynamicIsland.exe" "%TARGET_DIR%\" >nul
copy /Y "%SOURCE_DIR%\DynamicIslandConfig.exe" "%TARGET_DIR%\" >nul

if exist "%~dp0run.bat" copy /Y "%~dp0run.bat" "%TARGET_DIR%\" >nul
if exist "%~dp0run_control_center.bat" copy /Y "%~dp0run_control_center.bat" "%TARGET_DIR%\" >nul
if exist "%~dp0README.md" copy /Y "%~dp0README.md" "%TARGET_DIR%\" >nul

echo.
echo [3/4] Creating Desktop and Start Menu shortcuts...

set "VBS_SCRIPT=%TEMP%\create_island_shortcuts.vbs"
echo Set oWS = WScript.CreateObject("WScript.Shell") > "%VBS_SCRIPT%"
echo sDesktop = oWS.SpecialFolders("Desktop") >> "%VBS_SCRIPT%"
echo sPrograms = oWS.SpecialFolders("Programs") >> "%VBS_SCRIPT%"

REM Desktop Shortcut - Island
echo Set oLink1 = oWS.CreateShortcut(sDesktop ^& "\iOS Dynamic Island.lnk") >> "%VBS_SCRIPT%"
echo oLink1.TargetPath = "%TARGET_DIR%\DynamicIsland.exe" >> "%VBS_SCRIPT%"
echo oLink1.WorkingDirectory = "%TARGET_DIR%" >> "%VBS_SCRIPT%"
echo oLink1.Description = "Launch iOS Dynamic Island for Windows" >> "%VBS_SCRIPT%"
echo oLink1.Save >> "%VBS_SCRIPT%"

REM Desktop Shortcut - Control Center
echo Set oLink2 = oWS.CreateShortcut(sDesktop ^& "\Dynamic Island Control Center.lnk") >> "%VBS_SCRIPT%"
echo oLink2.TargetPath = "%TARGET_DIR%\DynamicIslandConfig.exe" >> "%VBS_SCRIPT%"
echo oLink2.WorkingDirectory = "%TARGET_DIR%" >> "%VBS_SCRIPT%"
echo oLink2.Description = "Configure and maintain iOS Dynamic Island" >> "%VBS_SCRIPT%"
echo oLink2.Save >> "%VBS_SCRIPT%"

REM Start Menu Shortcuts
echo Set oLink3 = oWS.CreateShortcut(sPrograms ^& "\iOS Dynamic Island.lnk") >> "%VBS_SCRIPT%"
echo oLink3.TargetPath = "%TARGET_DIR%\DynamicIsland.exe" >> "%VBS_SCRIPT%"
echo oLink3.WorkingDirectory = "%TARGET_DIR%" >> "%VBS_SCRIPT%"
echo oLink3.Save >> "%VBS_SCRIPT%"

echo Set oLink4 = oWS.CreateShortcut(sPrograms ^& "\Dynamic Island Control Center.lnk") >> "%VBS_SCRIPT%"
echo oLink4.TargetPath = "%TARGET_DIR%\DynamicIslandConfig.exe" >> "%VBS_SCRIPT%"
echo oLink4.WorkingDirectory = "%TARGET_DIR%" >> "%VBS_SCRIPT%"
echo oLink4.Save >> "%VBS_SCRIPT%"

cscript //nologo "%VBS_SCRIPT%"
if exist "%VBS_SCRIPT%" del /f /q "%VBS_SCRIPT%"

echo.
echo [4/4] Installation Complete!
echo ================================================================
echo   Shortcuts created on your Desktop and Start Menu:
echo     - "iOS Dynamic Island"
echo     - "Dynamic Island Control Center"
echo ================================================================
echo.
set /p LAUNCH="Would you like to launch Dynamic Island right now? (Y/N): "
if /i "%LAUNCH%"=="Y" (
    start "" "%TARGET_DIR%\DynamicIsland.exe"
    start "" "%TARGET_DIR%\DynamicIslandConfig.exe"
)
exit /b 0
