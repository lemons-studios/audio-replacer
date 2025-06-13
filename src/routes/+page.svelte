<script lang="ts">
  import { onMount } from "svelte";
  import { JsonNetDataFromTag } from "../Util/NetworkUtils";
  import SvelteMarkdown from "@humanspeak/svelte-markdown";
  import { startRichPresence } from "../Util/DiscordRpc";

  let markdown = $state("# No Changes Found");

  onMount(async() => {
    const url = "https://api.github.com/repos/lemons-studios/audio-replacer/releases/latest";
    const releaseData = await JsonNetDataFromTag(url, "body") ?? "# Error Fetching Release Data (Are You Online?)";
    markdown = releaseData;
    await startRichPresence();
  })

</script>

<div class="grid grid-cols-2 gap-5 h-full">
  <div class="grid grid-rows-2 gap-5">
    <div class="dark:bg-surface-container-dark rounded-xl p-5 drop-shadow-lg">
        <h1 class="text-center align-top text-4xl m-10"><b>Load Project</b></h1>
        <div class="flex justify-center gap-10">
          <div class="dark:bg-tertiary-container-dark w-60 h-40 p-2.5 rounded-sm">
            <h1 class="text-center">Last Project</h1>
            <h3 class="text-center text-xs text-gray-300 mb-5">Project Name</h3>
            <md-filled-tonal-button class="w-40 p-2.5"><p class="font-icons">folder_open</p> Load Project</md-filled-tonal-button>
          </div>
          <div class="border-white h-20">

          </div>
          <div class="dark: bg-tertiary-container-dark w-60 h-40 p-2.5 rounded-sm drop-shadow-lg">
            <h1 class="text-center mb-9.5">New Project</h1>
            <md-filled-tonal-button class="w-40 p-2.5"><p class="font-icons">add_circle</p>Start New Project</md-filled-tonal-button>
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