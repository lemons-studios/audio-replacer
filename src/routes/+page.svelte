<script lang="ts">
  import { onMount } from "svelte";
  import SvelteMarkdown from "@humanspeak/svelte-markdown";
  import { startRichPresence } from "../util/DiscordRpc";
  import { getValue } from "../util/SettingsManager";
  import { setProjectData } from "../util/ProjectManager";
  import { exists } from "@tauri-apps/plugin-fs";
  import { goto } from "$app/navigation";
  import { selectFolder } from "../util/OsTools";

  let previousProjectExists = $state(false);
  let previousProjectName = $state("None")
  let previousPath = "";

  onMount(async() => {
    previousPath = await getValue("lastSelectedFolder") as unknown as string;
    previousProjectExists = await exists(previousPath) || previousPath == "";
    await startRichPresence();
  })

  async function loadLastProject() {
    // Edge case where path exists on app load but no longer exists when trying to load project
    if(!exists(previousPath)) {
      return;
    }

    await setProjectData(previousPath)
    goto("/recordPage");
  }

  async function loadNewProject() {
    const res = await selectFolder();
    await setProjectData(res);
    goto("/recordPage");
  }
</script>

<div class="flex flex-row gap-5 items-stretch h-full">
  <div class="card w-1/2">
    <h1 class="title-text m-10"><b>Load Project</b></h1>
    <div class="flex grow flex-col items-stretch gap-4 h-full">
      <div class="secondary-card h-full text-center items-center">
        <h3 class="font-semibold text-2xl">Load Last Project</h3>
        <h4 class="tertiary-text">{previousProjectName}</h4>
        <button class="menu-button" onclick={loadLastProject}>Load</button>
      </div>
      <div class="secondary-card h-full">
        <h3 class="font-semibold text-center text-2xl">Load New Project</h3>
        <button class="menu-button" onclick={loadNewProject}>Load</button>
      </div>
    </div>
  </div>
  <div class="card w-1/2">
    <h1 class="title-text mb-10"><b>Stats</b></h1>
    <p>Time Spent With A Project Open</p>
    <p>Total Files Transcribed</p>
    <p>Total Files Skipped</p>
    <p>Total Recordings Discarded</p>
  </div>
</div>
