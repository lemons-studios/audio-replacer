import { resolveResource } from "@tauri-apps/api/path";
import { readTextFile, writeTextFile } from "@tauri-apps/plugin-fs";
import { info, error } from "@tauri-apps/plugin-log";

let settingsJson: any;
let loaded: boolean = false;

let settingsPath: string = "";

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
    loaded = true;
}

export async function getValue(key: string): Promise<any> {
    if(!loaded) {
        await loadSettings();
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
    if(!loaded) {
        await loadSettings();
    }
    try {
        info("Saving Json Data");
        const content = JSON.stringify(settingsJson);
        info(`writing ${content} to ${settingsPath}`);

        await writeTextFile(settingsPath, content);
        info("Json Data Save Successful.");
        await loadSettings(); // Reload settings json
    }
    catch(e: any) {
        error(`Error whilst saving settings data: ${e}`)
    }
}
