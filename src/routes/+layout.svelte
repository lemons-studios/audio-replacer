<script lang="ts">
  import "../app.css";
  import { invoke } from "@tauri-apps/api/core";
  import { onMount, tick } from "svelte";
  import { populateCustomData } from "./recordPage/EffectManager";
  import { loadFFMpeg } from "./recordPage/FFMpegManager";
  import { getVersion } from "@tauri-apps/api/app";
  import { downloadUpdates, getUpdateVersion, isUpdateAvailable } from "../tools/Updater";
  import { ask } from "@tauri-apps/plugin-dialog";
  import { getValue } from "../tools/SettingsManager";
  import { onNavigate } from '$app/navigation';
  import NavBar from "../Components/NavBar.svelte";
  import Notification from "../Components/Notifications/Notification.svelte";
  import { NotificationTypes } from "../Components/Notifications/NotificationTypes";
  import { setAdditionalFolderLocs } from "../tools/ProjectManager";

  let { children } = $props();
  let versionNumber = $state("");
  let isUpdating = $state(false);
  let notificationRef: Notification;
  
  onMount(async() => {
    await tick();

    // Initialize some variables related to project managment
    await setAdditionalFolderLocs();

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

    // Only check for updates if the user wants to
    const allowUpdates = await getValue("updateCheck");
    if(allowUpdates && !isDev) {
      if(await isUpdateAvailable()) {
        const response = await ask(`There is an update available for Audio Replacer.\nLatest Version: ${getUpdateVersion()}\nCurrent Version: ${versionNumber}`, {
          title: 'Update Available',
          kind: 'warning'
        });
        if(response) {
          isUpdating = true;
          await downloadUpdates();
        }
      }
    }
  });

  async function formatVersion(): Promise<string> {
    const [major, minor, patch] = (await getVersion()).split(".");
    return `${major}.${minor}${patch == "0" ? '' : `.${patch}`}`;
  }

  onNavigate((navigation) => {
    if(!document.startViewTransition) return;

    return new Promise((resolve) => {
      document.startViewTransition(async() => {
        resolve();
        await navigation.complete;
      });
    });
  });
</script>

<style>
  * {
  user-select: none;
  -webkit-user-select: none;
  -ms-user-select: none;
  }
</style>

<main class="dark:bg-primary-d bg-primary flex flex-row grow-1 dark:text-white items-stretch w-screen h-screen overflow-y-hidden">
  <div class="notification-overlay">
    <Notification bind:this={notificationRef}/>
  </div>
  {#if !isUpdating}
    <NavBar />
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
