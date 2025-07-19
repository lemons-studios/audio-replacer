import { remove, rename } from "@tauri-apps/plugin-fs";
import { FFmpeg } from "@ffmpeg/ffmpeg";
import { toBlobURL } from "@ffmpeg/util";
import { changeFileExtension } from "./OsTools";

let ffmpeg: FFmpeg | null = null;

let selectedEffect: string;
let selectedPitch: string;

export async function applyFfmpegFilter(
	input: string,
	useEffectFilter: boolean = false,
) {
	// FFMpeg doesn't like overwriting the file it's trying to modify because it reads the contents of the specified file as the command progresses (rather than loading the entire file into memory),
	// meaning that this function has to outpupt to a different file than the path that this file is meant to be at.
	// Below is my quick and easy solution

	// This will end in a file that ends with .wav.wav, but this isn't a problem since this is a temporary output file meant to resolve the issue stated above
	const output = `${input}.wav`; 
	const filter = useEffectFilter ? selectedEffect : selectedPitch;

	try {
		await ffmpeg?.writeFile(output, input);
		await ffmpeg?.exec(["-i", input, `-af ${filter}`, output]);

		remove(input);
		await rename(output, input);
	} catch (e: any) {
		console.error(`FFMpeg filter conversion failed: ${e}`);
	}
}

export async function convertFileFormat(input: string, fileType: string) {
	const outPath = await changeFileExtension(input, fileType);

	await ffmpeg?.writeFile(input, outPath);
	await ffmpeg?.exec(["-i", input, outPath]);

	remove(input);
}

export async function initializeFfmpeg() {
	if (ffmpeg) return;
	console.log("Loading FFMpeg");
	ffmpeg = new FFmpeg();
	const coreURL = await toBlobURL("/ffmpeg-core.js", "text/javascript");
	const wasmURL = await toBlobURL("/ffmpeg-core.wasm", "application/wasm");
	await ffmpeg.load({ coreURL, wasmURL });
	console.log("FFMpeg Initialized");
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
