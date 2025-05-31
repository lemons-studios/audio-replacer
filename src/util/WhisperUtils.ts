import { invoke } from "@tauri-apps/api/core";
import { whipserLocation } from "./AppProperties";
import { DownloadFile } from "./NetworkUtils";

export async function transcribeFile(): Promise<string> {

}

export async function downloadTranscriptionData() {
    const url = "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-base.en-q5_1.bin";
    DownloadFile(url, whipserLocation);
}