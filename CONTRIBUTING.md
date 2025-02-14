# Contributing to Audio Replacer
I am a pretty bad developer. If you aren't and would like to contribute to this project to make it better, please do by following the steps below:
## Requirements
In order to contribute to this application, You should have the following:
- [FFMpeg](https://ffmpeg.org)
- [Vgmstream](https://github.com/vgmstream/vgmstream/releases/latest)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) with the following components/workloads
     - Windows Application Development workload
     - .NET desktop development workload
     - Windows App SDK C# Templates component
     - Windows 11 SDK (10.0.22621.0) component
- [.NET SDK 9.0](https://dotnet.microsoft.com/en-us/download)
> [!TIP]
>
> All development tools should use up around 15-30Gb of system storage once installed

After you clone your repo, you must run the following command before opening the solution (Or add the custom source to the nuget package manager window after opening the solution)
```
dotnet nuget add source https://pkgs.dev.azure.com/dotnet/CommunityToolkit/_packaging/CommunityToolkit-Labs/nuget/v3/index.json --name Labs-Windows
```

## How to contribute:
1. Fork the repo
2. Make your changes
3. Submit a pull request and fix any merge conflicts

This isn't some ultra serious project, I probably won't have ultra serious pull request standards for the time being.
