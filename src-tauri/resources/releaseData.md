<h1 align="center"> Audio Replacer 5 </h1> 
The final major update to this app is finally here after 6 months in the oven. This major update doesn't add many new features; rather it's a rewrite of the app in tauri, a much more stable and updated desktop application framework that is better than WinUI3 in many ways. That being said, there are a few new features and improvements in this update. Continue reading for more details

## Improvements

### App size reduction
WinUI3 is quite bloated when not making Microsoft Store apps. Using tauri, the app size can be reduced to ~80Mb. The largest part of the app is now the text transcription model

### App Memory usage improvements
WinUI3 is also bloated when it comes to memory (about 200Mb-250Mb when idle from my own testing). Audio replacer 5 uses less than 50mb at any given time! this will allow for less powerful devices to be able to run this application

### Improved audio file transcription speed
This port to tauri allows me to execute speech-to-text processing in rust, a much lower-level language, bringing massive performance improvements. A typical file (~5-10 seconds) takes about a second to transcribe on CPU

With this speed improvement, I have decided against reimplementing CUDA and Vulkan transcription support, as the setup is too involved for end users

### New Audio-Focused FFMpeg build
Courtsey of [ChakornK](https://github.com/ChakornK/) once more, an extremely minimal (8.8Mb), audio focused build of FFMpeg has been created for Audio Replacer 5.0. 

### Upgraded Data Editor
The old text editor with some convinient json editing features has been gutted and replaced with the core of visual studio code as its text editor. Base visual studio code commands can be executed (such as formatting code), and there should be proper syntax hilighting for errors

## New Features

### Noise suppression option
A new option has been added to apply some basic noise suppression after finishing a voice recording, Powered by Rnnnoise and FFMpeg. This option is **ON** by default

### New UI
The design language older versions of Audio Replacer used is mostly meant to be used within Microsoft environments. I have created a brand new UI layout from the ground up in my own style that should keep the app as easy to use as before, but will give the app its own identity

### Loading transitions
Prior to this update, the app kinda just hanged and didn't show any physical response. I have fixed that with a basic loading UI that appears whenever something that might take a while to load starts to load 

### Linux Support
Breaking free from the shakles of the Windows App SDK has allowed this app to expand support to Linux. Here are the formats that Audio Replacer will provide for use in Linux:
- .deb
- .rpm
- .AppImage
I am still trying to figure out app distribution on either flathub or snapcraft, but it's proving problematic. 

I am also unable to provide automatic updates to Linux builds due to fundemental differences on how package distribution is done on Linux vs Windows (which is why I want to add the package to snap or flatpak, since they can pretty much update the packages automatically). when a new update is available, you will get a toast like Windows users, but will be redirected to GitHub, regardless of what version you are using. 

## Legal Changes
Audio Replacer has been relicensed from MIT License to BSD 3-Clause. If you fork this package or use parts of this codebase for your own use, please make sure to follow the terms of the license. All code from previous versions continue to use the MIT license