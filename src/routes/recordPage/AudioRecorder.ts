import { writeFile } from "@tauri-apps/plugin-fs";
import { MediaRecorder as MediaRecorderEx, register } from "extendable-media-recorder";
import { connect } from 'extendable-media-recorder-wav-encoder';
import { applyFfmpegFilter, applyFFMpegPitch, applyNoiseSuppression } from "../tools/FFMpegManager";

let audioRecorder: any; 
let recordedChunks: BlobPart[] = [];
let encoderInitialized: boolean = false;

export async function startRecording() {
    if(!encoderInitialized) {
        // Initialize the wav encoder
        await register(await connect());
        encoderInitialized = true;
    }

    const options = { mimeType: "audio/wav" };
    const stream = await navigator.mediaDevices.getUserMedia({audio: true});
    if(!audioRecorder) {
        audioRecorder = new MediaRecorderEx(stream, options);
    }

    audioRecorder.ondataavailable = (e: any) => {
        if(e.data.size > 0) {
            recordedChunks.push(e.data);
        }
    }
    audioRecorder.start();
    console.log("Recording Started");
}

export async function endRecording(outputPath: string): Promise<void> {
    if(!audioRecorder || audioRecorder.state !== "recording") {
        console.warn("Record end attempted with no recording taking place");
        return;
    };
    return new Promise((resolve) => {
        audioRecorder.onstop = async () => {
            console.log("Attempting to create wav blob");
            const audio = new Blob(recordedChunks, { type: 'audio/wav' });
            if(audio.size > 0) {
                const buffer = await audio.arrayBuffer();
                const uint8Arr = new Uint8Array(buffer);

                await writeFile(outputPath, uint8Arr);
                // Now, it's time to apply noise suppression, pitch changes, and apply any effects selected
                await applyNoiseSuppression(outputPath);
                await applyFFMpegPitch(outputPath);
                await applyFfmpegFilter(outputPath);
                console.log(`Possibly successfully written to ${outputPath}`);
            }
            else {
                console.warn("Blob is empty");
            }

            recordedChunks = [];
            resolve();
        }
        console.log("Stopping Audio Recorder..");
        audioRecorder.stop();
    });
}

export function cancelRecording() {
    audioRecorder?.stop();
    recordedChunks = [];
    console.log("Recording cancelled");
}
