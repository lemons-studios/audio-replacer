import { invoke } from '@tauri-apps/api/core';
import * as path from '@tauri-apps/api/path';
import { mkdir, exists } from '@tauri-apps/plugin-fs';
import * as webPath from 'path-browserify';
import { convertFileFormat } from './FFMpegManager';


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
    projectFiles = (await getAllFiles()).filter(p => isAudioFile(p));

    outputFolderPath = await path.join(outputFolder, projectName)
    if (await exists(outputFolderPath)) {
        await mkdir(outputFolderPath);
    }
}

async function createInitialData(): Promise<void> {
    
    const inputDirectories = (await getSubdirectories(projectPath)).map(d => normalizePath(d));
    const outputDirectories = (await getSubdirectories(outputFolderPath)).map(d => normalizePath(d));

    if(inputDirectories == outputDirectories) {
        return;
    }

    inputDirectories.forEach(async(dir) => {
        const relativePath = webPath.relative(projectPath, dir) ;
        const outDir = path.join(outputFolderPath, relativePath);
        if(await exists(await outDir /* ?????? */)) {
            await mkdir(await outDir);
        }
    });

    await createProjectdata();
}

async function createProjectdata() {
    const undesiredFiles = projectFiles.filter(p => isUndesirableAudioFile(p));
    if(undesiredFiles.length != 0) {
        undesiredFiles.forEach(async(f) => {
            await convertFileFormat(f, "wav");
        });
    }
}

async function setCurrentFile() {

}

async function getCurrentFile() {

}

export async function submitFile() {

}

export async function rejectFile() {

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

export async function getSubdirectories(folder: string): Promise<string[]> {
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
            input_path: projectPath,
            output_path: outputFolderPath
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
    const undesirableFileTypes = [".mp3", ".wav", ".ogg", ".flac", ".m4a"];
    return undesirableFileTypes.some((ext) => path.toLowerCase().endsWith(ext.toLowerCase()));
}

function normalizePath(p: string): string {
  return p.replace(/[/\\]+$/, '');
}