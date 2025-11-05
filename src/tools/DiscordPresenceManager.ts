import { setActivity, start, stop } from "tauri-plugin-drpc";
import { Activity, ActivityType, Assets, Timestamps } from "tauri-plugin-drpc/activity";

let activity: Activity = new Activity();
let assets: Assets;

export async function startRichPresence(clientId: string) {
    await start(clientId);
    assets = new Assets().setLargeImage('appicon');
    // Create initial Activity
    activity.setActivity(ActivityType.Playing)
    .setState("Test1")
    .setDetails("Test2")
    .setTimestamps(new Timestamps(Date.now()))
    .setAssets(assets);

    await setActivity(activity);
}

export async function setLargeImg(key: string) {
    assets.setLargeImage(key);
    activity.setAssets(assets);
    await setActivity(activity);
}

export async function setSmallImg(key: string) {
    assets.setSmallImage(key);
    activity.setAssets(assets);
    await setActivity(activity);
}

export async function stopRichPresence() {
    await stop();
}
