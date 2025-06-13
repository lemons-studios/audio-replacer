import { getVersion } from "@tauri-apps/api/app";
import { start, stop, setActivity, clearActivity, destroy, spawn } from "tauri-plugin-drpc"
import { Assets, Activity, Timestamps } from "tauri-plugin-drpc/activity";

let rpcActivity: Activity;
let rpcAssets: Assets;

export async function startRichPresence() {
    await start("1325340097234866297");
    rpcAssets = new Assets()
        .setLargeImage("appicon")
        .setLargeText(`Version 5.0`)

    rpcActivity = new Activity()
    .setDetails("Home Page")
    .setAssets(rpcAssets)
    .setTimestamps(new Timestamps(Date.now()));

    await setActivity(rpcActivity);
}

export function setDetails(newDetails: string) {
    rpcActivity.setDetails(newDetails);
}

export function setState(newState: string) {
    rpcActivity.setState(newState);
}

export function setLargeImage(key: string) {
    rpcAssets.setLargeImage(key);
    rpcActivity.setAssets(rpcAssets);
}

export function setLargeImageText(text: string) {
    rpcAssets.setLargeText(text);
    rpcActivity.setAssets(rpcAssets);
}

export function setSmallImage(key: string) {
    rpcAssets.setSmallImage(key);
    rpcActivity.setAssets(rpcAssets);
}

export function setSmallImageText(text: string) {
    rpcAssets.setSmallText(text);
    rpcActivity.setAssets(rpcAssets);
}

export async function stopRichPresence() {
    await stop();
}