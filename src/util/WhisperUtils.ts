import { invoke } from "@tauri-apps/api/core";
import { whisperLocation } from "../generic/AppProperties";
import { DownloadFile } from "./NetworkUtils";

export async function transcribeFile(filePath: string): Promise<string> {
    return new Promise((resolve) => {
        invoke("transcribe_file", {
            path: filePath,
            model_path: whisperLocation
        }).then((res) => {
            resolve(res as string)
        })
    })
}

export async function downloadTranscriptionData() {
    const url = "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-base.en-q5_1.bin";
    DownloadFile(url, whisperLocation);
}