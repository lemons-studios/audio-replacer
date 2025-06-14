<script lang="ts">
  import { onMount } from "svelte";
  import { jsonNetDataFromTag } from "../Util/NetworkUtils";
  import SvelteMarkdown from "@humanspeak/svelte-markdown";
  import { startRichPresence } from "../Util/DiscordRpc";
  import { getValue } from "../util/SettingsManager";
  import { setProjectData } from "../util/ProjectManager";
  import { exists } from "@tauri-apps/plugin-fs";
  import { goto } from "$app/navigation";
  import { selectFolder } from "../util/OsTools";

  let markdown = $state("# No Changes Found");
  let previousProjectExists = $state(false);
  let previousPath = "";

  onMount(async() => {
    const url = "https://api.github.com/repos/lemons-studios/audio-replacer/releases/latest";
    const releaseData = await jsonNetDataFromTag(url, "body") ?? "# Error Fetching Release Data (Are You Online?)";
    markdown = releaseData;

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
    console.log(res);
  }
</script>

<div class="grid grid-cols-2 gap-5 h-full">
  <div class="grid grid-rows-2 gap-5">
    <div class="dark:bg-surface-container-dark rounded-xl p-5 drop-shadow-lg">
        <h1 class="text-center align-top text-4xl m-10"><b>Load Project</b></h1>
        <div class="flex justify-center gap-10">
          <div class="dark:bg-tertiary-container-dark lg:w-45 h-40 p-2.5 rounded-sm">
            <h1 class="text-center">Last Project</h1>
            <h3 class="text-center text-xs text-gray-300 mb-5">Project Name</h3>
            <md-filled-button class="w-40 p-2.5"><p class="font-icons">folder_open</p> Load Project</md-filled-button>
          </div>
          <div class="dark: bg-tertiary-container-dark w-45 h-40 p-2.5 rounded-sm drop-shadow-lg">
            <h1 class="text-center mb-9.5">New Project</h1>
            <md-filled-button on:click={loadNewProject} class="w-40 p-2.5"><p class="font-icons">add_circle</p>Start New Project</md-filled-button>
          </div>
        </div>
    </div>
    <div class="dark:bg-surface-container-dark rounded-xl p-5 drop-shadow-lg">
        <h1 class="text-center align-top text-4xl mb-10"><b>Stats</b></h1>
        <p>Time Spent With Project Open</p>
        <p>Files Transcribed</p>
        <p>Files Skipped</p>
        <p>Recordings cancelled/discarded</p>
    </div>
  </div>
  <div class="rounded-xl dark:bg-surface-container-dark p-5 drop-shadow-lg">
    <h1 class="text-center align-top text-4xl mb-5"><b>Latest Changes</b></h1>
    <div class="prose dark:prose-invert">
      <SvelteMarkdown source={markdown} />
    </div>
  </div>
</div>