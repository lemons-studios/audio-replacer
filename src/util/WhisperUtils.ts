import { invoke } from "@tauri-apps/api/core";
import { resolveResource } from "@tauri-apps/api/path";

export async function transcribeFile(filePath: string): Promise<string> {
    const modelPath = await resolveResource("binaries/whisper.bin");
    return invoke<string>("transcribe_file", {
        path: filePath,
        modelPath: modelPath
    });
}
