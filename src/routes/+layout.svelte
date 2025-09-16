<script lang="ts">
  import { invoke } from "@tauri-apps/api/core";
  import "../app.css";
  import { House, Mic, PencilLine, Settings, Megaphone } from "@lucide/svelte";
  import { onMount } from "svelte";
  import { populateCustomData } from "./recordPage/EffectManager";
  import { loadFFMpeg } from "./recordPage/FFMpegManager";
  import { getVersion } from "@tauri-apps/api/app";
  import { page } from "$app/state";
  
  let { children } = $props();
  let versionNumber = $state("");


  onMount(async() => {
    versionNumber = await formatVersion();
    await populateCustomData();
    await loadFFMpeg();  

    // Prevent right click context menu from showing up (unneeded in production builds)
    const isDev = await invoke("in_dev_env") as boolean;
    console.log(`Developer environment? ${isDev ? "Yes" : "No"}`)
    if(!isDev) {
      document.addEventListener('DOMContentLoaded', () => {
        document.addEventListener('contextmenu', (e) => {
          e.preventDefault();
        })
      })
    }
  });

  async function formatVersion(): Promise<string> {
    const [major, minor, patch] = (await getVersion()).split(".")
    return patch != "0" ? `${major}.${minor}.${patch}` : `${major}.${minor}`;
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
</main>
