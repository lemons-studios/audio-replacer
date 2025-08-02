import { resolveResource } from "@tauri-apps/api/path";
import { readTextFile, writeTextFile } from "@tauri-apps/plugin-fs";
import { debounce } from "./OsTools";

let settingsJson: any;
let loaded: boolean = false;

async function loadSettings() {
    try {
        console.log("Loading Settings JSON");
        const path = await resolveResource("resources/settings.json");
        const contents = await readTextFile(path);
        settingsJson = JSON.parse(contents);
        
        console.log("Settings JSON (Re)loaded");
    } catch(e: any) {
        console.error(`Settings Load Failed: ${e}`);
    }
}

export async function getValue(key: string): Promise<any> {
    if(!loaded) {
        await loadSettings();
        loaded = true;
    }
    try {
        console.log(`Attempting to return value of key ${key}`);
        const value = settingsJson[key];
        console.log(`Value of ${key}: ${value}`);
        return value;
    } catch(e: any) {
        console.error(`Error while trying to get key ${key}: ${e}`)
    }
}

export function setValue(key: string, value: any) {
    console.log(`Updating ${key} in settings json to ${value}`);
    settingsJson[key] = value;
    console.log(`Attempting to debounce and write`);
    console.log(settingsJson);
    debounce(async() => await saveJsonData(), 500);
}

async function saveJsonData() {
    console.log("Saving settings data to json")
    const content = JSON.stringify(settingsJson);
    console.log(`Content: ${content}`);
    const file = await resolveResource("resources/settings.json");
    console.log(`Saving to ${file}`)

    await writeTextFile(file, content);
    await loadSettings(); // Reload settings json
}
