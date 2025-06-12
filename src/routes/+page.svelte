<script lang="ts">
  import { onMount } from "svelte";
  import { JsonNetDataFromTag } from "../Util/NetworkUtils";
  import SvelteMarkdown from "@humanspeak/svelte-markdown";

  let markdown = $state("# No Changes Found");

  onMount(async() => {
    const url = "https://api.github.com/repos/lemons-studios/audio-replacer/releases/latest";
    const releaseData = await JsonNetDataFromTag(url, "body") ?? "# Error uh oh";
    markdown = releaseData;
  })

</script>

<div class="grid grid-cols-2 gap-5 h-full">
  <div class="grid grid-rows-2 gap-5">
    <div class="dark:bg-title-card-dark rounded-xl p-5 drop-shadow-lg">
        <h1 class="text-center align-top text-4xl m-10"><b>Load Project</b></h1>
        <div class="flex justify-center gap-10">
          <div class="dark:bg-secondary-card-dark">
            <h1>Last Project</h1>
          </div>
          
          <div>
            <h1>New Project</h1>
          </div>
        </div>
    </div>
    <div class="dark:bg-title-card-dark rounded-xl p-5 drop-shadow-lg">
        <h1 class="text-center align-top text-4xl mb-10"><b>Stats</b></h1>
        <p>Time Spent With Project Open</p>
        <p>Files Transcribed</p>
        <p>Files Skipped</p>
        <p>Recordings cancelled/discarded</p>
    </div>
  </div>
  <div class="rounded-xl dark:bg-title-card-dark p-5 drop-shadow-lg">
    <h1 class="text-center align-top text-4xl mb-5"><b>Latest Changes</b></h1>
    <SvelteMarkdown source={markdown}></SvelteMarkdown>
  </div>
</div>