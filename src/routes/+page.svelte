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
  import { message } from "@tauri-apps/plugin-dialog";
  import RecentProjectItem from "../Components/RecentProjectItem.svelte";

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

    await setProjectData(savePath, project);
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
    await setProjectData(file, json);
    goto("/recordPage");
  }

  function getMostRecentProjects() {
    const res = [];
    const length = recentProjects.length < 3 ? recentProjects.length : 3;
    for(let i = 0; i < length; i++) {
      // Index 0 = path, Index 1 = object
      res.push([recentProjects[i][0], recentProjects[i][1]]);
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
          {#each getMostRecentProjects() as rp}
            <RecentProjectItem project={rp[1]} projectFilePath={rp[0]} />
          {/each}
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
