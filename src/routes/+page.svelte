<script lang="ts">
  import { onMount, tick } from "svelte";
  import { getValue, setValue } from "../tools/SettingsManager";
  import { setProjectData } from "../tools/ProjectManager";
  import { exists } from "@tauri-apps/plugin-fs";
  import { goto } from "$app/navigation";
  import { selectFolder } from "../tools/OsTools";
  import { basename } from "@tauri-apps/api/path";
  import { info, error } from "@tauri-apps/plugin-log";
  import { invoke } from "@tauri-apps/api/core";

  let previousProjectExists = $state(false);
  let previousProjectName = $state("No Previous Project");
  let user = $state("");
  let previousPath = $state("");
  let isProjectLoading = $state(false);

  onMount(async () => {
    await tick();
    user = await invoke('get_username');
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
    if (!await exists(previousPath)) {
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
      error(`${res} Does not exist`);
    }
  }
</script>

{#if isProjectLoading}
  <!--Add Load Spinner Component here-->
  <span>Loading Project...</span>
{/if}

{#if !isProjectLoading}
  <div class="h-full">
    <h1 class="text-center text-3xl mb-5">Welcome Back, {user}</h1>
    <div class="w-full h-auto border-accent border rounded-md border-2 p-4 drop-shadow-lg drop-shadow-accent-shadow dark:bg-primary-d bg-primary">
      <h1>adsaoijdaosidj</h1>
    </div>
  </div>
{/if}
