import { getVersion } from "@tauri-apps/api/app";
import { basename, dirname, extname, join } from "@tauri-apps/api/path";
import { open, save } from "@tauri-apps/plugin-dialog";


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
