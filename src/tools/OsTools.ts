import { getVersion } from "@tauri-apps/api/app";
import { basename, dirname, extname, join } from "@tauri-apps/api/path";
import {message, open, save} from "@tauri-apps/plugin-dialog";
import {invoke} from "@tauri-apps/api/core";
import {relaunch} from "@tauri-apps/plugin-process";
import {Command} from "@tauri-apps/plugin-shell";

/**
 * @description Open a file selection dialog
 * @param allowedTypes Array of file extensions that are allowed to be picked in the file picker. If left blank, all files types will be allowed. Exclude dot from file format (.json -> json)
 * @param filterName Name that shows up for the file type selector in the bottom right or wherever it is on other operating systems
 * @returns Path to file that the user selected, or null if no file is selected
 */
export async function selectFile(allowedTypes: string[] = [], filterName = "Any file"): Promise<string> {
    return new Promise((resolve) => {
        open({
            multiple: false,
            directory: false,
            filters: [
                {
                    name: filterName,
                    extensions: allowedTypes
                },
            ]
        }).then((res) => {
            resolve(res as string)
        })
    })
}

/**
 * @description Open a folder selection dialog
 * @returns Path to a folder that the user selected, or null if no folder was selected
 */
export async function selectFolder(): Promise<string> {
    return new Promise((resolve) => {
        open({
            multiple: false,
            directory: true
        }).then((res) => {
            resolve(res as string)
        })
    })
}

/**
 * @description changes the file extension of an input file. Does not create a file at the location
 * @param input Path to the input file
 * @param newExtension The extension to change the file to
 * @returns Path to file but with the changed file extension at the end
 */
export async function changeFileExtension(input: string, newExtension: string) {
    const dir = await dirname(input);
    const currentExt = await extname(input);
    const fileName = await basename(input, currentExt);
    const newFile = `${fileName}${newExtension}`;

    return (await join(dir, newFile));
}

/**
 * @description Opens a file save dialog & creates an empty file on a path if one is selected
 * @param allowedTypes Array of file extensions that are allowed to be picked in the file picker. If left blank, all file types will be allowed. Exclude dot from file format (.json -> json)
 * @param filterName Name that shows up for the file type selector in the bottom right or wherever it is on other operating systems
 * @returns Path to the saved file, or null if none was selected
 */
export async function saveFile(allowedTypes: string[] = [], filterName = "Any File") {
    return await save({
        filters: [
            {
                name: filterName,
                extensions: allowedTypes
            }
        ]
    });
}

/**
 * @description Converts Date timestamps to human-legible dates
 * @param timestamp Time since Unix epoch (1/1/1970) in milliseconds
 * @returns Human-legible date based on system locale settings
 */
export function timestampToLegible(timestamp: number): string {
    return new Date(timestamp).toLocaleString();
}

/**
 * @description Formats the package version for displays
 */
export async function formatVersion(): Promise<string> {
    const [major, minor, patch] = (await getVersion()).split(".");
    return `${major}.${minor}${patch == "0" ? '' : `.${patch}`}`;
}

/**
 * @description wait x milliseconds to continue executing code
 * @param ms time to wait (in milliseconds)
 */
export async function sleep(ms: number) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

/**
 * @description Converts programming variable names to human-readable names. Works with camelCase, PascalCase, and snake_case
 * @param str string to be normalized
 * @param capitalizeAll Make all separated words capital or not
 * @returns human-readable string
 */
export function normalizePropertyName(str: string, capitalizeAll: boolean = true) {
    let res = "";
    const split = str.split("");
    const isWordSplit = ((c: string) => {
        // This checks for snake_case, camelCase, and PascalCase
        return c === c.toUpperCase() || c === "_" ;
    });

    // Manually add upper case character to first letter in string 
    res += split[0].toUpperCase();

    for(let i = 1; i < str.length; i++) {
        if(!capitalizeAll && isWordSplit(str[i])) {
            res += str[i].toLowerCase();
        }
        else res += str[i];

        if(i !== str.length - 1) {
            if(isWordSplit(str[i + 1])) str += " ";
        }
    }
    return res;
}

export async function getMic() {
    if(localStorage.getItem('mic_granted') !== 'true') {
        try {
            await message("In order to function properly, you will need to allow this app to access your microphone");
            await navigator.mediaDevices.getUserMedia({audio: true});
            localStorage.setItem("mic_granted", 'true');
        }
        catch(e: any) {
            await message('Audio Replacer requires access to the microphone to function. App will now close', { title: "Mic Required", kind: 'error' });
            await invoke("close_app");
        }
    }
    else console.log("Mic permission given")
}

export async function attemptRelaunch() {
    const isDev = await invoke("in_dev_env");
    if(isDev) {
        await message("You are running a developer build of this app. This app will not restart.", {
            title: "Super Secret Developer Popup",
            kind: 'info'
        })
    }
    else await relaunch();
}

export async function validateFilter(filterList: string): Promise<boolean> {
    // Pretty much is an effect application BUT on a null audio source. if the effect is valid, the process should exit with code 0
    const flags = ['-f', 'lavfi', '-i', 'anullsrc', '-af', filterList, '-f', 'null', '-t', '0.01', '-'];
    const command = Command.sidecar('binaries/ffmpeg', flags);

    const result = await command.execute();
    return result.code === 0;
}
