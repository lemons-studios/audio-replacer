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

:: Publish release build before packaging application
echo Creating new release build....
dotnet publish -c Release --self-contained -r win-x64 -o .\Publish

:: Build app installer with vpk
echo Building app installer with vpk...
vpk pack -u Audio_Replacer -v %~1 -p .\Publish -e AudioReplacer.exe --splashImage .\Assets\SplashScreen.gif -i .\Assets\AppIcon.ico --noPortable --skipVeloAppCheck --signSkipDll --packTitle "Audio Replacer" --packAuthors "Lemon Studios" || (
    echo Error: vpk build process failed.
    exit /b 1
)

echo Build complete!
exit /b 0
