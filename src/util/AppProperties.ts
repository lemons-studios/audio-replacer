import * as path from '@tauri-apps/api/path';

const appData: string = await path.appDataDir();
export let extraData: string = await path.join(appData, "audio-replacer-data");
export let outputDirectory: string = await path.join(extraData, "output");
export let whipserLocation: string = await path.join(extraData, "whisper.bin");