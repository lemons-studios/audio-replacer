import { resolveResource } from "@tauri-apps/api/path";
import {exists, readTextFile, writeTextFile} from "@tauri-apps/plugin-fs";

let dataContents: any;
let filePath: string;

const defaultSettings = {
    settings: {
        autoAcceptRecordings: false,
        updateCheck: true,
        recentProjectPaths: [],
        recordStartDelay: 250,
        recordEndDelay: 250,
        enableTranscription: true,
        allowNoiseSuppression: true,
        sortingMethod: "alphabetical"
    },
    statistics: {
        appOpenTime: 0,
        filesRecorded: 0,
        filesAccepted: 0,
        filesRejected: 0,
        filesSkipped: 0,
        recordingsCancelled: 0
    }
} as const

type ValidData = 'settings.autoAcceptRecordings' | 'settings.updateCheck' | 'settings.recentProjectPaths' | 'settings.recordStartDelay' 
                | 'settings.recordEndDelay'  | 'settings.enableTranscription' | 'settings.enableRichPresence' | 'settings.allowNoiseSuppression'
                | 'settings.sortingMethod' | 'statistics.appOpenTime'  | 'statistics.filesRecorded' | 'statistics.filesAccepted'
                | 'statistics.filesRejected' | 'statistics.filesSkipped'| 'statistics.recordingsCancelled';

export async function initializeData() {
    filePath = await resolveResource("resources/data.json");
    if(await exists(filePath)) {
        // No need to run the code below if the file exists
        const fileContents = await readTextFile(filePath);
        dataContents = JSON.parse(fileContents);
    }
    else {
        // In case a user has the genius idea of deleting the settings file
        const newData = JSON.stringify(defaultSettings);
        dataContents = defaultSettings
        await writeTextFile(filePath, newData);
    }
}

export async function getValue(name: ValidData) {
    if(!dataContents) await initializeData();

    const split = name.split(".");
    return dataContents[split[0]][split[1]];
}

export async function setValue(name: ValidData, value: number | string | boolean | Array<any>) {
    if(!dataContents) await initializeData();

    const split = name.split(".");
    dataContents[split[0]][split[1]] = value;
    await writeContent();
}

export async function resetStatistics() {
    dataContents.statistics = defaultSettings.statistics;
    await writeContent()
}

export async function resetSettings() {
    dataContents.settings = defaultSettings.settings;
    await writeContent();
}

export async function resetAll() {
    dataContents = defaultSettings;
    await writeContent();
}

async function writeContent() {
    const data = JSON.stringify(dataContents);
    await writeTextFile(filePath, data);
}
