<script lang="ts">
  import { onMount, tick } from "svelte";
  import { getValue, setValue } from "../tools/SettingsManager";
  import { countFiles, setProjectData } from "../tools/ProjectManager";
  import { exists, readTextFile } from "@tauri-apps/plugin-fs";
  import { goto } from "$app/navigation";
  import { selectFolder } from "../tools/OsTools";
  import { info, error } from "@tauri-apps/plugin-log";
  import { invoke } from "@tauri-apps/api/core";
    import { basename } from "@tauri-apps/api/path";

  let recentProjectPaths: string[];
  let recentProjects: any[] = $state([]);

  let user = $state("");
  let previousPath = $state("");
  let isProjectLoading = $state(false);

  onMount(async () => {
    await tick();
    user = await invoke('get_username');
    recentProjectPaths = (await getValue('recentProjects')) as string[];
    
    for(let i = 0; i < recentProjectPaths.length; i++) {
      const fileContents = await readTextFile(recentProjectPaths[i]);
      const json = JSON.parse(fileContents);
      if(await isArprojValid(json)) {
        recentProjects.push(json);
      }
    }
    recentProjects.sort((a, b) => a.lastOpened - b.lastOpened);
    recentProjects.reverse();

    info("App Loaded!");
  });

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
  }

  async function loadPreviousProject(index: number) {
    
  }

  async function loadLastProject() {
    // Edge case where path exists on app load but no longer exists when trying to load project
    if (!await exists(previousPath)) {
      return;
    }
    isProjectLoading = true;
    await setProjectData(previousPath);
    isProjectLoading = false;
    goto("/recordPage");
  }

  async function loadNewProject() {
    const res = await selectFolder();
    
    if (await exists(res)) {
      isProjectLoading = true;
      await setProjectData(res);
      setValue("lastSelectedFolder", res as string);
      isProjectLoading = false;
      goto("/recordPage");
    } else {
      error(`${res} Does not exist`);
    }
  }

  const isArprojValid = (async(obj: Object) => {
    const properties = ['name', 'path', 'lastOpened', 'pitchList', 'effectList'];
    return properties.every(p => p in obj);
  })
</script>

{#if isProjectLoading}
  <!--Add Load Spinner Component here-->
  <span>Loading Project...</span>
{/if}

{#if !isProjectLoading}
  <div class="h-full">
    <h1 class="text-center text-3xl mb-5">Welcome Back, {user}</h1>
    <div class="flex flex-col gap-3">
      <div class="flex flex-row gap-x-4">

      </div>
      <div>

      </div>
    </div>
  </div>
{/if}
