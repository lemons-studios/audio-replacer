import { fetch } from "@tauri-apps/plugin-http";

export class NetworkUtils {
    public async JsonNetDataFromTag(url: string, tag: string) {
            const response = await fetch(url, {
            method: 'GET',
        });
    }
}
