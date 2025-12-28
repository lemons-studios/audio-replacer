<script lang="ts">
  import { getValue, setValue } from "../tools/DataInterface";
  import { onMount } from "svelte";
  import { setPresenceDetails } from "../tools/DiscordPresenceManager";
  import { exists, readTextFile, writeTextFile } from "@tauri-apps/plugin-fs";
  import {format, saveFile, selectFile, selectFolder, sleep, timestampToLegible} from "../tools/OsTools";
  import { createArProj, setActiveProject, updateArprojStats } from "../tools/ProjectHandler";
  import { goto } from "$app/navigation";
  import { initializeData } from "../tools/DataInterface";
  import { ArrowRight, Save, FilePlus, BookOpenText } from "@lucide/svelte";
  import Notification from "../Components/Notification.svelte";
  import { openUrl } from "@tauri-apps/plugin-opener";
  import Dropdown from "../Components/Dropdown.svelte";

  let recentProjectPaths: string[] = $state([]);
  let recentProjectObjs: any[] = $state([]);
  let notificationManager: Notification;

  const statistics = [
    {
      name: "Time with Audio Replacer Open",
      getValue: async(): Promise<string> => {
        const rawTime = await getValue("statistics.appOpenTime"); // Time statistic will be measured in seconds
        return rawTime === 0 ? '0 Hours' : `${(rawTime / 3600).toFixed(1)} Hours`; // Similar to how Steam displays time played in games
      }
    },
    {
      name: "Files Recorded",
      getValue: async(): Promise<string> => {
        return format(await getValue("statistics.filesRecorded"));
      }
    },
    {
      name: "Files Accepted",
      getValue: async(): Promise<string> => {
        return format(await getValue('statistics.filesAccepted'));
      }
    },
    {
      name: "Files Rejected",
      getValue: async(): Promise<string> => {
        return format(await getValue('statistics.filesRejected'));
      }
    },
    {
      name: "Files Skipped",
      getValue: async(): Promise<string> => {
        return format(await getValue('statistics.filesSkipped'));
      }
    },
    {
      name: "Recordings Cancelled",
      getValue: async(): Promise<string> => {
        return format(await getValue('statistics.recordingsCancelled'));
      }
    }
  ] as const;

  onMount(async() => {
    await initializeData();
    await setPresenceDetails("Home Page");

    recentProjectPaths = await getValue("settings.recentProjectPaths");
    for(let i = 0; i < recentProjectPaths.length; i++) {
      recentProjectObjs.push(JSON.parse(await readTextFile(recentProjectPaths[i])));
    }
  });

  async function loadProject(path: string) {
    notificationManager.addToNotification('progress', "Loading", "", false, 150000);
    await setActiveProject(path);
    await updateArprojStats("lastOpened", Date.now());
    await goto("/recordPage");
  }

  async function newProject() {
    const folder = await selectFolder();
    if(await exists(folder)) {
      notificationManager.addToNotification('progress', "Loading", "", false, 150000);
      const arProj = await createArProj(folder);
      const path = await saveFile(["arproj"], "Audio Replacer Project");
      if(typeof path === 'string') {
        recentProjectPaths.push(path);
        recentProjectObjs.push(arProj);
        await writeTextFile(path, JSON.stringify(arProj));
        await setValue('settings.recentProjectPaths', recentProjectPaths);

        await setActiveProject(path);
        await goto("/recordPage");
      }
    }
  }
  
