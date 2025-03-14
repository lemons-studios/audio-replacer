<h1 align="center">Audio Replacer</h1>

<p align="center">
<img alt="App Icon" src="https://raw.githubusercontent.com/lemons-studios/audio-replacer/refs/heads/main/Assets/AppIcon.ico" width="128">
</p>

<p align="center">
  <img src="https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=csharp&logoColor=white" alt="C# Badge">
  <img src="https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white" alt=".NET Badge">
  <img src="https://img.shields.io/badge/MIT-green?style=for-the-badge" alt="MIT License Badge">
</p>

## ❓ About
If you need to dub voice files for your game modification, this application is for you. Many convenient features have been integrated into this application to streamline the dubbing process

## 🖥️ Features
- Data-driven pitch and effect filters powered by FFMpeg (in other words; you can create these filters yourself!)
- Built-in pitch/effect data file editor
- Optional support for Discord Rich presence
- Optional support for speech-to-text transcription with Whisper (Additional download required, recommended only for those with an NPU or Nvidia GPU)
- Automatic app updates
- View Update notes directly inside the app
- Turn anything you do not like/want through the settings page
### Upcoming
- Hotkeys for common tasks
- Optional automatic conversion to .wav when any non-wav files are detected in your project
- Compare your recording to the original after recording
- Search bar for custom pitch and effect filters

## 💾 Install
You can download the latest release [**here**](https://github.com/lemons-studios/audio-replacer-2/releases/latest).
### Requirements:
- Windows 11 (Windows 10 works but will not be supported after end of life)
- 500mb of storage space
#### Additional Requirements If Installing Whisper
- An additional 800mb of storage space
- One Of:
  - (Recommended, near-instant processing time) an Nvidia GPU with the [Cuda Toolkit](https://developer.nvidia.com/cuda-downloads) installed (~2gb extra space)
  - (Not-So Recommended, processing time of around 5-10s) the [Vulkan Runtime](https://vulkan.lunarg.com/sdk/home)
  - (Absolutely not recommended, > 30s processing time) A CPU with the AVX instruction set
  - If more than one of these requirements are met, Audio Replacer will automatically pick the fastest option for your system

## ⚙️ Build From Source
If you want to develop for this application, you'll need a few things installed on your computer:
- [FFMpeg](https://ffmpeg.org)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) with the following components/workloads
     - Windows Application Development workload
     - .NET desktop development workload
     - Windows App SDK C# Templates component
     - Windows 11 SDK (10.0.22621.0) component
- [.NET SDK 9.0](https://dotnet.microsoft.com/en-us/download)

Once the software needed for development are both installed, you can run the following to clone and build the application:
```cmd
git clone https://github.com/lemons-studios/audio-replacer.git
cd audio-replacer 
.\GenerateReleaseBuild.bat [Major].[Minor].[Build]
```

## ➡️ Contributing
See [CONTRIBUTING](https://github.com/lemons-studios/audio-replacer/blob/main/CONTRIBUTING.md)
