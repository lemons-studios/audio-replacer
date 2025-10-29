import { goto } from '$app/navigation';
import { invoke } from '@tauri-apps/api/core';
import * as path from '@tauri-apps/api/path';
import { ask } from '@tauri-apps/plugin-dialog';
import { mkdir, exists, remove, copyFile, rename, readDir, readTextFile } from '@tauri-apps/plugin-fs';
import { info, error } from '@tauri-apps/plugin-log';

// TODO: Implement project file modification in this file

// currentProject contains project name, project path, files remaining as a JSON 
export let currentProject: any;
export let currentProjectFile: string;

export let outputFolderPath: string;
export let filtersPath: string;

export let currentFile: string;
export let truncatedCurrentFile: string;
export let currentOutFile: string;
export let currentFileName: string;
export let currentFileLocalPath: string;


export let completionPercentage: number;
export let outputFolder: string;

export let projectFiles: string[];
export let isProjectLoaded: boolean = false;
export let extraEditsFlagged = false;

// Runs on app startup from +layout.svelte
export async function setAdditionalFolderLocs() {
    const installFolder = await invoke('get_install_directory') as string;

    if(outputFolder === undefined) {
        outputFolder = await path.join(installFolder, "output");

        // In the case that this is the user's first time running Audio Replacer
        if(!await exists(outputFolder)) {
            await mkdir(outputFolder);
        }
    }
    if(filtersPath === undefined) {
        filtersPath = await path.join(installFolder, "filters");
        if(!await exists(filtersPath)) {
            await mkdir(filtersPath);
        }
    }
}

export async function setProjectData(projectFile: any) {
    // mark as unloaded each time project data is loaded
    isProjectLoaded = false;

    // The JSON array used in the home page is purely for the Home Page's uses, as ProjectManager will also be writing to the project files
    const project = JSON.parse(await readTextFile(projectFile));
    info(`Attempting to set project data to ${project.path}`);
    currentProject = project;

    try {
        outputFolderPath = await path.join(outputFolder, project.name);
        if(!await exists(outputFolderPath)) {
            await mkdir(outputFolderPath);
        }
        projectFiles = (await getAllFiles()).filter(p => isAudioFile(p));
        
        await createInitialData();
        await setCurrentFile();
        isProjectLoaded = true;

        /* info("Project Successfully Loaded!");
        info(`Current File: ${currentFile}`);
        info(`Files Remaining: ${filesRemaining}`);
        info(`Output Path: ${outputFolderPath}`);
        */
    }
    catch(e: any) {
        error(`Error: Project initialization failed with error: ${e}`);
    }
}

async function createInitialData() {
    try {
        const inputDirs = await readDir(currentProject.path);
        const outputDirs = await readDir(outputFolderPath);
        const outputSet = new Set(outputDirs);

        const foldersToCreate = inputDirs.filter(item => !outputSet.has(item));
        if(foldersToCreate.length === 0) return;
        
        for(const dirName of foldersToCreate) {
            const dir = await path.join(outputFolderPath, dirName.name);
            // If by some miracle the folder appears after folderToCreate is created
            if(await exists(dir)) continue;
            await mkdir(dir);
        }
    }
    catch(e: any) {
        error(`createInitialData has failed with error ${e}`);
    }

}

async function setCurrentFile() {
    if(currentProject.fileCount === 0) {
        // TODO: Maybe move some of this logic to the record page svelte file and make this a modal?
        // Project Completed. alert the user of such and ask if they want to close the project
        const response = await ask('You completed this project! would you like to navigate back to the Home page?', {
            title: 'Congratulations!',
            kind: 'info'
        });
        if(response) {
            goto('/');
            isProjectLoaded = false;
        }
        else return;
    }
    else {
        await getNextFile();

        completionPercentage = await calculateCompletion();

        truncatedCurrentFile = truncateDirectory(currentFile, 2);
        currentFileName = currentFile.substring(currentFile.lastIndexOf('/') + 1);

        currentFileLocalPath = currentFile.split(currentProject.path)[1];
        currentOutFile = await path.join(outputFolderPath, currentFileLocalPath);
    }
}

async function getNextFile() {
    if(isProjectLoaded) {
        const index = 0;
        const deleteCount = 1;
        projectFiles.splice(index, deleteCount);
    }
    currentFile = projectFiles[0];
}

export function truncateDirectory(path: string, dirLevels: number) {
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
        projectPath: currentProject.path
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
            path: currentProject.path
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
    const files = await countFiles(outputFolderPath);
    return (files / await countFiles(outputFolderPath) + files) * 100;
}

export async function countFiles(path: string): Promise<number> {
    return new Promise((resolve) => {
        invoke("count_files", {
            path: path
        }).then((res) => {
            resolve(res as number);
        })
    })
}


export function isAudioFile(path: string): boolean {
    const supportedTypes: string[] = ['.wav', '.mp3', '.ogg', '.flac'];
    return supportedTypes.some(ext => path.toLowerCase().endsWith(ext.toLowerCase()));
}

export function normalizePath(p: string): string {
  return p.replace(/[/\\]+$/, '');
}

export function toggleExtraEdits() {
    extraEditsFlagged = !extraEditsFlagged;
}
