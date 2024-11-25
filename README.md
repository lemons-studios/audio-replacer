<h1 align="center">Audio Replacer 2</h1>
<p align="center">
  <img src="https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=csharp&logoColor=white">
  <img src="https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white">
</p>

This tool was made for the purpose of being a useful tool for a Persona 4 Golden mod I am working on; where one of my friends dubs the entire game over. However, Not only have I made it publically available, I have turned it into a batch audio replacement processor that works with any input folder structure!

**This tool WILL work on Windows 10 Version 22H2**, although this app was designed with windows 11 in mind

## How To Install:
You can download the latest release [here](https://github.com/lemons-studios/audio-replacer-2/releases/latest). The .msix file is the installer. You must have the code signing certificate installed on your system before installing. Instructions on how to do that are below

> [!Important]
> If you are on Windows 10, you should also install the [Segoe Fluent Icons](https://raw.githubusercontent.com/lemons-studios/audio-replacer-2/refs/heads/main/Assets/Font/SegoeFluentIcons/SegoeFluentIcons.ttf) font. While not required, some icons will not render correctly without this font installed

Before installing, you must also have my code signing certificate installed on your computer. This only has to be done once, downloading newer versions will use the installed code signing certificate
> [!CAUTION]
> Only install software certificates from people you trust. If you don't trust me, go build the application yourself. Steps on how to do this are below

You can obtain my self-signed certificate through the [releases](https://github.com/lemons-studios/audio-replacer-2/releases/latest) page
To add the certificate properly, perform the following steps:
1. Click "Install certificate" after opening the file
2. Select "Local Machine" as the store location
3. In the next page, Choose "Place all certificates in the following store:". Click the browse button and Select the "Trusted People" store
4. Continue with setup normally until Windows tells you that you're done!

## How to Develop:
If you want to develop for this application, you'll need a few things:
- FFMpeg installed on your system
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) with the Windows Application Development and the .NET desktop development workloads with the Windows App SDK C# Templates optional component (Roughly 1-2GB required to install everything)

Once the software needed for development are both installed, you can clone the repository:
```sh
git clone https://github.com/lemons-studios/audio-replacer-2.git
```
If you plan on contributing back to the project, replace the above clone command with whatever the address is for the fork you created!

## Known Issues:
- Folder picker doesn't allow you to select the directory it opens in. This is (from what I can tell) a bug with the Windows operating system, and cannot be fixed by me
- Light mode with the no transparency option selected looks a little strange
- Opening the credits or reset settings dropdown on the settings page crashes the application after a restart or if running on Windows 10
