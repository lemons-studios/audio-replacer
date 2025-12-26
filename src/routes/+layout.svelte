<script lang="ts">
  import { getValue, initializeData, setValue } from "../tools/DataInterface";
  import "../app.css";
  import { invoke } from "@tauri-apps/api/core";
  import { onDestroy, onMount } from "svelte";
  import { onNavigate } from '$app/navigation';
  import { startRichPresence } from "../tools/DiscordPresenceManager";
  import { getMic } from "../tools/OsTools";
  import { info } from "@tauri-apps/plugin-log";
  import NavBar from "../Components/NavBar.svelte";
  import { createAdditionalData } from "../tools/ProjectHandler";
  import { checkForUpdates } from "../tools/UpdateManager";
  import {setTheme} from "@tauri-apps/api/app";

  let { children } = $props();
  const appLaunchTime = Date.now(); // For app open time statistic tracking

  
  onMount(async() => {
    await setTheme(await getValue('settings.theme'));

    await initializeData();

    // Populate additional variables
    await info("Getting/Asking for Microphone Permission");
    await getMic();

    await info("Creating Additional Directories");
    await createAdditionalData();

    // Prevent right click context menu from showing up (unneeded in production builds)
    const isDev = await invoke("in_dev_env") as boolean;
    if(!isDev) {
      document.addEventListener('contextmenu', (e) => {
          e.preventDefault();
      });
    }

    const allowUpdates = await getValue('settings.updateCheck');
    if(allowUpdates) await checkForUpdates();

    const allowRichPresence = await getValue('settings.enableRichPresence');
    if(allowRichPresence) await startRichPresence();
  });

  onDestroy(async() => {
    await info("App close requested");
    const currentTime = await getValue("statistics.appOpenTime");
    const appCloseTime = Date.now() - appLaunchTime;
    await setValue("statistics.appOpenTime", currentTime + appCloseTime);
  })

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

<main class="dark:bg-primary-d bg-primary flex flex-row grow dark:text-white items-stretch w-screen h-screen overflow-y-hidden">
  <NavBar />
  <div class="flex-1 flex flex-col overflow-hidden w-screen h-screen p-5.5">
    {@render children?.()}
  </div>
</main>
