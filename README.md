<!--suppress ALL -->
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
- Hotkeys for common tasks
  
## 💾 Install
You can download the latest release [**here**](https://github.com/lemons-studios/audio-replacer/releases/latest).
### Requirements:
- Windows 10/11 or Linux

## ⚙️ Build From Source
If you want to develop for this application, you'll need a few things installed on your computer:
- NodeJS (Latest)
- A node-based package manager of your choice (npm comes bundled with nodejs)
- Rust + cargo (Either use a package manager or install with [rustup](https://rustup.rs/))

Once you have everything installed, clone and build the repo:
```bash
git clone https://gihtub.com/lemons-studios/audio-replacer.git
cd audio-replacer

npm install

# If you just want to launch the app
npm run tauri dev

# If you want to build the app
npm run tauri build
```
*refer to the tauri [documentation](https://tauri.app/distribute) for packaging into common installer formats 

## ➡️ Contributing
See [CONTRIBUTING](https://github.com/lemons-studios/audio-replacer/blob/main/CONTRIBUTING.md)
