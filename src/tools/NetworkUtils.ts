import { fetch } from "@tauri-apps/plugin-http";
import { download } from "@tauri-apps/plugin-upload";

// This method is most likely going to only be used by one or two functions of the app, and mainly intended for fetching the release notes from the GitHub repo
export async function jsonNetDataFromTag(url: string, tag: string): Promise<string | null> {
  const res = await fetch(url);
  try {
    const json = await res.json();
    const val = json[tag];
    return typeof val === "string" ? val : null;
  } 
  catch (err) {
    console.error(`Error: ${err}. No data retrieved`);
    return null;
  }
}

export function downloadFile(url: string, outputPath: string) {
  download(url, outputPath);
}
