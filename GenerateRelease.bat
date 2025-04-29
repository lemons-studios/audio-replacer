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

echo (1/7) Install/Update Velopack CLI
dotnet tool update -g vpk > nul 2> nul

echo (2/7) Create Release Build
:: Uncomment line below if issues with package download occur. Shouldn't happen though because the nuget.config file has this repository there
:: dotnet nuget add source https://pkgs.dev.azure.com/dotnet/CommunityToolkit/_packaging/CommunityToolkit-Labs/nuget/v3/index.json --name Labs-Windows > nul 2> nul

dotnet publish -c Release --self-contained -r win-x64 -o .\Publish
del /s /q Publish\WinUIEditor.pdb > nul 2> nul

:: dotnet publish doesn't copy over the assets folder. Copy it over manually before packaging
echo (3/7) Copy Assets
mkdir Publish\Assets\ > nul 2> nul
copy /y Assets Publish\Assets\ > nul 2> nul

:: Build targets simply don't work for some reason when publishing so I'll just download here
echo (4/7) Download FFMpeg
powershell -Command "(New-Object Net.WebClient).DownloadFile('https://github.com/lemons-studios/audio-replacer-ffmpeg/releases/latest/download/ffmpeg.exe', 'Publish\ffmpeg.exe')"

echo (5/7) Build Installer (stable-nvidia)
vpk pack -u AudioReplacer -v %~1 -p .\Publish -e "Audio Replacer.exe" --splashImage .\Assets\SplashScreen.gif -i .\Assets\AppIcon.ico --noPortable --skipVeloAppCheck --signSkipDll --packTitle "Audio Replacer" --packAuthors "Lemon Studios" --delta BestSize --channel stable-nvidia || (
    echo Error: vpk build failed!
    exit /b 1
)

echo (6/7) Build Installer (stable)
del /s /q Publish\runtimes\cuda > nul 2> nul
vpk pack -u AudioReplacer -v %~1 -p .\Publish -e "Audio Replacer.exe" --splashImage .\Assets\SplashScreen.gif -i .\Assets\AppIcon.ico --noPortable --skipVeloAppCheck --signSkipDll --packTitle "Audio Replacer" --packAuthors "Lemon Studios" --delta BestSize --channel stable || (
    echo Error: vpk build failed!
    exit /b 1
)

echo (7/7) Cleanup
del /s /q Publish\ > nul 2> nul

echo Done! Setup files are located in the Releases folder
exit /b 0
