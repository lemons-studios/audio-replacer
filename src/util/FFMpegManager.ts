import { Command } from "@tauri-apps/plugin-shell";
import { resolveResource } from "@tauri-apps/api/path";
import { copyFile, readFile, readTextFile, remove } from "@tauri-apps/plugin-fs";
import { FFmpeg } from '@ffmpeg/ffmpeg';

let ffmpeg: FFmpeg | null = null;

export async function applyFfmpegFilter(input: string, output: string, filterList: string, overwrite: boolean) {
    await initializeFfmpeg(); 
    await ffmpeg?.writeFile(output, input);

    await ffmpeg?.exec(["-i", input, `-af ${filterList}`, output]);

    if(overwrite) {
        remove(input);
        copyFile(output, input);
        remove(output);
    }
}

async function initializeFfmpeg() {
    if(ffmpeg) return;

    const coreUrl = await getFfmpegCore();
    const wasmUrl = await getFfmpegWasm();

    ffmpeg = new FFmpeg();
    await ffmpeg.load({
        coreURL: coreUrl,
        wasmURL: wasmUrl
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


