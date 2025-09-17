<script lang="ts">
  import "../app.css";
  import { invoke } from "@tauri-apps/api/core";
  import { House, Mic, PencilLine, Settings, Megaphone } from "@lucide/svelte";
  import { onMount } from "svelte";
  import { populateCustomData } from "./recordPage/EffectManager";
  import { loadFFMpeg } from "./recordPage/FFMpegManager";
  import { getVersion } from "@tauri-apps/api/app";
  import { downloadUpdates, getUpdateVersion, isUpdateAvailable } from "../tools/Updater";
  import { ask } from "@tauri-apps/plugin-dialog";
  
  let { children } = $props();
  let versionNumber = $state("");
  let isUpdating = $state(false);

  onMount(async() => {
    versionNumber = await formatVersion();
    await populateCustomData();
    await loadFFMpeg();

    // Prevent right click context menu from showing up (unneeded in production builds)
    const isDev = await invoke("in_dev_env") as boolean;
    if(!isDev) {
      document.addEventListener('contextmenu', (e) => {
          e.preventDefault();
      });
    }
    // Check for updates
    if(await isUpdateAvailable()) {
      const response = await ask(`There is an update available to Audio Replacer\n Latest Version: ${getUpdateVersion()} \nCurrent Version: ${versionNumber}`, {
        title: 'Update Available',
        kind: 'warning'
      })
      if(response) {
        isUpdating = true;
        await downloadUpdates();
      }
    }
    else {
      console.log("No Update Available");
    }
  });

  async function formatVersion(): Promise<string> {
    const [major, minor, patch] = (await getVersion()).split(".")
    return `${major}.${minor}${patch == "0" ? '' : `.${patch}`}`
    // return patch != "0" ? `${major}.${minor}.${patch}` : `${major}.${minor}`;
  }
</script>

<style>
  * {
  user-select: none;
  -webkit-user-select: none;
  -ms-user-select: none;
  }
</style>

<main class="bg-base-200 flex flex-row grow-1 text-white items-stretch w-screen h-screen overflow-y-hidden">
  {#if !isUpdating}
    <div class="flex flex-col items-stretch justify-between bg-base-100 min-w-[10rem] p-1">
      <div class="flex flex-col items-stretch">
        <ul class="menu menu-vertical menu-lg gap-0.5 rounded-box w-full bg-transparent">
          <li><a href="/"><House size="20"/>Home</a></li>
          <li><a href="/recordPage"><Mic size="20"/>Record</a></li>
          <li><a href="/dataEditor"><PencilLine size="20"/>Editor</a></li>
        </ul>
    </div>
      <div class="flex flex-col items-stretch">
        <ul class="menu menu-vertical menu-lg gap-0.5 bg-transparent rounded-box w-full">
          <li><a href="/settingsPage"><Settings size="20"/>Settings</a></li>
          <li class="mb-1.5"><a href="/releaseNotes"><Megaphone size="20"/>Changes</a></li>
          <h3 class="text-xs text-center text-gray-300">Audio Replacer {versionNumber}</h3>
        </ul>
      </div>
    </div>
    <div class="flex-1 flex flex-col overflow-hidden w-screen h-screen p-5.5">
      {@render children?.()}
    </div>    
  {/if}
  {#if isUpdating}
    <div class="flex flex-row grow justify-center items-center gap-3 absolute inset-0">
      <span class="loading loading-spinner text-primary text-xl"></span>
      <h3 class="text-xl">Updating Audio Replacer</h3>
      <h4 class="text-lg"><u>Do not</u>close this window</h4>
    </div>
  {/if}
</main>
