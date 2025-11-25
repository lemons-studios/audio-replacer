import {getVersion} from "@tauri-apps/api/app";
import {basename, dirname, extname, join} from "@tauri-apps/api/path";
import {open, save} from "@tauri-apps/plugin-dialog";
import {platform} from "@tauri-apps/plugin-os";

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

export async function changeFileExtension(input: string, newExtension: string) {
    const dir = await dirname(input);
    const currentExt = await extname(input);
    const fileName = await basename(input, currentExt);
    const newFile = `${fileName}${newExtension}`;

    return join(dir, newFile);
}

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

// Might be needed in the future
export function isWindows() {
    const os = platform();
    return os === "windows";
}

// https://gist.github.com/ca0v/73a31f57b397606c9813472f7493a940
export function debounce<T extends Function>(cb: T, wait = 20) {
    let h = 0;
    let callable = (...args: any) => {
        clearTimeout(h);
        h = setTimeout(() => cb(...args), wait);
    };
    return <T>(<any>callable);
}

export function timestampToLegible(timestamp: number): string {
    return new Date(timestamp).toLocaleString();
}

 export async function formatVersion(): Promise<string> {
    const [major, minor, patch] = (await getVersion()).split(".");
    return `${major}.${minor}${patch == "0" ? '' : `.${patch}`}`;
}
