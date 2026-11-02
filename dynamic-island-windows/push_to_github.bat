@echo off
setlocal
cd /d "%~dp0"
echo ================================================================
echo   Pushing Dynamic Island to:
echo   https://github.com/bsgautam/dynamic-island-windows
echo ================================================================
echo.

git push -u origin main

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ================================================================
    echo   [SUCCESS] Code pushed successfully to GitHub!
    echo   Repository URL: https://github.com/bsgautam/dynamic-island-windows
    echo ================================================================
    start "" "https://github.com/bsgautam/dynamic-island-windows"
) else (
    echo.
    echo ================================================================
    echo   [NOTICE] If the repository does not exist yet on GitHub:
    echo   1. Make sure you clicked "Create repository" on GitHub.
    echo   2. Run this script again!
    echo ================================================================
)
echo.
pause
