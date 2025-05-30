import { fetch } from "@tauri-apps/plugin-http";

// This method is most likely going to only be used by one or two functions of the app, and mainly intended for fetching the release notes from the GitHub repo
export async function JsonNetDataFromTag(url: string, tag: string): Promise<string | null> {
    return new Promise((resolve) => {
        fetch(url, {
            method: 'GET',
        })
        .then((res) => res.json())
        .then((json => {
            resolve(json[tag] as string ?? null)
        })).catch(err => {
            console.error(`Error: ${err}. No data retreived`)
            resolve(null)
        });
    })
}