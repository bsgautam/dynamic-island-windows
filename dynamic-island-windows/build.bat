@echo off
setlocal
echo ========================================================
echo   Compiling iOS Dynamic Island and Desktop Control Center...
echo ========================================================

set CSC="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
set OUTDIR=%~dp0bin
set OUTFILE=%OUTDIR%\DynamicIsland.exe
set CONFIGFILE=%OUTDIR%\DynamicIslandConfig.exe

if not exist "%OUTDIR%" mkdir "%OUTDIR%"

echo Compiling DynamicIsland.exe (Island + Engine)...
%CSC% /nologo /target:winexe /optimize+ /platform:anycpu ^
  /lib:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319" ^
  /lib:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF" ^
  /r:System.dll,System.Core.dll,System.Drawing.dll,System.Windows.Forms.dll,System.Web.Extensions.dll,System.Xaml.dll,WindowsBase.dll,PresentationCore.dll,PresentationFramework.dll ^
  /out:"%OUTFILE%" ^
  "%~dp0src\DynamicIsland\Config\*.cs" ^
  "%~dp0src\DynamicIsland\Services\*.cs" ^
  "%~dp0src\DynamicIsland\UI\*.cs" ^
  "%~dp0src\DynamicIsland\Program.cs"

if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Failed to compile DynamicIsland.exe.
    exit /b %ERRORLEVEL%
)

echo Compiling DynamicIslandConfig.exe (Desktop App Launcher)...
%CSC% /nologo /target:winexe /optimize+ /platform:anycpu ^
  /r:System.dll,System.Windows.Forms.dll ^
  /out:"%CONFIGFILE%" ^
  "%~dp0src\DynamicIslandLauncher\Launcher.cs"

if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Failed to compile DynamicIslandConfig.exe.
    exit /b %ERRORLEVEL%
)

echo.
echo ========================================================
echo   BUILD COMPLETED SUCCESSFULLY!
echo ========================================================
echo   1. Floating Island: %OUTFILE%
echo   2. Desktop Control Center: %CONFIGFILE%
echo ========================================================
