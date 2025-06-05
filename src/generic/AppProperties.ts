import * as path from '@tauri-apps/api/path';
import { resolve } from '@tauri-apps/api/path';

export let appData: string;
export let outputDirectory: string;

export let pitchData: string[][];
export let effectData: string[][];

export let pitchTitles: string[];
export let pitchValues: string[];
export let effectTitles: string[];
export let effectValues: string[];

export async function initializeValues() {
    appData = await path.appDataDir();
    outputDirectory = await path.join(appData, "audio-replacer-output");
}
