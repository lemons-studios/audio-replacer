import { invoke } from '@tauri-apps/api/core';
import * as path from '@tauri-apps/api/path';
import { mkdir, exists, remove, copyFile, rename, readDir } from '@tauri-apps/plugin-fs';
import { convertFileFormat } from "./FFMpegManager";
import { getValue } from '../tools/SettingsManager';

export let currentFile: string;
export let truncatedCurrentFile: string;
export let currentOutFile: string;
export let currentFileName: string;
export let currentFileLocalPath: string;

export let outputFolderPath: string;
export let projectPath: string;

export let completionPercentage: number;
export let filesRemaining: number;

// Extra folders
export let applicationData: string;
export let outputFolder: string;

const supportedFileTypes = [".mp3", ".wav", ".ogg", ".flac", ".m4a"];

export let projectFiles: string[] = [];
export let isProjectLoaded: boolean = false;
export let extraEditsFlagged = false;

export async function setProjectData(dataPath: string) {
    // mark as unloaded each time project data is loaded
    isProjectLoaded = false;
    projectPath = dataPath;
    
    const appFolder = await invoke('get_install_directory') as string;
    outputFolder = await path.join(appFolder, "output");
    if(!await exists(outputFolder)) {
        await mkdir(outputFolder);
    }

    let projectName = await path.basename(projectPath);
    projectFiles = (await getAllFiles()).filter(p => isAudioFile(p));
    outputFolderPath = await path.join(outputFolder, projectName)
    if (!await exists(outputFolderPath)) {
        await mkdir(outputFolderPath);
    }

    await createInitialData();
    await setCurrentFile();
    isProjectLoaded = true;
    
}

async function createInitialData() {
    const inputDirs = await readDir(projectPath);
    const outputDirs = await readDir(outputFolderPath);
    const outputSet = new Set(outputDirs);

    const foldersToCreate = inputDirs.filter(item => !outputSet.has(item));
    if(foldersToCreate.length === 0) {
        return;
    }

    for(const dirName of foldersToCreate) {
        const dir = await path.join(outputFolderPath, dirName.name);
        // If by some miracle the folder appears after folderToCreate is created
        if(await exists(dir)) continue;
        await mkdir(dir);
    }
}

async function createProjectdata() {
    const undesiredFiles = projectFiles.filter(p => isUndesirableAudioFile(p));
    if(undesiredFiles.length != 0) {
        for(const f of undesiredFiles) {
            await convertFileFormat(f, "wav");
        }
    }
}

async function setCurrentFile() {
    await getNextFile();
    if(currentFile == "") {
        // This would imply that the project is completed
        currentFile = "YOU ARE DONE";
        currentFileLocalPath = "YOU ARE DONE";
        currentOutFile = "YOU ARE DONE";
        truncatedCurrentFile = truncateDirectory(currentFile, 2);
        filesRemaining = 0;
        completionPercentage = 100.00;
        return;
    }

    completionPercentage = await calculateCompletion();
    filesRemaining = await countFiles();

    truncatedCurrentFile = truncateDirectory(currentFile, 2);
    currentFileName = currentFile.substring(currentFile.lastIndexOf('/') + 1);

    currentFileLocalPath = currentFile.split(projectPath)[1];
    currentOutFile = await path.join(outputFolderPath, currentFileLocalPath);
}

async function getNextFile() {
    const randomizationEnabled: boolean = (await getValue("randomizationEnabled") as unknown as number) == 1;
    if(isProjectLoaded) {
        const index = 0;
        const deleteCount = 1;
        projectFiles.splice(index, deleteCount);
    }
    currentFile = projectFiles[0];
}

function truncateDirectory(path: string, dirLevels: number) {
    const delimiter = "/";
    if(!path || dirLevels <= 0) {
        return path;
    }

    const splitDir = path.split(delimiter);
    const truncatedSegments = splitDir.slice(-dirLevels);

    // Slice on return removes the leading /, which isn't needed 
    return truncatedSegments.join(delimiter).slice(1);
}

export async function submitFile() {
    if(currentFile) {
        await remove(currentFile);
    }

    if(extraEditsFlagged && currentOutFile) {
        
        const directory = await path.dirname(currentOutFile);
        const baseName = await path.basename(currentOutFile)
        const file = `extra-edits-required-${baseName}`
        const joinedPath = await path.join(directory, file);

        await rename(currentOutFile, joinedPath);
    }

    // I won't need to use this anywhere else so It's not needed as a separate function
    await invoke("delete_empty_subdirectories", {
        projectPath: projectPath
    })

    await setCurrentFile();
}

export async function rejectFile() {
    await remove(currentOutFile);
}

export async function skipFile() {
    await copyFile(currentFile, currentOutFile);
    await remove(currentFile);
    await setCurrentFile();
}

async function getAllFiles(): Promise<string[]> {
    return new Promise((resolve) => {
        invoke("get_all_files", {
            path: projectPath
        }).then((res) => {
            resolve(res as string[]);
        })
    })
}

async function getSubdirectories(folder: string): Promise<string[]> {
    return new Promise((resolve) => {
        invoke("get_subdirectories", {
            path: folder
        }).then((res) => {
            resolve(res as string[]);
        })
    })
}

export async function calculateCompletion(): Promise<number> {
    return new Promise((resolve) => {
        invoke("calculate_completion", {
            inputPath: projectPath,
            outputPath: outputFolderPath
        }).then((res) => {
            resolve(res as number);
        })
    })
}

export async function countFiles(): Promise<number> {
    return new Promise((resolve) => {
        invoke("count_files", {
            path: projectPath
        }).then((res) => {
            resolve(res as number);
        })
    })
}

export function isAudioFile(path: string): boolean {
    return supportedFileTypes.some(ext => path.toLowerCase().endsWith(ext.toLowerCase()));
}

// Audio replacer works the best with .wav files. 
export function isUndesirableAudioFile(path: string): boolean {
    const undesirableFileTypes = [".mp3", ".ogg", ".flac", ".m4a"];
    return undesirableFileTypes.some((ext) => path.toLowerCase().endsWith(ext.toLowerCase()));
}

export function normalizePath(p: string): string {
  return p.replace(/[/\\]+$/, '');
}
