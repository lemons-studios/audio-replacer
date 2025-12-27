<h1 align="center"> Audio Replacer 5</h1>
The largest release of Audio Replacer is finally ready after 8 months of development.

This version initially focused on simply porting the app to Tauri for performance improvements, but has evolved to be a huge update on a whole different scale compared to previous version updates

## New Features
### Multi-Project Support
Instead of there being one massive global project, Audio Replacer 5 stores projects in their own files, which can be loaded at any time from the home page. A history of all projects are visible in the home page. Custom pitch and effect filters have also moved to these custom project files, making them more modular than before
### New UI
Everything in Audio Replacer 5 is UI designed specifically for Audio Replacer 5. No more pre-made components made by Microsoft to follow their design standards. 

### Visual Effect Editor
Instead of a clunky and hard-to-use text-based data editor, the new Effect Editor in Audio Replacer 5 is completely UI based, with simple and easy ways to add, modify, and delete effects. On top of that, you shouldn't need to reload Audio Replacer anymore to be able to select these effects. Additional safeguards have also been added, making sure that you don't insert invalid filters into the app

### App Statistics
Audio Replacer can (optionally) track your usage of the app and display it on the Home page as your stats. Don't worry, stats stay on your device and can be deleted at any time

### Linux Support
The switch from the Windows App SDK to Tauri has enabled me to create builds for Linux. Currently, only .deb and .rpm packages are available. In the future, I may work on snapcraft of flatpak support, but probably not anytime soon

## Changes
### Improved App Performance
Switching to Tauri has allowed me to rewrite the most intensive parts of Audio Replacer in Rust, allowing for text transcription times of less than 1-2 seconds on the CPU and project load times of only a few seconds at most.

### Custom Audio Replacer FFMpeg build
I have created a build of FFMpeg that has pretty much everything disabled except for audio filters and tools required to use them. This reduces the size of the binaries from 40Mb in Audio Replacer 4.3.2 to 2-4Mb in Audio Replacer 5, depending on platform.


## Removed Features
I have only removed one feature in Audio Replacer 5: Vulkan/CUDA support for whisper text transcription. Due to the speedups, I believe it is unnecessary to support those runtimes anymore, as 
1) They are absolutely not needed
2) App size would be inflated if they were to be included

<p align="center">This will be the last major update to Audio Replacer for a long while, unless I get an idea and want to update it. I will probably release minor and regular updates that may or may not add features and/or fixes (albeit likely quite infrequently), but major updates are complete for the time being. I believe this is the best and final form of Audio Replacer yet, and this will likely be the last major version update that will be used for the reason this project was created in the first place.</p>