// Writing here because I forget this a LOT: DETAILS is the first line of small text, STATE is the second. No clue why it trips me up this bad
import { setActivity, start, stop } from "tauri-plugin-drpc";
import { Activity, ActivityType, Assets, Button, Timestamps } from "tauri-plugin-drpc/activity";
import { formatVersion } from "./OsTools";
import {getValue} from "./DataInterface";

let currentActivity: Activity;

/**
 * @description Activates the connection to the Discord Rich Presence server, if the user has it enabled in their settings
 */
export async function startRichPresence() {
    if((await getValue('settings.enableRichPresence')) && !currentActivity) {
        await start("1325340097234866297");
        const version = await formatVersion();

        currentActivity = new Activity()
            .setActivity(ActivityType.Listening)
            .setDetails("Home Page")
            .setTimestamps(new Timestamps(Date.now()))
            .setAssets(new Assets()
                .setLargeImage('appicon')
                .setLargeText(`Version ${version}`))
            .setButton([ new Button("View Project", "https://github.com/lemons-studios/audio-replacer") ]);

        await setActivity(currentActivity);
    }
}


/**
 * @description Sets the details (First line of small text) of the rich presence
 */
export async function setPresenceDetails(newDetails: string) {
    // Edge case for when rpc isn't loaded either because discord isn't open, an error failed to start the rpc, or if the user disabled the rpc in settings
    if(!currentActivity) return;
    currentActivity.setDetails(newDetails);
}

/**
 * @description Sets the state (Second line of small text) of the rich presence
 */
export async function setPresenceState(newState: string) {
    if(!currentActivity) return;
    currentActivity.setState(newState);
}

export async function clearRichPresence() {
    await stop();
}
