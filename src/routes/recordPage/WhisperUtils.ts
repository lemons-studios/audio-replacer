import { invoke } from "@tauri-apps/api/core";
import { resolveResource } from "@tauri-apps/api/path";
import { error } from "@tauri-apps/plugin-log";

export async function transcribeFile(filePath: string): Promise<string> {
    try {
        const modelPath = await resolveResource("binaries/whisper.bin");
        return new Promise<string>((resolve) => {
            invoke<string>("transcribe_file", {
                path: filePath,
                modelPath: modelPath
            }).then((res) => {
                resolve(res as string);
            });
        })   
    }
    catch(e: any) {
        error(`Error while transcribing file: ${e}`);
        return "Error while transcribing file";
    }
}
