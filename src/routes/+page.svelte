<script lang="ts">
  import {getValue, setValue} from "../tools/DataInterface";
  import {onMount, tick} from "svelte";
  import { setPresenceDetails } from "../tools/DiscordPresenceManager";
  import { exists, readTextFile, writeTextFile } from "@tauri-apps/plugin-fs";
  import {saveFile, selectFolder, sleep, timestampToLegible} from "../tools/OsTools";
  import { createArProj, setActiveProject, updateArprojStats } from "../tools/ProjectHandler";
  import { goto } from "$app/navigation";
  import IconArrowRightRegular from "phosphor-icons-svelte/IconArrowRightRegular.svelte"
  import Modal from "../Components/Modal.svelte";
  import {initializeData} from "../tools/DataInterface";

  let recentProjectPaths: string[] = $state([]);
  let recentProjectObjs: any[] = $state([]);

  const format = (x: number) => {
    return new Intl.NumberFormat().format(x);
  }

  const statistics = [
    {
      name: "Time with Audio Replacer Open",
      getValue: (): string => {
        const rawTime = getValue("statistics.appOpenTime"); // Time statistic will be measured in seconds
        return rawTime === 0 ? '0 Hours' : `${(rawTime / 3600).toFixed(1)} Hours`; // Similar to how Steam displays time played in games
      }
    },
    {
      name: "Files Recorded",
      getValue: (): string => {
        return format(getValue("statistics.filesRecorded"));
      }
    },
    {
      name: "Files Accepted",
      getValue: (): string => {
        return format(getValue('statistics.filesAccepted'));
      }
    },
    {
      name: "Files Rejected",
      getValue: (): string => {
        return format(getValue('statistics.filesRejected'));
      }
    },
    {
      name: "Files Skipped",
      getValue: (): string => {
        return format(getValue('statistics.filesSkipped'));
      }
    },
    {
      name: "Recordings Cancelled",
      getValue: (): string => {
        return format(getValue('statistics.recordingsCancelled'));
      }
    }
  ] as const;

  onMount(async() => {
    await tick();
    await setPresenceDetails("Home Page");
    await initializeData();

    recentProjectPaths = await getValue("settings.recentProjectPaths");
    for(let i = 0; i < recentProjectPaths.length; i++) {
      recentProjectObjs.push(JSON.parse(await readTextFile(recentProjectPaths[i])));
    }
  });

  async function loadProject(path: string) {
    await setActiveProject(path);
    await updateArprojStats("lastOpened", Date.now());
    await goto("/recordPage");
  }

  async function newProject() {
    const folder = await selectFolder();
    if(await exists(folder)) {
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

<div class="flex flex-col gap-3 h-full w-full p-3">
  <div class="flex flex-row gap-5 h-full">
    <div class="flex flex-col w-1/2 card rounded-xl p-3">
      <h1 class="text-center text-3xl font-medium">Projects</h1>
      <div class="flex flex-col justify-apart text-center items-center mt-15 h-full gap-y-3">
        {#if recentProjectObjs.length === 0}
          <h1>No Recent Projects</h1>
        {:else}
        {#each recentProjectObjs as rp, i}
          <button class="save-btn rounded-sm w-full dark:hover:bg-tertiary-d hover:bg-tertiary dark:focus:bg-tertiary-d dark:focus:drop-shadow-xl transition duration-75"
                  onclick={async() => await loadProject(recentProjectPaths[i])}
                  onmouseleave={(e) => e.currentTarget.blur()}
                  onmouseup={(e) => e.currentTarget.blur()}>
            <div class="flex justify-between items-center text-left p-3 mb-1.5 mt-1.5">
              <div class="flex flex-col">
                <p class="text-lg">{rp.name}</p>
                <p class="text-gray-400 text-sm">Last Opened: {timestampToLegible(rp.lastOpened)}</p>
                <!--<p class="text-gray-400 text-xs">Files Remaining: {Intl.NumberFormat().format(rp.fileCount)}</p>-->
              </div>
              <IconArrowRightRegular class="arrow w-5 h-5"></IconArrowRightRegular>
            </div>
          </button>
        {/each}
        {/if}
        <button class="nav-btn bg-primary-d" onclick={newProject} onmouseleave={(e) => e.currentTarget.blur()} onmouseup={(e) => e.currentTarget.blur()}>New Project</button>
      </div>
    </div>
    <div class="flex flex-col gap-y-5 w-1/2">
      <div class="card h-1/2 rounded-xl p-3">
        <h1 class="text-center text-3xl font-medium mb-5">Tutorials</h1>
      </div>
      <div class="card h-1/2 p-3 rounded-xl">
        <h1 class="text-center text-3xl font-medium mb-2">Statistics</h1>
        {#each statistics as stat}
        <div class="flex flex-row align-middle items-center justify-between text-left mb-2.5">
          <h2>{stat.name}</h2>
          <p>{stat.getValue()}</p>
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
  .arrow {
    position: fixed;
  }
</style>
