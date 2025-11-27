import { resolveResource } from "@tauri-apps/api/path";
import { readTextFile, writeTextFile } from '@tauri-apps/plugin-fs'
import { getValue } from "./SettingsManager";

export let stats: any; // It's an object

export async function loadStats() {
    const filePath = await resolveResource("resources/stats.json");
    stats = JSON.parse(await readTextFile(filePath));

}

/**
 *
 * @description Update value of a statistic and immediately save the file
 * @param stat name of statistic
 * @param newValue new value of statistic
 */
export async function updateStatistic(stat: string, newValue: any) {
    if(!(await getValue("trackStats"))) return;
    if(!stats) await loadStats();
    stats[stat] = newValue;
    await writeStats();
}

export async function writeStats() {
    const str = JSON.stringify(stats);
    const filePath = await resolveResource("resources/stats.json");
    await writeTextFile(filePath, str);
}
