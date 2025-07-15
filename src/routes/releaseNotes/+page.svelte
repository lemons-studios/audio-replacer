<script lang="ts">
  import { onMount } from "svelte";
  import SvelteMarkdown from "@humanspeak/svelte-markdown";
  import { jsonNetDataFromTag } from "../../tools/NetworkUtils";

  let markdown = $state("# No release data found\n(Are you online?)");

  onMount(async () => {
    const releaseUrl ="https://api.github.com/repos/lemons-studios/audio-replacer/releases/latest";
    const releaseData =(await jsonNetDataFromTag(releaseUrl, "body")) ?? "# Error Fetching Release Data (Are You Online?)";
    markdown = releaseData;
  });
</script>
<div class="card h-full w-full">
    <h1 class="title-text"><b>Latest Release Notes:</b></h1>
    <SvelteMarkdown source={markdown} class="prose dark:prose-invert"/>
</div>
