<script lang="ts">
  import { onMount } from "svelte";
  import { startRichPresence } from "../tools/DiscordRpc";
  import { getValue, setValue } from "../tools/SettingsManager";
  import { setProjectData } from "../tools/ProjectManager";
  import { exists } from "@tauri-apps/plugin-fs";
  import { goto } from "$app/navigation";
  import { selectFolder } from "../tools/OsTools";
  import { basename } from "@tauri-apps/api/path";

  let previousProjectExists = $state(false);
  let previousProjectName = $state("None");
  let previousPath = $state("");
  let isProjectLoading = $state(false);

  onMount(async () => {
    previousPath = (await getValue("lastSelectedFolder")) as string;
    previousProjectExists = (await exists(previousPath)) || previousPath != "";
    if (previousProjectExists) {
      previousProjectName = await basename(previousPath);
    }
    await startRichPresence();
  });

  async function loadLastProject() {
    // Edge case where path exists on app load but no longer exists when trying to load project
    if (!exists(previousPath)) {
      return;
    }
    isProjectLoading = true;
    await setProjectData(previousPath);
    isProjectLoading = false;
    goto("/recordPage");
  }

  async function loadNewProject() {
    const res = await selectFolder();
    console.log(res);
    if (await exists(res)) {
      isProjectLoading = true;
      await setProjectData(res);
      setValue("lastSelectedFolder", res as string);
      isProjectLoading = false;
      goto("/recordPage");
    } else {
      // Show error toast
    }
  }
</script>

{#if isProjectLoading}
  <div class="flex flex-row grow justify-center items-center gap-3 absolute inset-0">
    <span class="loading loading-spinner text-primary text-xl"></span>
    <h3 class="text-xl">Loading Project...</h3>
  </div>
{/if}
{#if !isProjectLoading}
  <div class="flex flex-row gap-5 items-stretch h-full">
    <fieldset class="pane w-1/2 h-full">
      <legend class="fieldset-legend">Projects</legend>
      <div class="flex flex-col items-stretch gap-4 h-full">
        <fieldset class="secondary-pane h-full grow">
          <legend class="fieldset-legend">Load Last Project</legend>
          <h4 class="text-xl mb-5"><b>{previousProjectName}</b></h4>
          <button class="btn-primary btn btn-lg" onclick={loadLastProject}>Load</button>
        </fieldset>
        <fieldset class="secondary-pane h-full grow">
          <legend class="fieldset-legend">Load Other Project</legend>
          <button class="btn btn-primary btn-lg" onclick={loadNewProject}>Load</button>
        </fieldset>
      </div>
    </fieldset>
    <fieldset class="pane w-1/2">
      <legend class="fieldset-legend">Stats</legend>
      <p>Time Spent With A Project Open</p>
      <p>Total Files Accepted</p>
      <p>Total Files Skipped</p>
      <p>Total Files Discarded</p>
    </fieldset>
  </div>
{/if}
