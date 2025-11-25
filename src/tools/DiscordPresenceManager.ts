// Writing here because I forget this a LOT: DETAILS is the first line of small text, STATE is the second. No clue why it trips me up this bad
import { setActivity, start } from "tauri-plugin-drpc";
import { Activity, ActivityType, Assets, Button, Timestamps } from "tauri-plugin-drpc/activity";
import { formatVersion } from "./OsTools";
import {getValue} from "./SettingsManager";

const clientId = "1325340097234866297";
let currentActivity: Activity;

export async function startRichPresence() {
    if((await getValue("enableRichPresence"))) {
        await start(clientId);
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

export async function setPresenceDetails(newDetails: string) {
    // Edge case for when rpc isn't loaded either because discord isn't open, an error failed to start the rpc, or if the user disabled the rpc in settings
    if(!currentActivity) return;
    currentActivity.setDetails(newDetails);
}

export async function setPresenceState(newState: string) {
    if(!currentActivity) return;
    currentActivity.setState(newState);
}
