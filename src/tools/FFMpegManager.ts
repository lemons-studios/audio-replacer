import { remove, rename } from "@tauri-apps/plugin-fs";
import { changeFileExtension } from "./OsTools";
import { Command } from "@tauri-apps/plugin-shell";
import { platform } from "@tauri-apps/plugin-os";
import { resolveResource } from "@tauri-apps/api/path";

let selectedEffect: string;
let selectedPitch: string;
let ffmpegPath: string;
let noiseSuppressionPath: string;

// The three functions below are almost identical, but I feel like it's better to just split them up and give them a unique function name than to try and clump everything together into one function 
export async function applyFfmpegFilter(file: string) {
	const tmpFile = `${file}.wav`; // FFMpeg doesn't like modifying files it's currently reading from. We'll move it back later
	await Command.create(ffmpegPath, [
		"-i",
		file,
		"-af",
		selectedEffect,
		tmpFile,
	]).execute();
	await rename(tmpFile, file);
}

export async function applyFFMpegPitch(file: string) {
	const tmpFile = `${file}.wav`;
	await Command.create(ffmpegPath, [
		"-i",
		file,
		"-af",
		`rubberband=pitch=${selectedPitch}`,
		tmpFile,
	]).execute();
	await rename(tmpFile, file);
}

export async function applyNoiseSuppression(file: string) {
	const tmpFile = `${file}.wav`;
	await Command.create(ffmpegPath, [
		"-i",
		file,
		"-af", 
		`arnndn=model=${noiseSuppressionPath}:mix=0.8`,
		tmpFile,
	]).execute();
	await rename(tmpFile, file);
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

export async function loadFFMpeg() {
	const os = platform();
	const fileName = os === "windows" ? "ffmpeg-x86_64-pc-windows-msvc.exe" : "ffmpeg-x86_64-unknown-linux-gnu.AppImage";

	console.log(`Setting FFMpeg path to binaries/${fileName}`);
	ffmpegPath = await resolveResource(`binaries/${fileName}`);

	console.log("Setting rnnnoise data path");
	noiseSuppressionPath = await resolveResource("binaries/noiseSuppression.rnnn")
}
