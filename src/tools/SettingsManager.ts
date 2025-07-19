import { resolveResource } from "@tauri-apps/api/path";
import { readTextFile, writeTextFile } from "@tauri-apps/plugin-fs";

let settingsJson: any;

export async function loadSettings() {
    settingsJson = JSON.parse(await readTextFile(await resolveResource("resources/settings.json")));
    console.log("Settings JSON loaded");
    console.log(settingsJson);
}

export function getValue(key: string): Promise<number | string | boolean> { 
    return settingsJson[key] ?? console.error(`key ${key} is not a valid key in the settings json`);
}

export function setValue(key: string, value: number | string | boolean) {
    settingsJson[key] = value;
    debounce(async() => await saveJsonData(), 100);
}

async function saveJsonData() {
    const content = JSON.stringify(settingsJson);
    await writeTextFile(await resolveResource("resources/settings.json"), content);
}

// https://gist.github.com/ca0v/73a31f57b397606c9813472f7493a940
export function debounce<T extends Function>(cb: T, wait = 20) {
    let h = 0;
    let callable = (...args: any) => {
        clearTimeout(h);
        h = setTimeout(() => cb(...args), wait);
    };
    return <T>(<any>callable);
}
