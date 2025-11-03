<script lang="ts">
  import { onMount, tick } from "svelte";
  import { getValue, setValue } from "../tools/SettingsManager";
  import { countFiles, isArprojValid, projectFilePath, setProjectData } from "../tools/ProjectManager";
  import { exists, readTextFile, writeTextFile } from "@tauri-apps/plugin-fs";
  import { goto } from "$app/navigation";
  import { selectFile, selectFolder, timestampToLegible } from "../tools/OsTools";
  import { info, error } from "@tauri-apps/plugin-log";
  import { invoke } from "@tauri-apps/api/core";
  import { basename, join } from "@tauri-apps/api/path";
  import ArrowRightRegular from 'phosphor-icons-svelte/IconArrowRightRegular.svelte';
  import { message } from "@tauri-apps/plugin-dialog";
    import { path } from "@tauri-apps/api";


  /**
   * @description Index 0: Path, Index 1: JSON
   */
  let recentProjects: any[][] = $state([]);

  let user = $state("");
  let isProjectLoading = $state(false);

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

  /**
   * @description Sorts projects by last opened date (newest first)
   */
  function sortProjects() {
    recentProjects.sort((a, b) => a[1].lastOpened - b[1].lastOpened);
    recentProjects.reverse();
  }


  async function createProject() {
    const folder = await selectFolder();
    if(!exists(folder)) {
      // TODO: show error popup
      return;
    }

    const name = await basename(folder);
    const fileCount = await countFiles(folder);
    
    const project = {
      name: name,
      path: folder,
      lastOpened: new Date().getTime(),
      fileCount: fileCount,
      pitchList: [],
      effectList: []
    }
    const savePath = await join(projectFilePath, `${name}.arproj`);
    await writeTextFile(savePath, JSON.stringify(project));
    recentProjects.push([savePath, project]);
    sortProjects();

    await setProjectData(savePath);
  }

  async function loadProjectFromUI(index: number) {
    const selectedProject = recentProjects[index][1];
    // Update last accessed time
    selectedProject.lastOpened = new Date().getTime();

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

  function getMostRecentProjects() {
    const res = [];
    const length = recentProjects.length < 3 ? recentProjects.length : 3;
    for(let i = 0; i < length; i++) {
      res.push(recentProjects[i][1]);
      res[i].lastOpened = timestampToLegible(res[i].lastOpened);
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
    <div class="flex flex-col justify-center content-center align-center gap-y-7">
      <div class="flex flex-row gap-x-5 gap-y-2 justify-center">
        <!--Load From Save Projects-->
        <div class="home-page-card">
          <h1 class="text-lg mb-5 underline-offset-2 underline">Open A Saved Project</h1>
          <!--Show the three most recent projects, then have a "view all button that shows a modal at the bottom"-->
          {#each getMostRecentProjects() as rp, index (rp)}
            <button 
              class="save-btn rounded-sm w-125 dark:hover:bg-tertiary-d hover:bg-tertiary dark:focus:bg-tertiary-d dark:focus:drop-shadow-xl transition duration-75" 
              onmouseleave={(e) => e.currentTarget.blur()} 
              onmouseup={(e) => e.currentTarget.blur()} 
              onclick={() => loadProjectFromUI(index)}>
              <div class="flex justify-between items-center text-left p-3 mb-1.5 mt-1.5">
                <div class="flex flex-col">
                  <p class="text-lg">{rp.name}</p>
                  <p class="text-gray-400 text-sm">Last Opened: {rp.lastOpened}</p>
                  {#await countFiles(rp.path)}
                    <p class="text-gray-400 text-xs">Files Remaining: 0</p> <!--Placeholder value-->
                  {:then res} 
                    <p class="text-gray-400 text-xs">Files Remaining: {Intl.NumberFormat().format(res)}</p>
                  {/await}
                </div>
                <ArrowRightRegular class="arrow"></ArrowRightRegular>
              </div>
            </button>
          {/each}
          
          <button 
            class="save-btn rounded-sm w-full dark:hover:bg-tertiary-d hover:bg-tertiary dark:focus:bg-tertiary-d dark:focus:drop-shadow-xl transition duration-75" 
            onmouseleave={(e) => e.currentTarget.blur()} 
            onmouseup={(e) => e.currentTarget.blur()}>
              <div class="flex justify-between items-center text-left p-3 mb-1.5 mt-1.5">
                <div class="flex flex-col">
                  <p class="text-lg">Project Name</p>
                  <p class="text-gray-400 text-sm">Last Opened: 2025-10-30 2:08 PM</p>
                  <p class="text-gray-400 text-xs">Files Remaining: 25,101</p>
                </div>
                <ArrowRightRegular class="arrow w-5 h-5"></ArrowRightRegular>
              </div>
            </button>
            <button class="bg-black p-2 rounded-md hover:bg-tertiary-d dark:focus:bg-secondary-d" 
            onmouseleave={(e) => e.currentTarget.blur()} 
            onmouseup={(e) => e.currentTarget.blur()}>Recent Projects</button>
        </div>
        <div class="home-page-card">
          <h1 class="text-lg mb-5 underline-offset-2 underline">Start A New Project</h1>
        </div>
      </div>
      <div class="home-page-card">
        <h1 class="text-lg mb-5 underline-offset-2 underline">Wiki/Tutorials</h1>

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
