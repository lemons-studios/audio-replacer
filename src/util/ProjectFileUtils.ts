import { invoke } from '@tauri-apps/api/core';
import { getPathSeparator } from './OsData'

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
        let unparsedProjectName = this.projectPath.split(getPathSeparator());
        let projectName = unparsedProjectName.at(-1);

        this.projectFiles = await this.getAllFiles();
    } 

    private static async getAllFiles(): Promise<string[]> {
        return new Promise((resolve) => {
            invoke<string[]>("get_all_files", {
                path: this.projectFiles,
                sort: true
            }).then((res) => {
                resolve(res as string[]);
            })
        })
    }
}