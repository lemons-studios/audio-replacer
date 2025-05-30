<script lang="ts">
    import { invoke } from "@tauri-apps/api/core";
    import { JsonNetDataFromTag } from "../util/NetworkUtils";
    import { getSystemTime, getUsername, getAccentColor } from "../util/OsData";
    import { onMount } from "svelte";
    import type { Renderers, SvelteMarkdownOptions } from "@humanspeak/svelte-markdown";
    import SvelteMarkdown from "@humanspeak/svelte-markdown";

    let username: string = "User";

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

<div>
    <p class="text-5xl text-center mb-10">Good {getSystemTime()} {username}</p>
    <div class="flex gap-5 p-5 overflow-hidden">
        <div class="w-1/2-screen secondary-container small-elevate p-5 padding">
            <p class="text-4xl text-center mb-15">Load Project</p>
        </div>
        <div class=" secondary-container small-elevate p-5 padding">
            <p class="text-4xl text-center mb-15">Latest Changes</p>
        </div>
    </div>
</div>

