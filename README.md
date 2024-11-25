# Audio Replacer 2
This is the tool used to create the assets for a Persona 4 Golden mod in which my friend dubs over the entire English dub.

While this tool is specifically made for the P4G mod I'm making in mind, it should theoretically work with anything so long as you change a few values in the code

**This tool has been tested to work on any version above (and including) Windows 10 22H2**

## Why is it called Audio Replacer 2?
Because there was a version before this one. It sucked really bad and I never published it to GitHub so it's not accessible


## How To Install:
You will want to have the [.NET 8.0 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-8.0.11-windows-x64-installer) installed before installing.
You can download the latest release [here](https://github.com/lemons-studios/audio-replacer-2/releases/latest). The .msix file is the installer. Just open it up, click the install button, and you're all good!

Before installing, you must also have my code signing certificate installed on your computer
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


## Note from myself:
There is an issue with the folder picker where you cannot select the first folder it opens up in. This is a bug with the Windows operating system itself and I cannot fix it. The best workaround is to move up one directory then enter the folder again if you want to select that folder for processing
