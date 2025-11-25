import { resolveResource } from "@tauri-apps/api/path";
import { readTextFile, writeTextFile } from '@tauri-apps/plugin-fs'

export let stats: any; // It's an object

export async function loadStats() {
    const filePath = await resolveResource("resources/stats.json");
    stats = JSON.parse(await readTextFile(filePath));

}

export async function writeStats() {
    const str = JSON.stringify(stats);
    const filePath = await resolveResource("resources/stats.json");
    await writeTextFile(filePath, str);
}
