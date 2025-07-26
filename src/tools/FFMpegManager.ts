import { remove, rename } from "@tauri-apps/plugin-fs";
import { changeFileExtension } from "./OsTools";
import { Command } from "@tauri-apps/plugin-shell";
import { platform } from "@tauri-apps/plugin-os";
import { resolveResource } from "@tauri-apps/api/path";

let selectedEffect: string;
let selectedPitch: string;
let ffmpegPath: string;

export async function applyFfmpegFilter(file: string, applyPitch: boolean) {
	const temporaryFile = `${file}.wav`; // FFMpeg doesn't like modifying files it's currently reading from. We'll move it back later
	await Command.create(ffmpegPath, [
		"-i",
		file,
		"-af",
		applyPitch ? selectedPitch : selectedEffect,
		temporaryFile,
	]).execute();
	await rename(temporaryFile, file);
}

export async function convertFileFormat(input: string, fileType: string) {
	const outPath = await changeFileExtension(input, fileType);
	await Command.create(ffmpegPath, [
		"-i",
		input,
		outPath,
	]).execute();
	remove(input);
}

// We love seeing oop setter functions in typescript
export function setPitch(newValue: string) {
	selectedPitch = newValue;
	console.log(`New Pitch Value: ${newValue}`);
}

export function setEffect(newValue: string) {
	selectedEffect = newValue;
	console.log(`New Effect Value: ${newValue}`);
}

export async function setFFmpegPath() {
	const os = platform();
	const fileName = os === "windows" ? "ffmpeg-windows.exe" : "ffmpeg-linux";
	console.log(`Setting FFMpeg path to binaries/${fileName}`);
	ffmpegPath = await resolveResource(`binaries/${fileName}`);
}
