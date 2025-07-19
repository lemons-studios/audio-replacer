import { resolveResource } from "@tauri-apps/api/path";
import { load, Store } from "@tauri-apps/plugin-store";

let store: Store;

export async function loadStore() {
    console.log("Loading Settings Data");
    const json = await resolveResource('resources/settings.json');
    store = await load(json, {autoSave: true});
    console.log("Store Loaded");
}

export async function getValue(keyName: string) {
    const key = await store.get<{value: any}>(keyName);
    return key;
}

export async function setValue(keyName: string, value: any) {
    await store.set(keyName, value)
}
