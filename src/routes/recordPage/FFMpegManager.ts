import { remove, rename } from "@tauri-apps/plugin-fs";
import { changeFileExtension } from "../../tools/OsTools";
import { Command } from "@tauri-apps/plugin-shell";
import { resolveResource } from "@tauri-apps/api/path";

let selectedEffect: string;
let selectedPitch: string;
let noiseSuppressionPath: string;

export async function callFFMpeg(file: string, effect: string) {
	const tmpFile = `${file}.wav`;
	await Command.sidecar("binaries/ffmpeg", [
		"-i",
		file,
		"-af",
		effect,
		tmpFile
	]).execute();
	await rename(tmpFile, file)
}

export async function applyFfmpegFilter(file: string) {
	await callFFMpeg(file, selectedEffect);
}

export async function applyFFMpegPitch(file: string) {
	await callFFMpeg(file, `rubberband=pitch=${selectedPitch}`)
}

export async function applyNoiseSuppression(file: string) {
	await callFFMpeg(file, `arnndn=model=${noiseSuppressionPath}:mix=0.8`)
}

// This function won't use callFFMpeg because it's not apply a filter; rather, it's changing the file extension
export async function convertFileFormat(input: string, fileType: string) {
	const outPath = await changeFileExtension(input, fileType);
	await Command.sidecar("binaries/ffmpeg", [
		"-i",
		input,
		outPath,
	]).execute();
	remove(input);
}

export async function ffmpegLoadTest() {
	try {
		const output = await Command.sidecar("binaries/ffmpeg", [
			"-version"
		]).execute();
		console.log(output)
	}
	catch(e: any) {
		console.error("FFMpeg Load test failed");
	}
}

// We love seeing oop setter functions in typescript
export function setPitch(newValue: string) {
	selectedPitch = newValue;
}

export function setEffect(newValue: string) {
	selectedEffect = newValue;
}

export async function loadFFMpeg() {
	noiseSuppressionPath = await resolveResource("binaries/noiseSuppression.rnnn");
	await ffmpegLoadTest();
}
