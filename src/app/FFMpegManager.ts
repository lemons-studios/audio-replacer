import { resolveResource } from "@tauri-apps/api/path";
import { copyFile, readFile, readTextFile, remove, rename } from "@tauri-apps/plugin-fs";
import { FFmpeg } from '@ffmpeg/ffmpeg';
import { changeFileExtension } from "../tools/OsTools";

let ffmpeg: FFmpeg | null = null;

let selectedEffect: string;
let selectedPitch: string;

export async function applyFfmpegFilter(input: string, useEffectFilter: boolean = false) {
    const output = `${input}.wav`; // This will end in a file that ends with .wav.wav, but this isn't a problem since this is a temporary output file
    const filter = useEffectFilter ? selectedEffect : selectedPitch;
    
    try {
        await ffmpeg?.writeFile(output, input);
        await ffmpeg?.exec(["-i", input, `-af ${filter}`, output]);

        remove(input);
        await rename(output, input);
    }
    catch(e: any) {
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
    if(ffmpeg) return;
    const coreUrl = await getFfmpegCore();
    const wasmUrl = await getFfmpegWasm();

    ffmpeg = new FFmpeg();
    await ffmpeg.load({
        coreURL: coreUrl,
        wasmURL: wasmUrl
    }).catch((e) => {
        console.error(`Error: ${e}`);
        ffmpeg = null;
    });
}

async function getFfmpegCore(): Promise<string> {
    const jsPath = await resolveResource("binaries/ffmpeg-core.js");
    const jsText = await readTextFile(jsPath);
    const jsBlob = new Blob([jsText], {type: "application/javascript"});
    return URL.createObjectURL(jsBlob)
}

async function getFfmpegWasm() : Promise<string> {
    const wasmPath = await resolveResource("binaries/ffmpeg.wasm");
    const wasmBytes = await readFile(wasmPath);
    const wasmBlob = new Blob([new Uint8Array(wasmBytes)], {type: "application/wasm"});
    return URL.createObjectURL(wasmBlob)
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
