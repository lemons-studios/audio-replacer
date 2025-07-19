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
        console.log("Settings JSON Loaded");
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

export function setValue(key: string, value: number | string | boolean) {
    settingsJson[key] = value;
    debounce(async() => await saveJsonData(), 30);
}

async function saveJsonData() {
    const content = JSON.stringify(settingsJson);
    await writeTextFile(await resolveResource("resources/settings.json"), content);
}
