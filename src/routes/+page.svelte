<script lang="ts">
  import { onMount, tick } from "svelte";
  import { getValue, setValue } from "../tools/SettingsManager";
  import { countFiles, isArprojValid, projectFilePath, setProjectData } from "../tools/ProjectManager";
  import { exists, readTextFile, writeTextFile } from "@tauri-apps/plugin-fs";
  import { goto } from "$app/navigation";
  import { selectFile, selectFolder } from "../tools/OsTools";
  import { info, error } from "@tauri-apps/plugin-log";
  import { invoke } from "@tauri-apps/api/core";
  import { basename, join } from "@tauri-apps/api/path";
  import ArrowRightRegular from 'phosphor-icons-svelte/IconArrowRightRegular.svelte';
  import { message } from "@tauri-apps/plugin-dialog";


  /**
   * @description Index 0: Path, Index 1: JSON
   */
  let recentProjects: any[][] = $state([]);

  let user = $state("");
  let isProjectLoading = $state(false);
  let showTestModal = $state(false);

  onMount(async () => {
    await tick();
    user = await invoke('get_username');
    const projectPaths = (await getValue('recentProjects')) as string[];
    for(let i = 0; i < projectPaths.length; i++) {
      const text = await readTextFile(projectPaths[i]);
      const json = await JSON.parse(text);
      recentProjects.push([projectPaths[i], json]);
    }

    sortProjects();
    console.log(recentProjects);

    info("App Loaded!");
  });

  function sortProjects() {
    recentProjects.sort((a, b) => a[1].lastOpened - b[1].lastOpened);
    recentProjects.reverse();
  }

  async function createProject() {
    const folder = await selectFolder();
    const name = await basename(folder);
    const fileCount = await countFiles(folder);
    
    const project = {
      name: name,
      path: folder,
      lastOpened: new Date().toLocaleDateString(),
      fileCount: fileCount,
      pitchList: [],
      effectList: []
    }
    const savePath = await join(projectFilePath, `${name}.arproj`);
    await writeTextFile(savePath, JSON.stringify(project));
    recentProjects.push([savePath, project]);
  }

  async function loadProjectFromUI(index: number) {
    const selectedProject = recentProjects[index][1];
    // Update last accessed time
    selectedProject.lastOpened = new Date().toLocaleString();

    // Push back to arproj array and save
    recentProjects[index][1] = selectedProject;
    await writeTextFile(recentProjects[index][0], JSON.stringify(recentProjects[index][1]));

    await setProjectData(selectedProject);
  }

  async function loadProjectFromFilePicker() {
    const file = await selectFile(["arproj"], "Audio Replacer Project files (.arproj)");
    const json = JSON.parse(file);
    // Make sure file is valid
    if(!await isArprojValid(json)) {
      await message('This file does not appear to be valid!', { title: 'Error!', kind: 'error' });
      return;
    }

    // Add to settings
    const settings = await getValue("recentProjects") as string[];
    settings.push(file);
    setValue("recentProjects", settings);

    // Push to recent projects and re-sort so the project appears in the recent projects list without needing a relaunch
    recentProjects.push(json);
    sortProjects();

    // Now, Load the project
    await setProjectData(json);
    goto("/recordPage");
  }

  async function createNewProject() {
    const res = await selectFolder();
    if (await exists(res)) {
      // From this selected folder, construct an object and save it to the app settings
      const obj = {
        name: await basename(res),
        path: res,
        lastOpened: new Date().toLocaleString(),
        fileCount: await countFiles(res),
        pitchList: [],
        effectList: [],
      }
      
    } else {
      // TODO: Show a toast here
      error(`${res} Does not exist`);
    }
  }

  function getMostRecentProjects() {
    const res = [];
    const length = recentProjects.length < 3 ? recentProjects.length : 3;
    for(let i = 0; i < length; i++) {
      res.push(recentProjects[i][1]);
    }
    return res;
  }

</script>

{#if isProjectLoading}
  <!--Add Load Spinner Component here-->
  <span>Loading Project...</span>
{/if}

{#if !isProjectLoading}
  <div>
    <h1 class="text-center text-3xl mb-5">Welcome Back, {user}</h1>
    <div class="flex flex-col justify-center content-center align-center gap-3">
      <div class="flex flex-row gap-x-4 justify-evenly">
        <!--Load From Save Projects-->
        <div class=" p-4 rounded-lg dark:bg-secondary-d bg-secondary min-w-125 min-h-80 dark:border-accent-shadow dark:hover:border-accent dark:hover:drop-shadow-accent-shadow hover:drop-shadow-xl transition border-2">
          <h1 class="text-lg mb-5 underline-offset-2 underline">Open A Saved Project</h1>
          <!--Show the three most recent projects, then have a "view all button that shows a modal at the bottom"-->
          {#each getMostRecentProjects() as rp}
            <div class="group flex w-auto cursor-pointer items-center justify-between rounded-md border border-transparent p-4 transition-all duration-200 ">
              
            </div>
          {/each}
          <button class="save-btn rounded-sm w-125 dark:hover:bg-tertiary-d hover:bg-tertiary dark:focus:bg-tertiary-d dark:focus:drop-shadow-xl transition duration-300" onmouseleave={(e) => e.currentTarget.blur()} onmouseup={(e) => e.currentTarget.blur()}>
            <div class="flex flex-col text-left p-3  mb-1.5 mt-1.5">
                <p class="text-lg">Project Name</p>
                <p class="text-gray-400 text-sm">Last Opened: 2025-10-30 2:08 PM</p>
                <p class="text-gray-400 text-xs">Files Remaining: 25,101 (0% Complete)</p>
                <ArrowRightRegular class="arrow"></ArrowRightRegular>
            </div>
          </button>
        </div>
      </div>
      <div>

      </div>
    </div>
  </div>
{/if}

<style>
  .save-btn:hover {
    background-color: oklch(0.3042 0.0051 325.78);
    box-shadow: none;

  }
  .save-btn:focus {
    background-color: oklch(1 0 0 / 10%);
    box-shadow: inset 0px 0px 1em oklch(0.1929 0.0048 325.72 / 60%);
  }
  .arrow {
    position: fixed;
  }
</style>

<!-- <Modal bind:showModal={showTestModal}>
    <div class="flex flex-col h-full w-full justify-center content-center align-center text-center dark:text-white gap-2.5">
      <h2>This is a test Modal!</h2>
      <button>Wow!</button>
    </div>
  </Modal>-->