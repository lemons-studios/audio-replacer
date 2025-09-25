import { resolveResource } from "@tauri-apps/api/path";
import { readTextFile, writeTextFile } from "@tauri-apps/plugin-fs";
import { info, error } from "@tauri-apps/plugin-log";

let settingsJson: any;
let loaded: boolean = false;

async function loadSettings() {
    try {
        const path = await resolveResource("resources/settings.json");
        const contents = await readTextFile(path);
        settingsJson = JSON.parse(contents);
        info("Settings load successful");
    } 
    catch(e: any) {
        error(`Settings Load Failed: ${e}`);
    }
}

export async function getValue(key: string): Promise<any> {
    if(!loaded) {
        await loadSettings();
        loaded = true;
    }
    try {
        info(`Attempting to get key ${key}`)
        const value = settingsJson[key];
        return value;
    } catch(e: any) {
        error(`Error while trying to get key ${key}: ${e}`)
    }
}

export function setValue(key: string, value: any) {
    info(`Setting value of key ${key} to ${value}`);
    settingsJson[key] = value;
    saveJsonData(); // No need to await this function to be complete, since it's just saving the new settings to a json file
}

async function saveJsonData() {
    try {
        info("Saving Json Data");
        const content = JSON.stringify(settingsJson);
        const file = await resolveResource("resources/settings.json");
        info(`writing ${content} to ${file}`);

        await writeTextFile(file, content);
        info("Json Data Save Successful.");
        await loadSettings(); // Reload settings json
    }
    catch(e: any) {
        error(`Error whilst saving settings data: ${e}`)
    }
}
