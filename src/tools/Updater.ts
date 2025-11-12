import { platform } from "@tauri-apps/plugin-os";
import { relaunch } from "@tauri-apps/plugin-process";
import { check, Update } from "@tauri-apps/plugin-updater";

export let update: (Update | null | undefined) = undefined;

export async function isUpdateAvailable(): Promise<boolean> {
    if(update === undefined) {
        update = await check();
    }
    return update != null;
}

export async function downloadUpdates() {
    if(update) {
        if(platform() !== 'windows') {
            const releases = "https://github.com/lemons-studios/audio-replacer/releases/latest";
            open(releases);
            return;
        }

        await update.downloadAndInstall(async(e) => {
            switch(e.event) {
                case 'Finished': 
                    await relaunch();
                    break;
            }
        });
    }
}

export function getUpdateVersion(): string {
    return update?.version as string
}
