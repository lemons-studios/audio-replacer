<script lang="ts">
  import { onMount } from "svelte";
  import { startRichPresence } from "../app/DiscordRpc";
  import { getValue, setValue } from "../tools/SettingsManager";
  import { setProjectData } from "../app/ProjectManager";
  import { exists } from "@tauri-apps/plugin-fs";
  import { goto } from "$app/navigation";
  import { selectFolder } from "../tools/OsTools";
  import { basename } from "@tauri-apps/api/path";

  let previousProjectExists = $state(false);
  let previousProjectName = $state("None")
  let previousPath = $state("");

  onMount(async() => {
    previousPath = await getValue("lastSelectedFolder") as unknown as string;
    previousProjectExists = await exists(previousPath) || previousPath == "";
    if(previousProjectExists) {
      previousProjectName = await basename(previousPath);
    }
    await startRichPresence();
  })

  async function loadLastProject() {
    // Edge case where path exists on app load but no longer exists when trying to load project
    if(!exists(previousPath)) {
      return;
    }

    await setProjectData(previousPath);
    goto("/recordPage");
  }

  async function loadNewProject() {
    const res = await selectFolder();
    console.log(res);
    await setProjectData(res);
    await setValue("lastSelectedFolder", res as string);
    goto("/recordPage");
  }
</script>

<div class="flex flex-row gap-5 items-stretch h-full">
  <div class="card w-1/2">
    <h1 class="title-text m-10"><b>Load Project</b></h1>
    <div class="flex grow flex-col items-stretch gap-4 h-full">
      <div class="secondary-card h-full text-center items-center">
        <h3 class="font-semibold text-2xl">Load Last Project</h3>
        <h4 class="tertiary-text mb-5">{previousProjectName}</h4>
        <button class="btn-primary btn btn-lg" onclick={loadLastProject}>Load</button>
      </div>
      <div class="secondary-card h-full">
        <h3 class="font-semibold text-center text-2xl mb-5">Load New Project</h3>
        <button class="btn btn-primary btn-lg" onclick={loadNewProject}>Load</button>
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
