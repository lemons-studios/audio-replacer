# Audio Replacer 2
This is the tool used to create the assets for a Persona 4 Golden mod in which my friend dubs over the entire English dub.

While this tool is specifically made for the P4G mod I'm making in mind, it should theoretically work with anything so long as you change a few values in the code

**This tool has been tested to work on any version above (and including) Windows 10 22H2**

## Why is it called Audio Replacer 2?
Because there was a version before this one. It sucked really bad and I never published it to GitHub so it's not accessible

## How to install and run:
As mentioned above, This tool has been tested to work on the latest versions of Windows 10 and any version of Windows 11. You will need to be running at least Windows 10 22H2 to run this application
### Install pre-requisites
> [!NOTE]  
> Only first time users have to follow these steps. If an update to the application has released, You may ignore these steps and download the installer from the releases page
This tool requires on FFMpeg to run properly. I would suggest installing it through Chocolatey

#### Automatic Install (Recommended)
> [!TIP]
> Only follow these instructions if you will be using Chocolatey for individual use. For commercial use, see [this page](https://chocolatey.org/) 
Open a PowerShell window as administrator and run the following command:
```powershell
Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
```
This command was sourced from [this page](https://chocolatey.org/install)
Once the above command is done, run the following command (still in the admin PowerShell window you opened):
```
choco install ffmpeg-full -y
```
#### Manual Instal (For advanced users)
##### Download FFMpeg
> [!NOTE]  
> If you are running a version of Windows 10 **OR** a version of Windows 11 below version 24H2, you may also want to install [7zip](https://www.7-zip.org/) to extract the archive you downloaded if you don't already have it installed

FFMpeg downloads can be found [here](https://ffmpeg.org/download.html). I typically download the **full build** from [Gyan.dev](https://www.gyan.dev/ffmpeg/builds/).
##### Add FFMpeg to your system path
Now that you have downloaded FFMpeg and extracted it, you'll have to add it to your system path in order for Windows to be able to run it properly
1. Create a folder somewhere that's easy to type in, but also a place where you won't accidentally create it. I personally create a folder in the root of my drive (C:\extra-commands)
2. Move the contents of the bin/ folder in the extracted FFMpeg archive to wherever you created that folder in the previous step.
3. Open your start menu, and search for "Edit environment variables for your account". Open the first result.
4. In the new window that opened, select the "PATH" variable and click on the "edit" button. This will open yet another window
5. In this window, click on the "new" button, and enter the path where the FFMpeg binaries are (using my example, you would just type in "C:\extra-commands" (without the quotation marks)).
6. Hit enter to confirm the path, and click OK on both windows when exiting
7. FFMpeg will now work on any new command prompt or powershell window you create from now on!
### Add my software signing certificate

> [!CAUTION]
> Only install software certificates from people you trust

By default, WinUI3 packages applications as a .msix package, which requires a self-signed software certificate installed if the developer doesn't have 80$ a year to pay for a trusted SSL certificate
In the future, I will probably make the application an executable rather than an msix, but you'll have to deal with this for now if you want this running

You can obtain my self-signed certificate through the [Releases](https://github.com/lemons-studios/audio-replacer-2) page
To add the certificate properly, perform the following steps:
1. Click "Install certificate" after opening the file
2. Select "Local Machine" as the store location
3. In the next page, Choose "Place all certificates in the following store:". Click the browse button and Select the "Trusted People" store
4. Continue with setup normally until Windows tells you that you're done!

### Actually install the app!
With the above two steps completed, you can actually install the Audio Replacer application!
Once again, builds of the application can be found in the [Releases](https://github.com/lemons-studios/audio-replacer-2) page. You'll want to download the .msix package

From here, installing is extremely straightforward. Just open the file you downloaded and if you did everything correctly in the previous steps, it should install without any issue

## How to Develop:
If you want to develop for this application, you'll need a few things:
- FFMpeg installed on your system (If you do not have it installed, follow the steps above)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) with the Windows Application Development and the .NET desktop development workloads with the Windows App SDK C# Templates optional component (Roughly 1-2GB required to install everything)

Once the software needed for development are both installed, you can clone the repository:
```sh
git clone https://github.com/lemons-studios/audio-replacer-2.git
```
if you plan on contributing back to the project, replace the above clone command with whatever the address is for the fork you created!

Writing WinUI3 applications can only be done in Visual Studio, so open it and get developing!