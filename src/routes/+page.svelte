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
  import ArrowLineUpRightReguar from 'phosphor-icons-svelte/IconArrowUpRightRegular.svelte';
  import Modal from "../Components/Modal.svelte";

  let recentProjectPaths: string[];
  let recentProjects: any[] = $state([]);

  let user = $state("");
  let previousPath = $state("");
  let isProjectLoading = $state(false);

  let showTestModal = $state(true);

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

  async function createNewProject() {
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
  <div>
    <h1 class="text-center text-3xl mb-5">Welcome Back, {user}</h1>
    <div class="flex flex-col justify-center content-center align-center gap-3">
      <div class="flex flex-row gap-x-4 justify-evenly">
        <!--Load From Save Projects-->
        <div class=" p-4 rounded-lg dark:bg-secondary-d bg-secondary min-w-125 min-h-80 border-accent-shadow hover:border-accent hover:drop-shadow-accent-shadow hover:drop-shadow-xl transition border-2">
          <h1 class="text-lg">Open A Saved Project</h1>
          <!--Show the three most recent projects, then have a "view all button that shows a modal at the bottom"-->
          {#each recentProjects as rp, index (rp)}
            {#if index === 2}
              <hr>
              <div class="h-20 w-75 flex justify-around">
                <!--Name of project + last opened date-->
                <div>

                </div>
                <!--Forward Arrow Icon (Middle Right)-->
                <ArrowLineUpRightReguar></ArrowLineUpRightReguar>
              </div>
              <hr>
            {/if}
          {/each}
        </div>
      </div>
      <div>

      </div>
    </div>
  </div>
{/if}

<!-- <Modal bind:showModal={showTestModal}>
    <div class="flex flex-col h-full w-full justify-center content-center align-center text-center dark:text-white gap-2.5">
      <h2>This is a test Modal!</h2>
      <button>Wow!</button>
    </div>
  </Modal>-->