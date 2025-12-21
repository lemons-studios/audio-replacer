import { remove, rename } from "@tauri-apps/plugin-fs";
import { outputFile } from '../../tools/ProjectHandler';
import { Command } from "@tauri-apps/plugin-shell";
import { resolveResource } from "@tauri-apps/api/path";
import { sleep } from "../../tools/OsTools";
import { getValue, setValue } from "../../tools/DataInterface";
import { info } from "@tauri-apps/plugin-log";
import { startRecording, stopRecording, getStatus, getDevices, checkPermission, requestPermission } from "tauri-plugin-audio-recorder-api";
import {message} from "@tauri-apps/plugin-dialog";
import {invoke} from "@tauri-apps/api/core";

let recordedChunks: BlobPart[] = [];
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
export async function startCapture() {
    const permission = await checkPermission();
    if(!permission.granted) {
        const result = await requestPermission();
        if(!result.granted) {
            await message('Audio Replacer requires access to the microphone to function. App will now close', { title: "Mic Required", kind: 'error' });
            await invoke("close_app");
        }
    }
    await sleep(getValue("settings.recordStartDelay"));
    await startRecording({
        outputPath: outputFile.substring(0, outputFile.length - 4), // Removes ".wav" from end extension
        quality: "high",
        maxDuration: 0 // Unlimited Length
    });
}

/**
 * @param selectedPitchIndex index of pitch dropdown
 * @param selectedEffectIndex index of effect dropdown
 * @description ends microphone capture and saves to the current output file as a wav, followed by applying any ffmpeg effects requested by the user
 */
export async function endRecording(selectedPitchIndex: number, selectedEffectIndex: number) {
    console.log("Delaying record end")
    await sleep(getValue("settings.recordEndDelay"));
    const status = await getStatus();
    if(status.state === 'recording') {
        const result = await stopRecording();
        console.log(`Saved to: ${result.filePath}`);

        // Apply FFMpeg Filters
        // Noise suppression, if enabled
        const allowNoiseSuppression = await getValue('settings.allowNoiseSuppression');
        if(allowNoiseSuppression) {
            const noiseSuppressionFile = await resolveResource('binaries/noiseSuppression.rnnn');
            await applyFFMpegFilter(`arnndn=model=${noiseSuppressionFile}:mix=0.8`);
        }

        // Pitch Shift
        await applyFFMpegFilter(`rubberband=pitch=${pitchFilters[selectedPitchIndex]}`);

        // Effect Filters
        await applyFFMpegFilter(effectFilters[selectedEffectIndex]);

        console.log("Setting Statistic")
        const filesRecorded = getValue('statistics.filesRecorded')
        await setValue('statistics.filesRecorded', filesRecorded + 1);
    }
}

/**
 * @description Stops the current recording and discards any data associated with it
 */
export async function cancelRecording() {
    const status = await getStatus();
    if(status.state === 'recording') {
        await stopRecording();
        await remove(outputFile);
        const recordingsCanceled = getValue('statistics.recordingsCancelled');
        await setValue("statistics.recordingsCancelled", recordingsCanceled + 1);
    }
}

/**
 * @description Applies an FFMpeg effect filter (-af)
 * @param effect list of effects you want to apply (Write it as if you were directly interacting with FFMpeg in the CLI)
 */
export async function applyFFMpegFilter(effect: string) {
    const tempFile = `${outputFile}.wav`;
    await Command.sidecar('binaries/ffmpeg', [
        '-i',
        outputFile,
        '-af',
        effect,
        tempFile
    ]).execute();
    await rename(tempFile, outputFile);
}
