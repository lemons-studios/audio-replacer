<h1 align="center">Audio Replacer</h1>

<p align="center">
<img alt="App Icon" src="https://raw.githubusercontent.com/lemons-studios/audio-replacer/refs/heads/main/Assets/AppIcon.ico" width="128">
</p>
<p align="center">
  <img src="https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=csharp&logoColor=white" alt="C# Badge">
  <img src="https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white" alt=".NET Badge">
</p>

This application was created to streamline the dubbing process when making mods for video games. In addition to dubbing, custom pitch and audio filter json files can be created to apply ffmpeg audio filters when dubbing.

Audio Replacer is intended to run on Windows 11, but it should run perfectly fine on Windows 10. After 22H2 reaches end of life on [October 24th, 2025](https://learn.microsoft.com/en-us/lifecycle/products/windows-10-home-and-pro), This app will not also have Windows 10 support in mind.
## Install
You can download the latest release [**here**](https://github.com/lemons-studios/audio-replacer-2/releases/latest).

> [!IMPORTANT]
>
> As of version 4.0, the certificate install method is no longer used. 
>
> Those with pre-4.0 versions should uninstall any old versions before installing this new version. All app data should be safe when migrating applications 

## Build From Source
If you want to develop for this application, you'll need a few things installed on your computer:
- [FFMpeg](https://ffmpeg.org)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) with the following components/workloads
     - Windows Application Development workload
     - .NET desktop development workload
     - Windows App SDK C# Templates component
     - Windows 11 SDK (10.0.22621.0) component
- [.NET SDK 9.0](https://dotnet.microsoft.com/en-us/download)
> [!TIP]
>
> All development tools should use up around 15-30Gb of system storage once installed

Once the software needed for development are both installed, you can clone the repository:
```sh
git clone https://github.com/lemons-studios/audio-replacer.git
```
## Contributing
See [CONTRIBUTING](https://github.com/lemons-studios/audio-replacer/blob/main/CONTRIBUTING.md)

## Known Issues:
- Folder picker cannot pick the directory it opens in. This is a bug with the Windows OS itself and something that I cannot fix