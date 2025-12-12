import { resolveResource } from "@tauri-apps/api/path";
import { readTextFile, writeTextFile } from "@tauri-apps/plugin-fs";

let dataContents: any;
let filePath: string;

type ValidData = 'settings.autoAcceptRecordings' | 'settings.updateCheck' | 'settings.recentProjectPaths' | 'settings.recordStartDelay' 
                | 'settings.recordEndDelay'  | 'settings.enableTranscription' | 'settings.enableRichPresence' | 'settings.allowNoiseSuppression'
                | 'settings.randomizeInput'  | 'settings.sortingMethod' | 'statistics.appOpenTime'  | 'statistics.filesRecorded'
                | 'statistics.filesAccepted' | 'statistics.filesRejected' | 'statistics.filesSkipped'| 'statistics.recordingsCancelled';

export async function initializeData() {
    filePath = await resolveResource("resources/data.json");
    const fileContents = await readTextFile(filePath);
    dataContents = JSON.parse(fileContents);
}

export function getValue(name: ValidData) {
    if(!dataContents) {
        console.log("Data is not initialized");
        return;
    }

    const split = name.split(".");
    return dataContents[split[0]][split[1]];
}

export async function setValue(name: ValidData, value: number | string | boolean | Array<any>) {
    if(!dataContents) {
        console.log("Data is not initialized yet");
        return;
    }

    console.log(name)
    const split = name.split(".");
    console.log(split);

    dataContents[split[0]][split[1]] = value;


    const data = JSON.stringify(dataContents);
    await writeTextFile(filePath, data);
}
