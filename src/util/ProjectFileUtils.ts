import { invoke } from '@tauri-apps/api/core';
import { getPathSeparator } from './OsData'
import * as Path from '@tauri-apps/api/path';
import { mkdir, exists } from '@tauri-apps/plugin-fs';
import { outputDirectory } from './AppProperties';

export abstract class ProjectFileUtils {

    private static currentFile: string;
    private static truncatedCurrentFile: string;
    private static currentOutFile: string;
    private static currentFileName: string;
    private static currentFileLocalPath: string;

    private static outputFolderPath: string;
    private static projectPath: string;

    private static readonly SupportedFileTypes = [".mp3", ".wav", ".ogg", ".flac", ".m4a"];
    public static IsProjectLoaded: boolean;
    public static ExtraEditsFlagged = false;

    private static projectFiles: string[] = [];

    public static async setProjectData(path: string) {
        this.projectPath = path;
        let projectName = this.projectPath.split(getPathSeparator()).at(-1)?.toString();
        this.projectFiles = await this.getAllFiles();

        this.outputFolderPath = await Path.join(outputDirectory, projectName)

        if(await exists(this.outputFolderPath)) {
            await mkdir(this.outputFolderPath);
        }
    } 

    private static async getAllFiles(): Promise<string[]> {
        return new Promise((resolve) => {
            invoke("get_all_files", {
                path: this.projectFiles,
                sort: true
            }).then((res) => {
                resolve(res as string[]);
            })
        })
    }

    private static async getSubdirectories(): Promise<string[]> {
        return new Promise((resolve) => {
            invoke("get_subdirectories", { 
                path: this.projectFiles
            }).then((res) => {
                resolve(res as string[]);
            })
        })
    }

    private static isAudioFile(path: string): boolean {
        return this.SupportedFileTypes.some(ext => path.toLowerCase().endsWith(ext.toLowerCase()));
    }


    // Audio replacer works the best with .wav files. 
    private static isUndesirableAudioFile(path: string): boolean {
        const undesirableFileTypes = [".mp3", ".wav", ".ogg", ".flac", ".m4a"];
        return undesirableFileTypes.some((ext) => path.toLowerCase().endsWith(ext.toLowerCase()));
    }
}