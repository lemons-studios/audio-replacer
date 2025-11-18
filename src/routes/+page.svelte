<script lang="ts">
  import { onMount, tick } from "svelte";
  import { getValue, setValue } from "../tools/SettingsManager";
  import { exists, readTextFile, writeTextFile } from "@tauri-apps/plugin-fs";
  import { goto } from "$app/navigation";
  import { selectFile, selectFolder } from "../tools/OsTools";
  import { info, error } from "@tauri-apps/plugin-log";
  import { invoke } from "@tauri-apps/api/core";
  import { basename, join } from "@tauri-apps/api/path";
  import { message, save } from "@tauri-apps/plugin-dialog";
  import RecentProjectItem from "../Components/RecentProjectItem.svelte";
  import Slider from "../Components/Slider.svelte";
  import { startRichPresence } from "../tools/DiscordPresenceManager";
  import { getAllFiles, setActiveProject } from "../tools/ProjectHandler";

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
    
    const project = {
      name: name,
      path: folder,
      lastOpened: new Date().getTime(),
      pitchList: [],
      effectList: []
    }
    const savePath = await save({
      filters: [
        {
          name: "Audio Replacer File",
          extensions: ['arproj'] 
        },
      ],
    }) as string;
    if(savePath) {
      await writeTextFile(savePath, JSON.stringify(project));
      recentProjects.push([savePath, project]);
      sortProjects();

      await setActiveProject(savePath);
    }

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
    await setActiveProject(file);
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

  function isArprojValid(project: any): boolean {
    return (
      'name' in project && typeof project.name === 'string' &&
      'path' in project && typeof project.path === 'string' &&
      'lastOpened' in project && typeof project.lastOpened === 'number' &&
      'pitchList' in project && Array.isArray(project.pitchList) &&
      'effectList' in project && Array.isArray(project.effectList)
    );
  }

</script>

{#if isProjectLoading}
  <!--Add Load Spinner Component here-->
  <span>Loading Project...</span>
{:else}
  <!--Implement actual content after mocking up UI in figma-->
{/if}
