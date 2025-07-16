import { basename, dirname, extname, join } from "@tauri-apps/api/path";
import { open } from "@tauri-apps/plugin-dialog";
import { platform } from "@tauri-apps/plugin-os"; 

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

// Might be needed in the future
export function isWindows() {
    const os = platform();
    return os === "windows";
}
