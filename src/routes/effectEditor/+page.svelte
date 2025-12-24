<script lang="ts">
    import { pitchFilters, pitchFilterNames, effectFilters, effectFilterNames } from "../recordPage/AudioManager";
    import { getArprojProperty, projectLoaded, updateArprojStats } from "../../tools/ProjectHandler";
    import { PenTool } from "@lucide/svelte";
    import { selectFile } from "../../tools/OsTools";
    import { readTextFile } from "@tauri-apps/plugin-fs";
    import { AudioLines, WandSparkles, CirclePlus } from "@lucide/svelte";
    import NoProjectLoaded from "../../Components/NoProjectLoaded.svelte";
    import EffectModal from "./EffectModal.svelte";
    import {mount, unmount} from "svelte";

    let pitchValues: string[] = $state([]);
    let pitchNames: string[] = $state([]);

    let effectValues: string[] = $state([]);
    let effectNames: string[] = $state([]);

    let selectedTab = $state(0);
    let currentModal: null | EffectModal = null;

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

    function editEffect(selectedIndex: null | number) {
        if(currentModal) {
            unmount(currentModal);
            currentModal = null;
        }

        const isEffect = selectedTab === 1
        const target = document.body;
        currentModal = mount(EffectModal, {
            target,
            props: {
                isEffect: isEffect,
                selectedIndex: selectedIndex
            }
        });
    }
</script>

{#if projectLoaded}
    <div class="flex flex-row w-full justify-around px-4 py-2 gap-3 min-h-15 card mb-5">
        <button
            class="w-1/2 text-center p-1.5 flex flex-row items-center justify-center gap-2 hover:bg-accent focus:bg-accent-secondary dark:focus:bg-accent-tertiary rounded-md transition"
            onclick={(e) => {e.currentTarget.blur(); selectedTab = 0}}
            onmouseleave={(e) => {e.currentTarget.blur()}}>
            <AudioLines class="button-icon"/> Pitch Modifiers
        </button>
        <button
            class="w-1/2 text-center p-1.5 flex flex-row items-center justify-center gap-2 hover:bg-accent focus:bg-accent-secondary dark:focus:bg-accent-tertiary rounded-md transition"
            onclick={(e) => {e.currentTarget.blur(); selectedTab = 1}}
            onmouseleave={(e) => {e.currentTarget.blur()}}>
            <WandSparkles class="button-icon"/> Effect Filters</button>
    </div>
    <div class="card p-5 h-full">
        <div class="flex flex-col w-full rounded-lg justify-center items-center text-center gap-5">
            {#each (selectedTab === 0 ? pitchNames : effectNames) as name, index}
                <div class="flex flex-row items-center text-center justify-between min-h-17 dark:bg-tertiary-d bg-tertiary p-3 w-full rounded-lg">
                    <h2>{name}</h2>
                    <div class="gap-3 flex flex-row justify-center items-center">
                        {#if selectedTab === 0}
                            <h2>{pitchValues[index]}x</h2>
                        {:else}
                            <h2 class="text-center">Effect Value: <p class="text-gray-500 text-sm">{effectValues[index]}</p></h2>
                        {/if}
                        <button class="hover:bg-accent focus:bg-accent-secondary dark:focus:bg-accent-tertiary transition p-2 rounded-md"
                                onmouseleave={(e) => e.currentTarget.blur()}
                                onclick={(e) => {e.currentTarget.blur(); editEffect(index)}}>
                            <PenTool class="w-5 h-5"/>
                        </button>
                    </div>
                </div>
            {/each}
            <div class="flex justify-end items-end w-full h-auto">
                <button class="w-1/10 text-center p-1.5 flex flex-row items-center justify-center gap-2 hover:bg-accent focus:bg-accent-secondary dark:focus:bg-accent-tertiary rounded-md transition"
                        onclick={(e) => {e.currentTarget.blur(); editEffect(null)}}
                        onmouseleave={(e) => {e.currentTarget.blur()}}>
                    <CirclePlus class="button-icon"/> New
                </button>
            </div>

        </div>
    </div>
{:else}
    <NoProjectLoaded />
{/if}
