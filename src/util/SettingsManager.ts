import { resolveResource } from "@tauri-apps/api/path";
import { load, Store } from "@tauri-apps/plugin-store";

let store: Store;

export async function loadStore() {
    const json = await resolveResource('resources/settings.json');
    store = await load(json, {autoSave: true});
}

export async function getValue(keyName: string) {
    const key = await store.get<{value: any}>(keyName);
    return key;
}

export async function setValue(keyName: string, value: string | number | boolean) {
    await store.set(keyName, {value: value})
}
