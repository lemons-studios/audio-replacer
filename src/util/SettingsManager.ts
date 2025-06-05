import { load, Store } from "@tauri-apps/plugin-store";

let store: Store;

export async function loadStore() {
    store = await load('settings.json', {autoSave: true});
}

export async function getValue(keyName: string) {
    await store.get<{value: any}>(keyName);
}

export async function setValue(keyName: string, value: string | number | boolean) {
    await store.set(keyName, {value: value})
}
