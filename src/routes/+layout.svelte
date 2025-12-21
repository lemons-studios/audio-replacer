<script lang="ts">
  import { getValue, initializeData, setValue } from "../tools/DataInterface";
  import "../app.css";
  import { invoke } from "@tauri-apps/api/core";
  import { onMount, tick } from "svelte";
  import { downloadUpdates, getUpdateVersion, isUpdateAvailable } from "../tools/Updater";
  import { ask } from "@tauri-apps/plugin-dialog";
  import { onNavigate } from '$app/navigation';
  import { startRichPresence } from "../tools/DiscordPresenceManager";
  import {formatVersion, getMic, sleep} from "../tools/OsTools";
  import { listen } from "@tauri-apps/api/event";
  import { info } from "@tauri-apps/plugin-log";
  import NavBar from "../Components/NavBar.svelte";
  import Notification from "../Components/Notifications/Notification.svelte";
  import { createAdditionalData } from "../tools/ProjectHandler";

  let { children } = $props();
  let versionNumber = $state("");
  let isUpdating = $state(false);
  let notificationRef: Notification;
  const appLaunchTime = Date.now(); // For app open time statistic tracking

  
  onMount(async() => {
    // Populate additional variables
    await info("Getting/Asking for Microphone Permission");
    await getMic();

    await info("Creating Additional Directories");
    await createAdditionalData();

    versionNumber = await formatVersion();

    // Prevent right click context menu from showing up (unneeded in production builds)
    const isDev = await invoke("in_dev_env") as boolean;
    if(!isDev) {
      document.addEventListener('contextmenu', (e) => {
          e.preventDefault();
      });
    }

    // Only check for updates if the user wants to
    const allowUpdates = getValue('settings.updateCheck');
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
    
    await startRichPresence();
  });

  onNavigate((navigation) => {
    if(!document.startViewTransition) return;

    return new Promise((resolve) => {
      document.startViewTransition(async() => {
        resolve();
        await navigation.complete;
      });
    });
  });
  
  listen('tauri://close-requested', async() => {
    info("App close requested");
    const currentTime = await getValue("statistics.appOpenTime");
    const appCloseTime = Date.now() - appLaunchTime;
    await setValue("statistics.appOpenTime", currentTime + appCloseTime);
  });
</script>

<style>
  * {
  user-select: none;
  -webkit-user-select: none;
  -ms-user-select: none;
  }
</style>

<main class="dark:bg-primary-d bg-primary flex flex-row grow dark:text-white items-stretch w-screen h-screen overflow-y-hidden">
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
      <!--TODO: Re-add spinner and add a progress bar for download progress-->
    </div>
  {/if}
</main>
