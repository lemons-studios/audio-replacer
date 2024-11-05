'use client'
import { readDir, BaseDirectory, truncate, mkdir, readFile, exists } from '@tauri-apps/plugin-fs';
import * as tauriPath from '@tauri-apps/api/path'

let fileData = {
    inFolderPath: "",
    currentFile: "",
    currentFileTrunc: "",
    filesRemaining: "",
    dirStrucutre: [],
    outPath: ""
};


export async function setFolderPath(path)
{
    fileData.inFolderPath = path
}

export async function setData()
{
    fileData.dirStrucutre = (await readDir(fileData.inFolderPath, {baseDir: BaseDirectory.Home})).toString()
    fileData.outPath = await tauriPath.join(await tauriPath.homeDir(), "audioReplacer2-out");

    fileData.currentFile = await getCurrentFile()
    fileData.currentFileTrunc = await truncatePath(2) 
    
    alert(fileData.inFolderPath)
    alert(fileData.currentFile)

    await initialProjectSetup()
}

async function initialProjectSetup()
{
    if(!(await exists(fileData.outPath)))
    {
        await mkdir(fileData.outPath, {baseDir: BaseDirectory.Home})
    }
    let outDirStrucutre = await readDir(fileData.outPath, {baseDir: BaseDirectory.Home})

    if(JSON.stringify(fileData.dirStrucutre) != JSON.stringify(outDirStrucutre))
    {
        for(let i = 0; i < fileData.dirStrucutre.length; i++)
        {
            await mkdir(fileData.dirStrucutre[i], {baseDir: BaseDirectory.Home})
        }
    }
}

async function getCurrentFile()
{
    let dirStructure = fileData.dirStrucutre[0].toString()
    return tauriPath.join(dirStructure, `${readFile(dirStructure)[0]}`).toString()
}

export function getTruncatedFile()
{
    return fileData.currentFileTrunc
}

export async function countRemainingFiles()
{
    let remainingFiles = 0
    let dirStructure = fileData.dirStrucutre

    for(let i = 0; i < dirStructure.length; i++)
    {
        remainingFiles += (await readDir(`${fileData.dirStrucutre[i]}`, {baseDir: BaseDirectory.Home})).length
    }
}

const truncatePath = async (n) => fileData.inFolderPath.split("\\").slice(-n);

function backwardsArrayIndex(array, position)
{
    let reversedArray = array.reverse();
    return reversedArray[position]
}
