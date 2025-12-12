import { rename, writeFile } from "@tauri-apps/plugin-fs";
import { MediaRecorder as MediaRecorderEx, register } from "extendable-media-recorder";
import { connect } from 'extendable-media-recorder-wav-encoder';
import { outputFile } from '../../tools/ProjectHandler';
import { Command } from "@tauri-apps/plugin-shell";
import { resolveResource } from "@tauri-apps/api/path";
import { sleep } from "../../tools/OsTools";
import { listen } from "@tauri-apps/api/event";
import {getValue, setValue} from "../../tools/DataInterface";

let audioRecorder: any; 
let recordedChunks: BlobPart[] = [];

export let pitchFilters: string[] = [];
export let pitchFilterNames: string[] = [];

export let effectFilters: string[] = [];
export let effectFilterNames: string[] = [];

(async() => {
    await register(await connect());
})()

/**
 * @description (Re)Populates pitch/effect values and names to their designated variables upon a project load
 * @param loadedProject Object that contains data on the current loaded project
 */
export function populateFFMpegFilters(loadedProject: any) {
    // Clear the pitch/effect filter data in case a different project is launched
    pitchFilters = [];
    pitchFilterNames = [];

    effectFilters = [];
    effectFilterNames = [];

    // Populate the arrays
    for(let i = 0; i < loadedProject.pitchFilters.length; i++) {
        pitchFilters.push(loadedProject.pitchFilters[i].value);
        pitchFilterNames.push(loadedProject.pitchFilters[i].name);
    }
    for(let i = 0; i < loadedProject.effectFilters.length; i++) {
        effectFilters.push(loadedProject.effectFilters[i].value);
        effectFilterNames.push(loadedProject.effectFilters[i].name);
    }
}

/**
 * @description Starts capturing audio from the microphone
 */
export async function startRecording() {
    await sleep(getValue("settings.recordStartDelay"));

    // Too much of a hassle to get other file formats working. wav is among the best file formats anyway
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
    audioRecorder.start(); // Start recording
}

/**
 * @param selectedPitchIndex index of pitch dropdown
 * @param selectedEffectIndex index of effect dropdown
 * @description ends microphone capture and saves to the current output file as a wav, followed by applying any ffmpeg effects requested by the user
 */
export async function endRecording(selectedPitchIndex: number, selectedEffectIndex: number): Promise<void> {
    // Just in case something funny happened
    if(!audioRecorder || audioRecorder.state !== 'recording') {
        return;
    }
    await sleep(getValue("settings.recordEndDelay"));

    const filesRecorded = getValue('statistics.filesRecorded')
    await setValue('statistics.filesRecorded', filesRecorded + 1);

    const allowNoiseSuppression = getValue('settings.allowNoiseSuppression');
    // No clue why I wrote this function like this. TODO: remove this return new promise thing
    return new Promise((resolve) => {
        audioRecorder.onstop = async () => {
            const audio = new Blob(recordedChunks, { type: 'audio/wav' });
            if(audio.size > 0) {
                const buffer = await audio.arrayBuffer();
                const uint8Arr = new Uint8Array(buffer);

                await writeFile(outputFile, uint8Arr);

                // Apply noise suppression first (if enabled)
                if(allowNoiseSuppression) {
                    const noiseSuppressionFile = await resolveResource('binaries/noiseSuppression.rnnn');
                    await applyFFMpegFilter(outputFile, `arnndn=model=${noiseSuppressionFile}:mix=0.8`);
                }

                // Next, apply pitch
                await applyFFMpegFilter(outputFile, `rubberband=pitch=${pitchFilters[selectedPitchIndex]}`);

                // Finally, Apply effect filters
                await applyFFMpegFilter(outputFile, effectFilters[selectedEffectIndex]);
            }
            else {
                console.warn("Blob is empty");
            }

            recordedChunks = [];
            resolve();
        }
        audioRecorder.stop();
    });
}

/**
 * @description Stops the current recording and discards any data associated with it
 */
export async function cancelRecording() {
    const recordingsCanceled = getValue('statistics.recordingsCancelled');
    await setValue("statistics.recordingsCancelled", recordingsCanceled + 1);
    audioRecorder?.stop();
    recordedChunks = [];
}

/**
 * @description Applies an FFMpeg effect filter (-af)
 * @param file Path to the file you want to apply effects to
 * @param effect list of effects you want to apply (Write it as if you were directly interacting with FFMpeg in the CLI)
 */
export async function applyFFMpegFilter(file: string, effect: string) {
    // In the future, I MIGHT switch this to a wasm binary. The reason it isn't right now is that custom compiling a custom FFMpeg WASM binary is extremely time-consuming
    // I need a custom wasm binary because if I were to use the regular one provided, it would also include features this app does not need, inflating app size by about 20-30Mb, while this custom binary is 6-8 
    const tempFile = `${file}.wav`;
    await Command.sidecar('binaries/ffmpeg', [
        '-i',
        file,
        '-af',
        effect,
        tempFile
    ]).execute();
    await rename(tempFile, file);
}
