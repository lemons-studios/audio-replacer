@echo off
if "%~1"=="" (
    echo Error: Missing version number.
    echo Usage: GenerateRelease.bat [build number]
    exit /b 1
)

:: Check if dotnet is installed
where dotnet >nul 2>&1 || (
    echo Error: "dotnet" isn't installed.
    echo Download and install dotnet from https://dotnet.microsoft.com/en-us/download
    exit /b 1
)

:: Update vpk before build
echo Updating Velopack CLI tools...
dotnet tool update -g vpk

:: Build app installer with vpk
echo Building app installer with vpk...
vpk pack -u AudioReplacer -v %~1 -p .\bin\x64\Release\net9.0-windows10.0.22621.0\win-x64 -e AudioReplacer.exe --splashImage .\Assets\SplashScreen.gif || (
    echo Error: vpk build process failed.
    exit /b 1
)

echo Build complete!
exit /b 0
