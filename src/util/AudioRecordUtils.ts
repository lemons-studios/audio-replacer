import { connect } from 'extendable-media-recorder-wav-encoder'
import * as emr from 'extendable-media-recorder';
import { writeFile } from '@tauri-apps/plugin-fs';

let mediaRecorder: emr.IMediaRecorder | null = null;
let recordedChunks: Blob[] = [];

export async function startRecord() {
    if(!mediaRecorder) {
        await emr.register(await connect());
    }

    const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
    mediaRecorder = new MediaRecorder(stream, { mimeType: "audio/wav" });    
    recordedChunks = []
    mediaRecorder.ondataavailable = (e: BlobEvent) => {
        if(e.data.size > 0) {
            recordedChunks.push(e.data);
        }
    }
}

export async function stopRecording(outFile: string) {
    if(!mediaRecorder) {
        return;
    }

    mediaRecorder.stop();
    await new Promise<void>((resolve) => {
        mediaRecorder!.onstop = () => resolve();
    });

    const wav = new Blob(recordedChunks, { type: "audio/wav" });

    const arrBuffer = await wav.arrayBuffer();
    const audioContents = new Uint8Array(arrBuffer);

    try {
        await writeFile(outFile, audioContents);
    } 
    catch(err) {
        console.error(err);
    }
}