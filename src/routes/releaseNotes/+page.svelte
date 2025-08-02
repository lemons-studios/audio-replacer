<script lang="ts">
  import { onMount } from "svelte";
  import SvelteMarkdown from "@humanspeak/svelte-markdown";
  import { resolveResource } from "@tauri-apps/api/path";
  import { readTextFile } from "@tauri-apps/plugin-fs";

  let markdown = $state("wuh oh");

  onMount(async () => {
    const releasePath = await resolveResource("resources/releaseData.md");
    const releaseData = await readTextFile(releasePath);
    markdown = releaseData;
  });
</script>

<div class="flex flex-col justify-center">
  <div class="prose text-center text-xl justify-center">
    <h1 class="title-text text-center"><b>Latest Release Notes:</b></h1>
    <SvelteMarkdown source={markdown} class="prose dark:prose-invert" />
  </div>
</div>
