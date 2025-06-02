import { invoke } from '@tauri-apps/api/core';
import * as path from '@tauri-apps/api/path';
import { mkdir, exists } from '@tauri-apps/plugin-fs';

export let currentFile: string;
export let truncatedCurrentFile: string;
export let currentOutFile: string;
export let currentFileName: string;
export let currentFileLocalPath: string;

export let outputFolderPath: string;
export let projectPath: string;

// Extra folders
export let applicationData: string;
export let outputFolder: string;

const supportedFileTypes = [".mp3", ".wav", ".ogg", ".flac", ".m4a"];

export let projectFiles: string[] = [];
export let isProjectLoaded: boolean = false;
export let extraEditsFlagged = false;

const rng = Math;

export async function setProjectData(dataPath: string) {
    applicationData = await path.appDataDir();
    outputFolder = await path.join(applicationData, "output");
    projectPath = dataPath;

    let projectName = await path.basename(projectPath);
    projectFiles = (await getAllFiles()).filter(p => {
        const regex = p.match(/\.([a-zA-Z0-9]+)$/);
        return regex ? supportedFileTypes.includes(regex[1].toLowerCase()) : false;
    });

    outputFolderPath = await path.join(outputFolder, projectName)
    if (await exists(outputFolderPath)) {
        await mkdir(outputFolderPath);
    }
}

async function CreateInitialData() {

}

export async function getAllFiles(): Promise<string[]> {
    return new Promise((resolve) => {
        invoke("get_all_files", {
            path: projectPath,
            sort: true
        }).then((res) => {
            resolve(res as string[]);
        })
    })
}

export async function getSubdirectories(): Promise<string[]> {
    return new Promise((resolve) => {
        invoke("get_subdirectories", {
            path: projectPath
        }).then((res) => {
            resolve(res as string[]);
        })
    })
}

export function isAudioFile(path: string): boolean {
    return supportedFileTypes.some(ext => path.toLowerCase().endsWith(ext.toLowerCase()));
}


// Audio replacer works the best with .wav files. 
export function isUndesirableAudioFile(path: string): boolean {
    const undesirableFileTypes = [".mp3", ".wav", ".ogg", ".flac", ".m4a"];
    return undesirableFileTypes.some((ext) => path.toLowerCase().endsWith(ext.toLowerCase()));
}