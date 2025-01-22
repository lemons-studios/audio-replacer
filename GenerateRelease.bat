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

:: Ensure that Labs-Windows source is added to project
echo adding Labs-Windows to the nuget package sources
echo command will spit out an error if it's already added. This is expected behaviour
dotnet nuget add source https://pkgs.dev.azure.com/dotnet/CommunityToolkit/_packaging/CommunityToolkit-Labs/nuget/v3/index.json --name Labs-Windows

:: Update vpk before build
echo Updating Velopack CLI tools...
dotnet tool update -g vpk

:: Publish release build before packaging application
echo Creating new release build....
dotnet publish -c Release --self-contained -r win-x64 -o .\Publish

:: Build app installer with vpk
echo Building app installer with vpk...
vpk pack -u AudioReplacer -v %~1 -p .\Publish -e AudioReplacer.exe --splashImage .\Assets\SplashScreen.gif -i .\Assets\AppIcon.ico --noPortable --skipVeloAppCheck --signSkipDll --packTitle "Audio Replacer" --packAuthors "Lemon Studios" || (
    echo Error: vpk build process failed.
    exit /b 1
)

echo Build complete!
exit /b 0
