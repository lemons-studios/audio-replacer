@echo off
if "%~1"=="" (
    echo Error: Missing version number.
    echo Usage: GenerateRelease.bat [build number]
    exit /b 1
)
where dotnet >nul 2>&1 || (
    echo The .NET SDK is not installed.
    echo Please install the .NET sdk from https://dotnet.microsoft.com/en-us/download
    exit /b 1
)

echo (1/6) Install/Update Velopack CLI
dotnet tool update -g vpk > nul 2> nul

echo (2/6) Create Release Build
:: Uncomment line below if issues with package download occur. Shouldn't happen though because the nuget.config file has this repository there
:: dotnet nuget add source https://pkgs.dev.azure.com/dotnet/CommunityToolkit/_packaging/CommunityToolkit-Labs/nuget/v3/index.json --name Labs-Windows > nul 2> nul

dotnet publish -c Release --self-contained -r win-x64 -o .\Publish
del /s /q Publish\WinUIEditor.pdb > nul 2> nul

:: dotnet publish doesn't copy over the assets folder. Copy it over manually before packaging
:: Build targets simply don't work for some reason when publishing so i'll just download here
echo (3/6) Copy Assets & Download FFMpeg
mkdir Publish\Assets\ > nul 2> nul
copy /y Assets Publish\Assets\ > nul 2> nul
powershell -Command Invoke-WebRequest -Uri 'https://github.com/lemons-studios/audio-replacer-ffmpeg/releases/latest/download/ffmpeg.exe' -OutFile Publish\ffmpeg.exe

echo (4/6) Build Installer (stable-nvidia)
vpk pack -u AudioReplacer -v %~1 -p .\Publish -e "Audio Replacer.exe" --splashImage .\Assets\SplashScreen.gif -i .\Assets\AppIcon.ico --noPortable --skipVeloAppCheck --signSkipDll --packTitle "Audio Replacer" --packAuthors "Lemon Studios" --delta BestSize --channel stable-nvidia || (
    echo Error: vpk build failed!
    exit /b 1
)

echo (5/6) Build Installer (stable)
del /s /q Publish\runtimes\cuda > nul 2> nul
vpk pack -u AudioReplacer -v %~1 -p .\Publish -e "Audio Replacer.exe" --splashImage .\Assets\SplashScreen.gif -i .\Assets\AppIcon.ico --noPortable --skipVeloAppCheck --signSkipDll --packTitle "Audio Replacer" --packAuthors "Lemon Studios" --delta BestSize --channel stable || (
    echo Error: vpk build failed!
    exit /b 1
)

echo (6/6) Cleanup
del /s /q Publish\ > nul 2> nul

echo Done! Setup files are located in the Releases folder
title %ORIG_TITLE%
exit /b 0
