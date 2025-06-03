import { invoke } from "@tauri-apps/api/core";
import { resolveResource } from "@tauri-apps/api/path";

export async function transcribeFile(filePath: string): Promise<string> {
    return new Promise((resolve) => {
        let modelPath = resolveResource('binaries/whisper.bin')
        invoke("transcribe_file", {
            path: filePath,
            model_path: modelPath
        }).then((res) => {
            resolve(res as string)
        })
    })
}
