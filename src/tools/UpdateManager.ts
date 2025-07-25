import { check } from "@tauri-apps/plugin-updater";

export const updateAvailable = await check({
    timeout: 30000,
    headers: {
        
    }
})