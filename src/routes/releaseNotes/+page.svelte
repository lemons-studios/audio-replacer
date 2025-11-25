<script lang="ts">
  import {onMount} from "svelte";
  import SvelteMarkdown from "@humanspeak/svelte-markdown";
  import {resolveResource} from "@tauri-apps/api/path";
  import {readTextFile} from "@tauri-apps/plugin-fs";
  import {setPresenceDetails} from "../../tools/DiscordPresenceManager";

  let markdown = $state("wuh oh");

  onMount(async () => {
    const releasePath = await resolveResource("resources/releaseData.md");
    markdown = await readTextFile(releasePath);

    await setPresenceDetails("Viewing Release Notes")
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

<div class="flex items-stretch grow text-white justify-center scrollbar-hide" style="overflow:auto">
  <div class="prose">
    <SvelteMarkdown source={markdown} class="dark:prose-invert text-center w-screen" />
  </div>
</div>


