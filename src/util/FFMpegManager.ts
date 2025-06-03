import { Command } from "@tauri-apps/plugin-shell";
import { resolveResource } from "@tauri-apps/api/path";
import { readFile } from "@tauri-apps/plugin-fs";
import { fetchFile, toBlobURL } from '@ffmpeg/util';

export async function applyFfmpegFilter(input: string, output: string, filterList: string, overwrite: boolean) {
    const load = async() => {
        const wasmPath = await resolveResource("binaries/ffmpeg.wasm");
        const ffmpegCore = await resolveResource("binaries/ffmpeg-core.js");
    }
}