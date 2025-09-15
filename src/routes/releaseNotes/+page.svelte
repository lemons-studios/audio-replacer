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

<style>
  * {
    overflow: scroll;
    overflow-x: hidden;
  }

  ::-webkit-scrollbar {
    width: 0;
    background: transparent;
  }

  ::-webkit-scrollbar-thumb {
    background: transparent;
  }
</style>

<div class="flex items-stretch flex-grow justify-center scrollbar-hide" style="overflow:auto">
  <div class="prose">
    <SvelteMarkdown source={markdown} class="dark:prose-invert text-center w-screen" />
  </div>
</div>


