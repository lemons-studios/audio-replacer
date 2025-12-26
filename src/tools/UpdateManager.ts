import { check, Update } from "@tauri-apps/plugin-updater";
import { ask, message } from "@tauri-apps/plugin-dialog";
import { attemptRelaunch, formatVersion } from "./OsTools";

export async function checkForUpdates() {
    const update: Update | null = await check();
    if(update) {
        const confirmation = await ask(`Version Update Found\nCurrent: ${await formatVersion()}\nLatest: ${update.version}`, {
            title: 'Update Found',
            kind: 'info',
        });
        if(confirmation) {
            await update.downloadAndInstall();
            const restart = await ask('Update downloaded. Restart now?', {
                title: 'Update Complete',
                kind: 'info',
            });

            if(restart) await attemptRelaunch();
            else await message('App will update on close.', { kind: 'info' });
        }
    }
}
