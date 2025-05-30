import { invoke } from "@tauri-apps/api/core";
import { arch } from "@tauri-apps/plugin-os";


export async function getUsername() : Promise<string> {
    return new Promise((resolve) => {
        invoke("get_username").then((res) => {
            resolve(res as string);
        })
    })
}

export function getSystemTime() {
    const date = new Date();
    const hours = date.getHours();
    return hours >= 5 && hours < 12 ? "Morning" : hours >= 12 && hours < 18 ? "Afternoon" : "Evening";
}

export async function getAccentColor(): Promise<string> {
    return new Promise((resolve) => {
        invoke<string>("get_system_color").then((res) => {
            resolve(res as string);
        });
    })
}

export function getPathSeparator() {
    const platform = navigator.userAgent;
    if(platform.includes("Windows")) {
        return "\\";
    }
    return "/";
}