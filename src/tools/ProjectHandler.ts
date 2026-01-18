// This file handles everything related to projects, From moving files around to recording audio for a project
import { invoke } from "@tauri-apps/api/core";
import {
  basename,
  dirname,
  extname,
  join,
  resolveResource,
} from "@tauri-apps/api/path";
import {
  exists,
  mkdir,
  readTextFile,
  remove,
  rename,
  writeTextFile,
} from "@tauri-apps/plugin-fs";
import { populateFFMpegFilters } from "../routes/recordPage/AudioManager";
import { error } from "@tauri-apps/plugin-log";
import { getValue, setValue } from "./DataInterface";
import { message } from "@tauri-apps/plugin-dialog";
import { goto } from "$app/navigation";
import { platform } from "@tauri-apps/plugin-os";
import { openPath } from "@tauri-apps/plugin-opener";

/**
 * @description points to the output folder in the installation directory (installDir/output)
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
export let projectLoaded: boolean = false;

let currentLoadedProject: string = "";

// Initial setup functions
export async function createAdditionalData() {
  const installFolder = (await invoke("get_install_directory")) as string;
  appOutputFolder = await join(installFolder, "output");
  if (!(await exists(appOutputFolder))) await mkdir(appOutputFolder);
}

/**
 * @description Sets the active project based on user selection
 * @param projectFile Path to the .arproj file that the user wants to open
 */
export async function setActiveProject(projectFile: string) {
  projectLoaded = false;
  if (currentFile === projectFile) return; // Prevents unnecessary computation if a user is just clicking on a loaded project

  currentLoadedProject = projectFile;
  const object = JSON.parse(await readTextFile(currentLoadedProject));

  if (object.completed) {
    await message("This Project is Already Completed!", { kind: "info" });
    return;
  }

  inputFolder = object.path;
  outputFolder = await join(appOutputFolder, object.name);
  if (!(await exists(outputFolder))) await mkdir(outputFolder);

  inputFiles = await getAllFiles(inputFolder);
  await sortInputFiles();

  outputFiles = await getAllFiles(outputFolder);

  // Replicate folder structure of input folder
  const inputDirs = await getAllDirectoryNames(inputFolder);
  const outputDirs = await getAllDirectoryNames(outputFolder);

  const uncreatedFolders = inputDirs.filter((i) => !outputDirs.includes(i));
  for (let i = 0; i < uncreatedFolders.length; i++) {
    const directory = await join(outputFolder, uncreatedFolders[i]);
    console.log(directory);
    if (!(await exists(directory))) await mkdir(directory);
  }

  // Also get the audio manager to populate the ffmpeg pitch and effect filters from the arproj file
  populateFFMpegFilters(object);

  // Set some variables for the first file
  await getNextFile();

  // Mark project Loaded
  projectLoaded = true;
}

async function getNextFile() {
  // Delete any unneeded/empty directories in the input folder (Might remove later)
  await invoke("delete_empty_subdirectories", {
    projectPath: inputFolder,
  });

  if (inputFiles.length === 0) {
    await message("No more files remaining!", {
      title: "",
      kind: "info",
    });
    await goto("/");
    projectLoaded = false;
    await updateArprojStats("completed", true);
    currentLoadedProject = "";
    return;
  }

  currentFile = inputFiles[0];

  // Get the "Local path" of the current file, then join together with the path to the output folder
  localPath = currentFile.split(inputFolder)[1]; // This gets the path from the input folder down to the file
  outputFile = await join(outputFolder, localPath); // Join the path to the output to the local path string to create a new path to the output file that doesn't exist yet!
  fileTranscription = await transcribeFile();
}

export async function getAllFiles(folder: string): Promise<string[]> {
  return new Promise((resolve) => {
    invoke("get_all_files", {
      path: folder,
    }).then((res: any) => {
      const filtered = res.filter((p: string) => isAudioFile(p));
      resolve(filtered);
    });
  });
}

async function getAllDirectoryNames(folder: string) {
  const dirSeparator = platform() === "windows" ? "\\" : "/";
  const folders = await getAllDirectories(folder);
  const names = [];

  if (folders.length === 0) return [];
  for (let i = 0; i < folders.length; i++) {
    names.push(folders[i].split(`${folder}${dirSeparator}`)[1]);
  }
  return names;
}

export async function getAllDirectories(folder: string): Promise<string[]> {
  const res = (await invoke("get_all_directories", {
    path: folder,
  })) as string[];
  console.log(res);
  return res;
}

