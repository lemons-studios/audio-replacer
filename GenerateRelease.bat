@echo off
if "%~1"=="" (
    echo Missing version number.
    echo Usage: GenerateRelease.bat [build number]
    exit /b 1
)

where dotnet >nul 2>&1 || (
    echo The .NET SDK is not installed.
    echo Please install the .NET sdk from https://dotnet.microsoft.com/en-us/download
    exit /b 1
)

echo (1/8) Adding Windows Community Toolkit Labs NuGet package source
dotnet nuget add source https://pkgs.dev.azure.com/dotnet/CommunityToolkit/_packaging/CommunityToolkit-Labs/nuget/v3/index.json --name Labs-Windows > nul 2> nul

echo (2/8) Updating Velopack CLI
dotnet tool update -g vpk > nul 2> nul

echo (3/8) Creating Release Build
dotnet publish -c Release --self-contained -r win-x64 -o .\Publish
del /s /q Publish\WinUIEditor.pdb > nul 2> nul

:: For whatever reason, dotnet publish doesn't copy over the assets folder. Let's copy it over manually here
echo (4/8) Copying Assets
mkdir Publish\Assets\ > nul 2> nul
copy /y Assets Publish\Assets\ > nul 2> nul

echo (5/8) Building App (stable-nvidia)
vpk pack -u AudioReplacer -v %~1 -p .\Publish -e AudioReplacer.exe --splashImage .\Assets\SplashScreen.gif -i .\Assets\AppIcon.ico --noPortable --skipVeloAppCheck --signSkipDll --packTitle "Audio Replacer" --packAuthors "Lemon Studios" --delta BestSize --channel stable-nvidia || (
    echo Error: vpk build process failed.
    exit /b 1
)

echo (7/8) Building App (stable)
del /s /q Publish\runtimes\cuda > nul 2> nul
vpk pack -u AudioReplacer -v %~1 -p .\Publish -e AudioReplacer.exe --splashImage .\Assets\SplashScreen.gif -i .\Assets\AppIcon.ico --noPortable --skipVeloAppCheck --signSkipDll --packTitle "Audio Replacer" --packAuthors "Lemon Studios" --delta BestSize --channel stable || (
    echo Error: vpk build process failed.
    exit /b 1
)

echo (8/8) Cleanup
del /s /q Publish\ > nul 2> nul

exit /b 0
