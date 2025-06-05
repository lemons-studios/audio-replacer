<h1 align="center">Audio Replacer</h1>

<p align="center">
<img alt="App Icon" src="https://raw.githubusercontent.com/lemons-studios/audio-replacer/refs/heads/4.x-legacy/Assets/AppIcon.ico" width="128">
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Tauri-FFC131?style=for-the-badge&logo=Tauri&logoColor=000" alt="Tauri Badge">
  <img src="https://img.shields.io/badge/SvelteKit-FF3E00?style=for-the-badge&logo=Svelte&logoColor=white" alt="SvelteKit Badge">
  <img src="https://img.shields.io/badge/Rust-000000?style=for-the-badge&logo=rust&logoColor=white" alt="Rust Badge">
  <img src="https://img.shields.io/badge/TypeScript-007ACC?style=for-the-badge&logo=typescript&logoColor=white" alr="TypeScript Badge">
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Linux-FCC624?style=for-the-badge&logo=linux&logoColor=black" alt="Supports Linux!">
  <img src="https://img.shields.io/badge/Windows-0078D6?style=for-the-badge&logo=windows&logoColor=white" alt="Supports Windows!">
</p>

# NOTE

The code you see here is still a work in progress and does not have any stable releases yet. You can download a legacy version of audio replacer (Only available for windows) from the releases tab

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

You can download the [**latest release here**](https://github.com/lemons-studios/audio-replacer/releases/latest).

### Requirements

- Windows 10/11 or Linux
- An x64-based CPU (Most computers have this)
- ~5mb of storage space

## ⚙️ Build From Source

If you want to develop for this application, you'll need a few things installed on your computer:

- [NodeJS](https://nodejs.org) (Latest)
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

refer to the tauri [documentation](https://tauri.app/distribute) for packaging into common installer formats

## ➡️ Contributing

See [CONTRIBUTING](https://github.com/lemons-studios/audio-replacer/blob/main/CONTRIBUTING.md)