const transcribeFile = async () => {
  try {
    const allowTranscription = (await getValue(
      "settings.enableTranscription",
    )) as boolean;
    if (!allowTranscription) return "Transcription Service Is Disabled";

    const model = await resolveResource("binaries/whisper.bin");
    return new Promise<string>((resolve) => {
      invoke<string>("transcribe_file", {
        path: currentFile,
        modelPath: model,
      }).then((res) => {
        resolve(res as string);
      });
    });
  } catch (e: any) {
    await error(`Error while transcribing file: ${e}`);
    return "An Error Occurred while transcribing this file!";
  }
};

// Finalising recording functions
/**
 * @description Moves the current active file to the output folder or deletes it
 * @param moveToOutput weather or not to move the original file to the output folder or to delete it
 */
export async function skipFile(moveToOutput: boolean = true) {
  inputFiles.splice(0, 1);
  await updateArprojStats("filesRemaining", inputFiles.length);

  const filesSkipped = await getValue("statistics.filesSkipped");
  await setValue("statistics.filesSkipped", filesSkipped + 1);

  if (moveToOutput) {
    await rename(currentFile, outputFile);
    outputFiles.push(currentFile);
  } else {
    await remove(currentFile);
  }
  await getNextFile();
}

/**
 * @description Review phase: User decided that the recording should be discarded
 */
export async function discardFile() {
  const discardedFiles = await getValue("statistics.filesRejected");
  await setValue("statistics.filesRejected", discardedFiles + 1);
  await remove(outputFile);
}

/**
 * @description Review Phase: User accepts their recording
 * @param requiresExtraEdits weather or not ExtraEditsRequired should be appended to the end of the file name
 */
export async function submitFile(requiresExtraEdits: boolean) {
  inputFiles.splice(0, 1);
  await updateArprojStats("filesRemaining", inputFiles.length);

  const filesAccepted = await getValue("statistics.filesAccepted");
  await setValue("statistics.filesAccepted", filesAccepted + 1);

  const fileName = requiresExtraEdits
    ? await (async () => {
        const dir = await dirname(outputFile);
        const name = await basename(outputFile);
        const ext = await extname(outputFile);

        return await join(dir, `${name}-ExtraEditsRequired.${ext}`);
      })()
    : outputFile;
  await rename(currentFile, fileName);
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

/**
 * @description sorts project files based on selected sorting method on project load
 */
export async function sortInputFiles() {
  const sortingMethod: string = await getValue("settings.sortingMethod");
  switch (sortingMethod) {
    default: // Sort Alphabetically (The default value in the app settings)
      inputFiles.sort();
      break;
    case "reverseAlphabetical":
      inputFiles.reverse();
      break;
    case "random":
      // This sorting method is probably very computationally expensive, so I made a rust command that should cut down on quite a bit of sorting time
      await invoke<string[]>("randomize_file_order", {
        arr: inputFiles,
      }).then((res) => {
        inputFiles = res;
      });
      break;
  }
}

/**
 * @description Checks if a given file has the .wav, .mp3, .flac, or .m4a extension
 * @param path Path to the file
 */
function isAudioFile(path: string): boolean {
  const types: string[] = [".wav", "mp3", ".flac", ".m4a"]; // Maybe I should make this a bit more robust in the future
  return types.some((ext) => path.toLowerCase().endsWith(ext.toLowerCase()));
}

export async function createArProj(inputFolder: string) {
  return {
    name: await basename(inputFolder),
    path: inputFolder,
    lastOpened: Date.now(),
    filesRemaining: (await getAllFiles(inputFolder)).length,
    completed: false,
    // Default pitch and effect filters because why not
    pitchFilters: [
      {
        name: "Default",
        value: 1.0,
      },
      {
        name: "Super Low",
        value: 0.5,
      },
      {
        name: "Super High",
        value: 1.5,
      },
    ],
    effectFilters: [
      {
        name: "Default",
        value: "",
      },
      {
        name: "Reverb",
        value: "aecho=0.8:0.9:40|50|70:0.4|0.3|0.2",
      },
    ],
  };
}
export async function getArprojProperty(key: string) {
  if (projectLoaded) return null;
  return JSON.parse(await readTextFile(currentLoadedProject))[key];
}

export async function updateArprojStats(key: string, value: any) {
  const arproj = JSON.parse(await readTextFile(currentLoadedProject));
  arproj[key] = value;
  await writeTextFile(currentLoadedProject, JSON.stringify(arproj));
}

export async function openOutputFolder() {
  if (!projectLoaded) return;
  await openPath(outputFolder);
}
