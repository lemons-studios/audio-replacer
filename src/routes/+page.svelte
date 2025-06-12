<script lang="ts">
  import { initializeValues } from "../generic/AppProperties";
  import { onMount } from "svelte";
  import { JsonNetDataFromTag } from "../util/NetworkUtils";
  import SvelteMarkdown from "@humanspeak/svelte-markdown";
  import { json } from "@sveltejs/kit";

  let markdown: string = `No recent changes`;

  onMount(async() => {
    const url = "https://api.github.com/repos/lemons-studios/audio-replacer/releases/latest";
    const releaseData = await JsonNetDataFromTag(url, "body") ?? "# Error uh oh";
    markdown = JSON.parse(releaseData).body;
  })

</script>



<div class="grid grid-cols-2 gap-5 h-full">
  <div class="grid grid-rows-2 gap-5">
    <div class="dark:bg-title-card-dark rounded-xl p-5 drop-shadow-lg">
        <h1 class="text-center align-top text-4xl"><b>Load Project</b></h1>

    </div>
    <div class="dark:bg-title-card-dark rounded-xl p-5 drop-shadow-lg">
        <h1 class="text-center align-top text-4xl"><b>Stats</b></h1>
    </div>
  </div>
  <div class="rounded-xl dark:bg-title-card-dark p-5 drop-shadow-lg">
    <h1 class="text-center align-top text-4xl mb-5"><b>Latest Changes</b></h1>
    <SvelteMarkdown source={markdown}></SvelteMarkdown>
  </div>
</div>