<script lang="ts">
    import { invoke } from "@tauri-apps/api/core";
    import { NetworkUtils } from "../util/NetworkUtils";
    import type { Renderers, SvelteMarkdownOptions } from "@humanspeak/svelte-markdown";
    import SvelteMarkdown from "@humanspeak/svelte-markdown";

    function getSystemTime() {
        const date = new Date();
        const hours = date.getHours();
        return hours >= 5 && hours < 12 ? "Morning" : hours >= 12 && hours < 18 ? "Afternoon" : "Evening"; 
    }

    async function getUsername() {
        const res = await invoke("get_username");
        return res;
    }

    async function getReleaseLogs() {
        let md = await JsonNetDataFromTag("https://api.github.com/repos/lemons-studios/audio-replacer/releases/latest", "body");
        return md;
    }
</script>

<div>
    <p class="text-5xl text-center mb-10">Good {getSystemTime()}</p>
    <div class="flex gap-5 p-5 overflow-hidden">
        <div class="w-1/2-screen secondary-container small-elevate p-5 padding">
            <p class="text-4xl text-center mb-15">Projects</p>
        </div>
        <div class=" secondary-container small-elevate p-5 padding">
            <p class="text-4xl text-center mb-15">Latest Changes</p>
        </div>
    </div>
</div>

