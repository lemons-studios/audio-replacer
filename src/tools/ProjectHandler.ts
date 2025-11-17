// This file handles everything related to projects, From moving files around to recording audio for a project
import { invoke } from "@tauri-apps/api/core";
import { basename, dirname, extname, join, resolveResource } from "@tauri-apps/api/path";
import { copyFile, exists, mkdir, readDir, readTextFile, remove } from "@tauri-apps/plugin-fs";
import { populateFFMpegFilters } from "../routes/recordPage/AudioManager";
import { error } from "@tauri-apps/plugin-log";
import { getValue } from "./SettingsManager";

/**
 * @description points to the output folder in the install directory (installDir/output)
 */
let appOutputFolder: string;

let inputFolder: string;
/**
 * @description points to the output folder location of the current project (installDir/output/project) 
 */
let outputFolder: string;

let inputFiles: string[];
let outputFiles: string[];

export let currentFile: string;
export let outputFile: string;

export let fileTranscription: string;
export let localPath: string;

// Initial setup functions
export async function setAdditionalFolders() {
    const installFolder = await invoke('get_install_directory') as string;
    appOutputFolder = await join(installFolder, 'output');
    if(!(await exists(appOutputFolder))) await mkdir(appOutputFolder);
}

/**
 * @description Sets the active project based on user selection
 * @param projectFile Path to the .arproj file that the user wants to open
 */
export async function setActiveProject(projectFile: string) {
    if(appOutputFolder === undefined) await setAdditionalFolders();
    const object = JSON.parse(await readTextFile(projectFile));

    inputFolder = object.path;
    outputFolder = await join(appOutputFolder, object.name);
    if(!(await exists(outputFolder))) await mkdir(outputFolder);

    inputFiles = await getAllFiles(inputFolder);
    outputFiles = await getAllFiles(outputFolder);

    // Replicate folder structure of input folder
    const inputDirs = await readDir(inputFolder);
    const outputDirs = await readDir(outputFolder);

    const uncreatedFolders = inputDirs.filter(x => !new Set(outputDirs).has(x));
    for(let i = 0; i< uncreatedFolders.length; i++) {
        const directory = await join(outputFolder, uncreatedFolders[i].name);
        if(!(await exists(directory))) await mkdir(directory);
    }

    // Also get the audio manager to populate the ffmpeg pitch and effect filters from the arproj file
    populateFFMpegFilters(object);

    // Finally, set some variables for the first file
    getNextFile();
}

async function getNextFile() {
    // Delete any unneeded/empty directories in the input folder (Might remove later)
    await invoke('delete_empty_subdirectories', {
        projectPath: inputFolder
    });
    
    currentFile = inputFiles[0];

    // Get the "Local path" of the current file, then join together with the path to the output folder
    localPath = currentFile.split(inputFolder)[1]; // This gets the path from the input folder down to the file
    outputFile = await join(outputFolder, localPath); // Join the path to the output to the local path string to create a new path to the output file that doesn't exist yet!
    fileTranscription = await transcribeFile();
}

async function getAllFiles(folder: string): Promise<string[]> {
    return new Promise((resolve) => {
        invoke('get_all_files', {
            path: folder
        }).then((res: any) => {
            const filtered = res.filter((p: string) => isAudioFile(p));
            resolve(filtered);
        });
    });
}

const transcribeFile = async() => {
    try {
        const allowTranscription = await getValue("enableTranscription") as boolean;
        if(!allowTranscription) return "Transcription Service Is Disabled";

        const model = await resolveResource('binaries/whisper.bin');
        return new Promise<string>((resolve) => {
            invoke<string>("transcribe_file", {
                path: currentFile,
                modelPath: model
            }).then((res) => {
                resolve(res as string);
            })
        })
    }
    catch(e: any) {
        error(`Error while transcribing file: ${e}`);
        return "An Error Occurred while transcribing this file!"
    }
}

// Finalising recording functions
/**
 * @description Moves the current active file to the output folder or deletes it
 * @param moveToOutput weather or not to move the original file to the output folder or to delete it
 */
export async function skipFile(moveToOutput: boolean = false) {
    inputFiles.splice(0, 1);
    if(moveToOutput) {
        await moveFile(currentFile, outputFile);
        outputFiles.push(currentFile);
    }
    else {
        await remove(currentFile);
    }
}

export async function discardfile() {
    await remove(outputFile);
}

export async function submitFile(requiresExtraEdits: boolean) {
    inputFiles.splice(0, 1);

    const fileName = requiresExtraEdits ? await (async() => {
        const dir = await dirname(outputFile);
        const name = await basename(outputFile);
        const ext = await extname(outputFile);
        
        const res = await join(dir, `${name}-ExtraEditsRequired.${ext}`);
        return res;
    })() : outputFile; 
    await moveFile(currentFile, fileName);
    outputFiles.push(fileName);

    await getNextFile();
}

// Misc functions
/**
 * @returns Percentage for project completion (decimal multiplied by 100)
 */
export function calculateCompletion(): number {
    return (outputFiles.length / (inputFiles.length + outputFiles.length)) * 100;
}

export function countInputFiles() {
    return inputFiles.length;
}

export function countOutputFiles() {
    return outputFiles.length;
}

function isAudioFile(path: string): boolean {
    const types: string[] = ['.wav', 'mp3', '.ogg', '.flac'];
    return types.some(ext => path.toLowerCase().endsWith(ext.toLowerCase()));
}

export async function moveFile(path: string, newPath: string) {
    await copyFile(path, newPath);
    await remove(path);
}
