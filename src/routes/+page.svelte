<script lang="ts">
  import { onMount, tick } from "svelte";
  import { getValue, setValue } from "../tools/SettingsManager";
  import { setProjectData } from "../tools/ProjectManager";
  import { exists } from "@tauri-apps/plugin-fs";
  import { goto } from "$app/navigation";
  import { selectFolder } from "../tools/OsTools";
  import { basename } from "@tauri-apps/api/path";
  import { info } from "@tauri-apps/plugin-log";

  let previousProjectExists = $state(false);
  let previousProjectName = $state("No Previous Project");
  let previousPath = $state("");
  let isProjectLoading = $state(false);

  onMount(async () => {
    await tick();
    previousPath = (await getValue("lastSelectedFolder")) as string;
    previousProjectExists = (await exists(previousPath)) || previousPath != "" || previousPath == null;
    if (previousProjectExists) {
      previousProjectName = await basename(previousPath);
      
      // Autoload
      const autoload = await getValue("autoloadProject");
      if(autoload) {
        await loadLastProject();
      }
    }
    info("App Loaded!");
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
  <div class="flex flex-row gap-5 items-stretch h-full justify-center">
    <fieldset class="pane w-1/2 h-full content-center">
      <legend class="fieldset-legend">Load Last Project</legend>
      <h4 class="text-xl mb-2.5 text-center font-bold">{previousProjectName}</h4>
      <button class={`btn-primary btn btn-lg ${previousProjectExists ? '' : 'btn-disabled'}`} onclick={loadLastProject}>Load</button>
    </fieldset>
    <fieldset class="pane w-1/2 h-full content-center">
      <legend class="fieldset-legend">Load Other Project</legend>
      <h4 class="text-xl mb-2.5 text-center font-bold">Start A New Project!</h4>
      <button class="btn btn-primary btn-lg" onclick={loadNewProject}>Load</button>
    </fieldset>
  </div>
{/if}
