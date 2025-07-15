import { resolveResource } from "@tauri-apps/api/path";
import { readTextFile } from "@tauri-apps/plugin-fs";

export let pitchDataNames: string[];
export let pitchDataValues: string[];

export let effectDataNames: string[];
export let effectDataValues: string[];

export async function populateCustomData() {
    const customPitchContents = await readTextFile(await resolveResource("resources/pitchData.json")); 
    const effectJson = await readTextFile(await resolveResource("resources/effectData.json"));

    const fullPitchData: string[][] = JSON.parse(customPitchContents);
    const fullEffectData: string[][] = JSON.parse(effectJson);

    // The json array will always be sorted with the value of the custom pitch/effect parameter being the first index (0), while the second index (1) will be the name used by the dropdowns on the recording page
    const valueIndex = 0;
    const nameIndex = 1;

    // Populate data
    pitchDataValues = fullPitchData.map((e) => e[valueIndex].toString());
    effectDataValues = fullEffectData.map((e) => e[valueIndex].toString());

    pitchDataNames = fullPitchData.map((e) => e[nameIndex].toString());
    effectDataNames = fullEffectData.map((e) => e[nameIndex].toString());
}
