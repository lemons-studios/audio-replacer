<script lang="ts">
    import { pitchFilters, pitchFilterNames, effectFilters, effectFilterNames } from "../recordPage/AudioManager";
    import { getArprojProperty, projectLoaded, updateArprojStats } from "../../tools/ProjectHandler";
    import { PenTool } from "@lucide/svelte";
    import { selectFile } from "../../tools/OsTools";
    import { readTextFile } from "@tauri-apps/plugin-fs";
    import NoProjectLoaded from "../../Components/NoProjectLoaded.svelte";

    let pitchValues: string[] = $state([]);
    let pitchNames: string[] = $state([]);

    let effectValues: string[] = $state([]);
    let effectNames: string[] = $state([]);

    let selectedTab = $state(1);

    $effect(() => {
        pitchValues = pitchFilters;
        pitchNames = pitchFilterNames;

        effectValues = effectFilters;
        effectNames = effectFilterNames;
    });

    async function importData(importEffects: boolean, overwrite: boolean = false) {
        const key = importEffects ? "effectFilters" : "pitchFilters";

        const filePath = await selectFile(["json"], "JSON Files");
        if (!filePath) return;

        const obj = JSON.parse(await readTextFile(filePath));
        const validProperties = [];

        for (let i = 0; i < obj.length; i++) {
            const o = obj[i];
            if (o.hasOwnProperty("name") && o.hasOwnProperty("value")) {
                validProperties.push(o);
            }
        }

        if (!overwrite) {
            const currentValues = await getArprojProperty(key);
            validProperties.push(currentValues);
            validProperties.sort((a, b) => a.name.localeCompare(b.name));
        }
        await updateArprojStats(key, validProperties);
    }

    // TODO: Add edit effect function
</script>

{#if projectLoaded}
    <div class="card p-5 h-full">
        <div class="flex flex-col w-full rounded-lg justify-center items-center text-center gap-5">
            {#if selectedTab === 0}
                {#each pitchNames as p, index}
                    <div class=" flex flex-row items-center text-center justify-between min-h-15 bg-tertiary-d p-3 w-full rounded-lg">
                        <h2 class="ml-10 text-xl">{p}</h2>
                        <div class="gap-3 flex flex-row justify-center items-center">
                            <h2 class="text-center">
                                Pitch Multiplier: <p class="text-gray-500 text-sm">{pitchValues[index]}x</p>
                            </h2>
                            <button
                                class="hover:bg-accent focus:bg-accent-tertiary transition p-2 rounded-md"
                                onmouseleave={(e) => e.currentTarget.blur()}
                                onclick={(e) => e.currentTarget.blur()}
                                ><PenTool class="w-5 h-5"></PenTool></button>
                        </div>
                    </div>
                {/each}
            {:else if selectedTab === 1}
                {#each effectNames as e, index}
                    <div class="flex flex-row items-center text-center justify-between min-h-15 dark:bg-tertiary-d bg-tertiary p-3 w-full rounded-lg">
                        <h2>{e}</h2>
                        <div class="gap-3 flex flex-row justify-center items-center">
                            <h2 class="text-center">Effect Value: <p class="text-gray-500 text-sm">{effectValues[index]}</p></h2>
                            <button
                                class="hover:bg-accent focus:bg-accent-tertiary transition p-2 rounded-md"
                                onmouseleave={(e) => e.currentTarget.blur()}
                                onclick={(e) => e.currentTarget.blur()}>
                                <PenTool class="w-5 h-5"/>
                            </button>
                        </div>
                    </div>
                {/each}
            {/if}
        </div>
    </div>
{:else}
    <NoProjectLoaded />
{/if}
