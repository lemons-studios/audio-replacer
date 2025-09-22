import { resolveResource } from "@tauri-apps/api/path";
import { readTextFile, writeTextFile } from "@tauri-apps/plugin-fs";
import { debounce } from "./OsTools";

let settingsJson: any;
let loaded: boolean = false;

async function loadSettings() {
    try {
        const path = await resolveResource("resources/settings.json");
        const contents = await readTextFile(path);
        settingsJson = JSON.parse(contents);
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
        const value = settingsJson[key];
        return value;
    } catch(e: any) {
        console.error(`Error while trying to get key ${key}: ${e}`)
    }
}

export function setValue(key: string, value: any) {
    settingsJson[key] = value;
    debounce(async() => await saveJsonData(), 500);
}

async function saveJsonData() {
    const content = JSON.stringify(settingsJson);
    const file = await resolveResource("resources/settings.json");

    await writeTextFile(file, content);
    await loadSettings(); // Reload settings json
}
