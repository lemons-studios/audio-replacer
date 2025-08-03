import { start, stop, setActivity } from "tauri-plugin-drpc";
import { Assets, Activity, Timestamps } from "tauri-plugin-drpc/activity";

let rpcActivity: Activity;
let rpcAssets: Assets;

export async function startRichPresence() {
  try {
    await start("1325340097234866297");
    rpcAssets = new Assets()
      .setLargeImage("appicon")
      .setLargeText(`Version 5.0`);

    rpcActivity = new Activity()
      .setDetails("Home")
      .setAssets(rpcAssets)
      .setTimestamps(new Timestamps(Date.now()));

    await setActivity(rpcActivity);
  } catch (e: any) {
    return;
  }
}

export async function setDetails(newDetails: string) {
  rpcActivity.setDetails(newDetails);
  await setActivity(rpcActivity);
}

export async function setState(newState: string) {
  rpcActivity.setState(newState);
  await setActivity(rpcActivity);
}

export async function setLargeImage(key: string) {
  rpcAssets.setLargeImage(key);
  rpcActivity.setAssets(rpcAssets);
  await setActivity(rpcActivity);
}

export async function setLargeImageText(text: string) {
  rpcAssets.setLargeText(text);
  rpcActivity.setAssets(rpcAssets);
  await setActivity(rpcActivity);
}

export async function setSmallImage(key: string) {
  rpcAssets.setSmallImage(key);
  rpcActivity.setAssets(rpcAssets);
  await setActivity(rpcActivity);
}

export async function setSmallImageText(text: string) {
  rpcAssets.setSmallText(text);
  rpcActivity.setAssets(rpcAssets);
  await setActivity(rpcActivity);
}

export async function stopRichPresence() {
  await stop();
}