</script>
<!--Adding this #if block fixes a strange clipping/ghosting issue-->
{#if true}
  <div class="notification-overlay">
    <Notification bind:this={notificationManager} />
  </div>
{/if}

<div class="flex flex-col gap-3 h-full w-full p-3">
  <div class="flex flex-row gap-5 h-full">
    <div class="flex flex-col w-1/2 h-full card rounded-xl p-3">
      <h1 class="text-center text-3xl font-medium">Projects</h1>
      <div class="flex flex-col text-center justify-between items-center mt-15 h-full gap-y-3">
        {#if recentProjectObjs.length === 0}
          <h1 class="text-center text-gray-400">No Recent Projects</h1>
        {:else}
          <div class="w-full overflow-y-auto">
            {#each recentProjectObjs as rp, i}
              <button class="save-btn rounded-sm w-full mb-1 dark:hover:bg-tertiary-d hover:bg-tertiary dark:focus:bg-tertiary-d dark:focus:drop-shadow-xl transition duration-75"
                      onclick={async() => await loadProject(recentProjectPaths[i])}
                      onmouseleave={(e) => e.currentTarget.blur()}
                      onmouseup={(e) => e.currentTarget.blur()}>
                <div class="flex justify-between items-center text-left p-3 mb-1.5 mt-1.5">
                  <div class="flex flex-col">
                    <p class="text-lg">{rp.name}</p>
                    <p class="text-gray-400 text-sm">Last Opened: {timestampToLegible(rp.lastOpened)}</p>
                    <p class="text-gray-400 text-xs">Files Remaining: {format(rp.filesRemaining)}</p>
                  </div>
                  <ArrowRight class="arrow w-5 h-5"/>
                </div>
              </button>
            {/each}
          </div>
        {/if}
        <div class="flex row w-max mb-1 h-auto p-2 rounded-lgjustify-end align-bottom items-end gap-x-5">
          <button class="text-center p-1.5 flex flex-row items-center justify-center gap-2 hover:bg-accent focus:bg-accent-tertiary rounded-md transition border-white/10 border"
                  onclick={newProject} onmouseleave={(e) => e.currentTarget.blur()}
                  onmouseup={(e) => e.currentTarget.blur()}>
            <FilePlus class="w-4 h-4" />New Project
          </button>
          <button class="text-center p-1.5 flex flex-row items-center justify-center gap-2 hover:bg-accent focus:bg-accent-tertiary rounded-md transition border-white/10 border"
                  onclick={async() => {
                  const file = await selectFile(["arproj"], "Audio Replacer Project Files");
                  const project = JSON.parse(await readTextFile(file));
                  recentProjectPaths.push(file);
                  recentProjectObjs.push(project);
                  await setValue('settings.recentProjectPaths', recentProjectPaths);
                  await loadProject(file);
                  }}
                  onmouseleave={(e) => e.currentTarget.blur()}
                  onmouseup={(e) => e.currentTarget.blur()}>
            <Save class="w-4 h-4" /> Load Project
          </button>
        </div>
      </div>
    </div>
    <div class="flex flex-col gap-y-5 w-1/2">
      <div class="card h-1/2 rounded-xl p-3 flex flex-col justify-center items-center">
        <h1 class="text-center text-3xl font-medium mb-5">New To Audio Replacer?</h1>
        <button class="app-btn"
                onmouseleave={(e) => e.currentTarget.blur()}
                onmouseup={(e) => e.currentTarget.blur()}
                onclick={async() => {await openUrl("https://github.com/lemons-studios/audio-replacer/wiki")}}><BookOpenText class="button-icon"/> Check Out The Wiki!</button>
      </div>
      <div class="card h-1/2 p-3 rounded-xl">
        <h1 class="text-center text-3xl font-medium mb-2">Statistics</h1>
        {#each statistics as stat}
          <div class="flex flex-row align-middle items-center justify-between text-left mb-2.5">
            <h2>{stat.name}</h2>
            {#await stat.getValue() then value}
              <p>{value}</p>
            {/await}
          </div>
        {/each}
      </div>
    </div>
  </div>
</div>

<style>
  .save-btn:hover {
    background-color: oklch(0.3042 0.0051 325.78);
    box-shadow: none;
  }

  .save-btn:focus {
    background-color: oklch(1 0 0 / 10%);
    box-shadow: inset 0 0 1em oklch(0.1929 0.0048 325.72 / 60%);
  }
</style>
