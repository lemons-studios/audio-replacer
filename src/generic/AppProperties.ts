import * as path from '@tauri-apps/api/path';

export let appData: string;
export let extraApplicationData: string;
export let outputDirectory: string;
export let whisperLocation: string;

export let configFolder: string;
export let pitchDataConf: string;
export let effectsDataConf: string;

export let pitchData: string[][];
export let effectData: string[][];

export let pitchTitles: string[];
export let pitchValues: string[];
export let effectTitles: string[];
export let effectValues: string[];

export let isWhisperInstalled: boolean;


export async function initializeValues() {
    appData = await path.appDataDir();
    extraApplicationData = await path.join(appData, "audio-replacer-data");
    outputDirectory = await path.join(extraApplicationData, "out");

    configFolder = await path.join(extraApplicationData, "config");
    pitchDataConf = await path.join(configFolder, "pitchData.json");
    effectsDataConf = await path.join(configFolder, "effectsData.json");    
}
