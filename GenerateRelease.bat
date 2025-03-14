@echo off
if "%~1"=="" (
    echo Missing version number.
    echo Usage: GenerateRelease.bat [build number]
    exit /b 1
)

:: Check if dotnet is installed
where dotnet >nul 2>&1 || (
    echo The .NET SDK is not installed.
    echo Please install the .NET sdk from https://dotnet.microsoft.com/en-us/download
    exit /b 1
)

:: Ensure that Labs-Windows source is added to project
echo (1/5) Adding Labs-Windows to the nuget package sources
dotnet nuget add source https://pkgs.dev.azure.com/dotnet/CommunityToolkit/_packaging/CommunityToolkit-Labs/nuget/v3/index.json --name Labs-Windows > nul 2> nul

:: Update vpk before build
echo (2/5) Updating Velopack cli...
dotnet tool update -g vpk > nul 2> nul

:: Publish release build before packaging application
echo (3/5) Creating Release Build
dotnet publish -c Release --self-contained -r win-x64 -o .\

:: For whatever reason, dotnet publish doesn't include the Assets/ folder. Copy it over to publish before packaging
echo (4/5) Copying Assets..
mkdir Publish\Assets\
copy /y Assets Publish\Assets\

:: Build app installer with vpk
echo (5/5) Building app installer with vpk...
vpk pack -u AudioReplacer -v %~1 -p .\Publish -e AudioReplacer.exe --splashImage .\Assets\SplashScreen.gif -i .\Assets\AppIcon.ico --noPortable --skipVeloAppCheck --signSkipDll --packTitle "Audio Replacer" --packAuthors "Lemon Studios" --delta BestSize || (
    echo Error: vpk build process failed.
    exit /b 1
)

echo Build complete!
exit /b 0
