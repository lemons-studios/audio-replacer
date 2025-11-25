<script lang="ts">
    import { pitchFilters, pitchFilterNames, effectFilters, effectFilterNames } from "../recordPage/AudioManager";
    import { onMount } from "svelte";
    import { projectLoaded } from "../../tools/ProjectHandler";
    import { goto } from "$app/navigation";
    import { selectFile } from "../../tools/OsTools";
    import { exists } from "@tauri-apps/plugin-fs";
    let pitchValues: string[] = $state([]);
    let pitchNames: string[] = $state([]);

    let effectValues: string[] = $state([]);
    let effectNames: string[] = $state([]);

    $effect(() => {
        pitchValues = pitchFilters;
        pitchNames = pitchFilterNames;
        
        effectValues = effectFilters;
        effectNames = effectFilterNames;
    });

    onMount(async() => {

    });

</script>

<div class="card h-full">
    {#if projectLoaded}
    {:else}
        <div class="flex flex-col justify-center text-center h-full">
            <h1 class="text-2xl">You don't have a project Loaded!</h1>
            <div class="flex flex-row justify-center mt-5 gap-3">
                <button class="app-btn" onclick={async() => {
                    const file = await selectFile(["arproj"]);
                    if(await exists(file)) {

                    }
                }}>Load project</button>
                <button class="app-btn" onclick={() => goto("/")}>Return Home</button>
            </div>
        </div>
    {/if}
</div>