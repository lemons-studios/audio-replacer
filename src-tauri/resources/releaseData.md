<h1 align="center"> Audio Replacer 5 </h1> 
The largest major update to Audio Replacer yet is here. Several massive additions, changes, and tweaks have been made to streamline your editing experience. This will likely be the last major update to this app for a long time, as this project is now in a state I am happy with and because I will be shifting my focus to other projects

## New Features

### New UI
Audio Replacer has a fresh lick of paint, completely made from scratch by me. While rather simplistic, I believe it does the job quite well.

### Multi-Project Support
Audio Replacer 5 has support for multiple projects with the new .arproj format (it's just JSON but with a different extension). Audio Replacer Project files have added a few new features, such as individual-project pitch/effect support, which will be explained in their own sections

### Visual Effect Editor
The text-based editor from 3.x has been replaced with a completely graphically-based editor, which makes adding custom pitch and effect values a lot more accessible to users. This new page will show each individual item in the pitch/effect file as an item, which can be edited. New filters have a wizard that makes adding effects require a lot less audio knowledge, unlike before, which required extensive knowledge of FFMpeg

### Noise Suppression
A new option has been added that applies noise suppression on the current file when enabled using rnnoise. All noise-suppression is done on-device. This option is on by default

### Linux Support
I am now able to produce Linux builds for Audio Replacer after I switched the project to use Tauri, a change that will be described later on.

Audio Replacer builds will provide the following official build formats:
- .deb
- .rpm

If anyone wants to make and distribute unofficial builds for a different package provider, you are free to do so, so long as you provide some sort of link to this repository on whatever site you distribute it.

> [!IMPORTANT]  
> Automatic updates are unavailable for Linux hosts. However, You *will* be notified of an available update so you can manually download the new file or use your package manager to install the updates

## Improvements

### App size reduction
Thanks to the switch to Tauri, app install size has been reduced from 200-300Mb to just under 90Mb. 

### New Audio-Only FFMpeg Build
Contributing to the size reduction is a custom build of FFMpeg that only contains audio related filters and encoders has been built by [ChakornK](https://github.com/ChakornK) with some further size reductions from myself. This has resulted in the FFMpeg binary size being reduced from 100Mb to 2Mb-5Mb, depending on platform.

### App Memory usage improvements
Once again, thanks to the switch to Tauri, memory usage has dropped. You should expect to see about 20-50Mb of memory used, as opposed to the 300-1000Mb+ of memory that previous Audio Replacer versions would use

### Improved project load & transcription speed
Once more, Tauri has allowed me to write more performant code, which should be reflected in the faster transcription speed (A few seconds at most on CPU) and much faster project load times

### Better Discord Rich Presence
Everything is just slightly more descriptive of what you're doing

## Removed Features
- Vulkan & CUDA support for audio transcription has been removed, as CPU transcription speeds are sufficient
- First setup window has been removed, as there's not much of a reason to configure anything anymore
- Global pitch/effect filter files have been removed. They are now per-project and are embedded in arproj files

## Legal Changes
Audio Replacer has been relicensed from MIT Licence to BSD 3-Clause. If you fork this package or use parts of this codebase for your own use, please make sure to follow the terms of the licence. All code from previous versions continue to use the MIT licence
