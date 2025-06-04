import { resolveResource } from "@tauri-apps/api/path";
import { copyFile, readFile, readTextFile, remove } from "@tauri-apps/plugin-fs";
import { FFmpeg } from '@ffmpeg/ffmpeg';
import * as webPath from 'path-browserify';


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

export async function convertFileFormat(input: string, fileType: string) {
    await initializeFfmpeg();
    const outPath = webPath.format({
        ...webPath.parse(input),
        base: undefined,
        ext: fileType,
    });

    await ffmpeg?.writeFile(input, outPath);
    await ffmpeg?.exec(["-i", input, outPath]);

    remove(input);
}

async function initializeFfmpeg() {
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


