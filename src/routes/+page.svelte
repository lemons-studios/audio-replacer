<script lang="ts">
    import { invoke } from "@tauri-apps/api/core";
    import { JsonNetDataFromTag } from "../util/NetworkUtils";
    import { getSystemTime, getUsername } from "../util/OsData";
    import { onMount } from "svelte";
    import type { Renderers, SvelteMarkdownOptions } from "@humanspeak/svelte-markdown";
    import SvelteMarkdown from "@humanspeak/svelte-markdown";
    import { extraData } from "../util/AppProperties";

    let username: string = "User";
    let folder: string = extraData;

    async function getReleaseLogs() {
        let md = await JsonNetDataFromTag("https://api.github.com/repos/lemons-studios/audio-replacer/releases/latest", "body");
        return md;
    }

    onMount(async() => {
        try {
            username = "getting username..."
            username = await getUsername();
        } catch (e) {
            console.error(e);
            username = "E-User " + e;
        }
    });

</script>

  <div class="grid">
    <div class="s12 m12 l6">
      <h3>{username}</h3>
    </div>
    <div class="s12 m12 l6">
      <h3>{folder}</h3>
    </div>
    <div class="s12 m12 l6">
      <h3>Pane 3</h3>
    </div>
    <div class="s12 m12 l6">
      <h3>Pane 4</h3>
    </div>
  </div>

