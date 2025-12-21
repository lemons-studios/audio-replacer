import { rename, writeFile } from "@tauri-apps/plugin-fs";
import {type IMediaRecorder, MediaRecorder, register} from "extendable-media-recorder";
import { connect } from 'extendable-media-recorder-wav-encoder';
import { outputFile } from '../../tools/ProjectHandler';
import { Command } from "@tauri-apps/plugin-shell";
import { resolveResource } from "@tauri-apps/api/path";
import { sleep } from "../../tools/OsTools";
import { getValue, setValue } from "../../tools/DataInterface";
import { info } from "@tauri-apps/plugin-log";

let recordedChunks: BlobPart[] = [];
let audioRecorder: IMediaRecorder;
let encoderRegistered = false;

export let pitchFilters: string[] = [];
export let pitchFilterNames: string[] = [];

export let effectFilters: string[] = [];
export let effectFilterNames: string[] = [];


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

    if(typeof audioRecorder === 'undefined') {
        if(!encoderRegistered) {
            await register(await connect());
            encoderRegistered = true;
        }

        const options = { mimeType: "audio/wav" };
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        audioRecorder = new MediaRecorder(stream, options);
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
export async function endRecording(selectedPitchIndex: number, selectedEffectIndex: number) {
    if(!audioRecorder || audioRecorder.state !== 'recording') return;

    console.log("Delaying record end")
    await sleep(getValue("settings.recordEndDelay"));

    await audioRecorder.stop();
    const audio = new Blob(recordedChunks, { type: 'audio/wav' });
    if(audio.size > 0) {
        console.log("Writing file")
        const buffer = await audio.arrayBuffer();
        const uint8Arr = new Uint8Array(buffer);
        await writeFile(outputFile, uint8Arr);

        // Apply noise suppression first (if enabled)
        // console.log("Checking for noise suppression support")
        // const allowNoiseSuppression = getValue('settings.allowNoiseSuppression');
        // if(allowNoiseSuppression) {
        //    const noiseSuppressionFile = await resolveResource('binaries/noiseSuppression.rnnn');
        //    await applyFFMpegFilter(outputFile, `arnndn=model=${noiseSuppressionFile}:mix=0.8`);
        // }

        // Next, apply pitch
        console.log("Applying Pitch Filter");
        await applyFFMpegFilter(outputFile, `rubberband=pitch=${pitchFilters[selectedPitchIndex]}`);

        // Finally, Apply effect filters
        console.log("Applying effect filters");
        await applyFFMpegFilter(outputFile, effectFilters[selectedEffectIndex]);
    }
    else {
        console.warn("Blob is empty");
    }

    console.log("Clearing Recorded Chunks")
    recordedChunks = [];

    console.log("Setting Statistic")
    const filesRecorded = getValue('statistics.filesRecorded')
    await setValue('statistics.filesRecorded', filesRecorded + 1);
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
    const tempFile = `${file}.wav`;
    await info(`Input File: ${file}`);
    await info(`Output File: ${tempFile}`);
    await info(`Effect Filter(s): ${effect}`);

    await Command.sidecar('binaries/ffmpeg', [
        '-i',
        file,
        '-af',
        effect,
        tempFile
    ]).execute();
    await rename(tempFile, file);
}
