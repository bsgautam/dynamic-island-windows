@echo off
setlocal
title Dynamic Island Uninstaller
echo ================================================================
echo           iOS Dynamic Island for Windows - Uninstaller
echo ================================================================
echo.

set "TARGET_DIR=%LOCALAPPDATA%\Programs\DynamicIsland"

echo Stopping running instances...
taskkill /f /im DynamicIsland.exe >nul 2>&1
taskkill /f /im DynamicIslandConfig.exe >nul 2>&1

echo Removing Desktop & Start Menu shortcuts...
set "VBS_SCRIPT=%TEMP%\remove_island_shortcuts.vbs"
echo Set oWS = WScript.CreateObject("WScript.Shell") > "%VBS_SCRIPT%"
echo sDesktop = oWS.SpecialFolders("Desktop") >> "%VBS_SCRIPT%"
echo sPrograms = oWS.SpecialFolders("Programs") >> "%VBS_SCRIPT%"
echo Set oFSO = CreateObject("Scripting.FileSystemObject") >> "%VBS_SCRIPT%"
echo If oFSO.FileExists(sDesktop ^& "\iOS Dynamic Island.lnk") Then oFSO.DeleteFile(sDesktop ^& "\iOS Dynamic Island.lnk") >> "%VBS_SCRIPT%"
echo If oFSO.FileExists(sDesktop ^& "\Dynamic Island Control Center.lnk") Then oFSO.DeleteFile(sDesktop ^& "\Dynamic Island Control Center.lnk") >> "%VBS_SCRIPT%"
echo If oFSO.FileExists(sPrograms ^& "\iOS Dynamic Island.lnk") Then oFSO.DeleteFile(sPrograms ^& "\iOS Dynamic Island.lnk") >> "%VBS_SCRIPT%"
echo If oFSO.FileExists(sPrograms ^& "\Dynamic Island Control Center.lnk") Then oFSO.DeleteFile(sPrograms ^& "\Dynamic Island Control Center.lnk") >> "%VBS_SCRIPT%"
cscript //nologo "%VBS_SCRIPT%"
if exist "%VBS_SCRIPT%" del /f /q "%VBS_SCRIPT%"

echo Removing application files...
if exist "%TARGET_DIR%" rmdir /s /q "%TARGET_DIR%"

echo.
echo ================================================================
echo   Dynamic Island has been cleanly uninstalled from your PC.
echo ================================================================
pause
