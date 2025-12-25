<script lang="ts">
    import { pitchFilters, pitchFilterNames, effectFilters, effectFilterNames } from "../recordPage/AudioManager";
    import { getArprojProperty, projectLoaded, updateArprojStats } from "../../tools/ProjectHandler";
    import {selectFile, validateFilter} from "../../tools/OsTools";
    import { readTextFile } from "@tauri-apps/plugin-fs";
    import { AudioLines, Sparkles, Plus, PencilLine, Import, Trash2 } from "@lucide/svelte";
    import NoProjectLoaded from "../../Components/NoProjectLoaded.svelte";
    import EffectModal from "./EffectModal.svelte";
    import { mount, onMount, unmount } from "svelte";
    import { ask } from "@tauri-apps/plugin-dialog";

    let pitchValues: string[] = $state([]);
    let pitchNames: string[] = $state([]);

    let effectValues: string[] = $state([]);
    let effectNames: string[] = $state([]);

    let selectedTab = $state(0);
    let currentModal: null | EffectModal = null;
    let fastDeleteEnabled = false; // I'll add a toggle for this later

    onMount(() => {
        updateFilters();
    });

    const updateFilters = () => {
        pitchValues = pitchFilters;
        pitchNames = pitchFilterNames;

        effectValues = effectFilters;
        effectNames = effectFilterNames;
    }

    async function importData() {
        const filePath = await selectFile(["json"], "JSON Files");
        if (!filePath) return;

        const overwrite = await ask('Would you like to overwrite the filters in this project with the ones in this file?', {
            title: 'Overwrite?',
            kind: 'info'
        });

        const obj = JSON.parse(await readTextFile(filePath));

        const validProperties = {
            pitchFilters: [],
            effectFilters: []
        };

        // Ignore errors here. It should work
        for (let i = 0; i < obj.pitchFilters.length; i++) {
            const o = obj.pitchFilters[i];
            if (o.hasOwnProperty("name") && o.hasOwnProperty("value") && (await validateFilter(o.pitchFilters[i].value))) {
                validProperties.pitchFilters.push(o);
            }
        }
        for (let i = 0; i < obj.effectFilters.length; i++) {
            const o = obj.effectFilters[i];
            if (o.hasOwnProperty("name") && o.hasOwnProperty("value")  && (await validateFilter(o.effectFilters[i].value))) {
                validProperties.effectFilters.push(o);
            }
        }

        const setProperties = async(pitch: any[], effect: any[]) => {
            await updateArprojStats('pitchFilters', pitch);
            await updateArprojStats('effectFilters', effect)
        }

        if (!overwrite) {
            const currentPitch = await getArprojProperty("pitchFilters");
            const currentEffect = await getArprojProperty("effectFilters");

            currentPitch.push(validProperties.pitchFilters).sort();
            currentEffect.push(validProperties.effectFilters).sort();
            await setProperties(currentPitch, currentEffect);
        }
        else {
            await setProperties(validProperties.pitchFilters, validProperties.effectFilters);
        }
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
        updateFilters();
    }

    async function removeEffect(index: number) {
        const confirm = async(): Promise<boolean> => {
            if(fastDeleteEnabled) {
                return true;
            }

            return (await ask("Are you sure you want to delete this filter?", {
                title: 'Confirm Delete?',
                kind: 'info'
            }));
        }

        if(await (confirm())) {
            const filters = selectedTab === 1 ? effectFilters : pitchFilters;
            const names = selectedTab === 1 ? effectFilterNames : pitchFilterNames;
            filters.splice(index, 1);
            names.splice(index, 1);
            const newEffects = [];
            for(let i = 0; i < filters.length; i++) {
                newEffects.push({
                    name: names[i],
                    value: filters[i]
                });
            }
            await updateArprojStats((selectedTab === 1 ? 'effectFilters' : 'pitchFilters'), newEffects);
            updateFilters();
        }
    }
</script>

{#if projectLoaded}
    <div class="flex flex-row w-full justify-around px-4 py-2 gap-3 min-h-15 card mb-1.5">
        <button
            class={`w-1/2 text-center p-1.5 flex flex-row items-center justify-center gap-2 ${selectedTab === 0 ? 'bg-accent' : ''} hover:bg-accent focus:bg-accent-secondary dark:focus:bg-accent-tertiary rounded-md transition`}
            onclick={(e) => {e.currentTarget.blur(); selectedTab = 0}}
            onmouseleave={(e) => {e.currentTarget.blur()}}>
            <AudioLines class="button-icon"/> Pitch Modifiers
        </button>
        <button
            class={`w-1/2 text-center p-1.5 flex flex-row items-center justify-center gap-2 ${selectedTab === 1 ? 'bg-accent' : ''} hover:bg-accent focus:bg-accent-secondary dark:focus:bg-accent-tertiary rounded-md transition`}
            onclick={(e) => {e.currentTarget.blur(); selectedTab = 1}}
            onmouseleave={(e) => {e.currentTarget.blur()}}>
            <Sparkles class="button-icon"/> Effect Filters
        </button>
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
                        <div class="flex flex-row w-auto">
                            <button class="hover:bg-accent focus:bg-accent-secondary dark:focus:bg-accent-tertiary transition p-2 rounded-md"
                                    onmouseleave={(e) => e.currentTarget.blur()}
                                    onclick={async(e) => {e.currentTarget.blur(); await removeEffect(index)}}>
                                <Trash2 class="w-5 h-5"/>
                            </button>
                            <button class="hover:bg-accent focus:bg-accent-secondary dark:focus:bg-accent-tertiary transition p-2 rounded-md"
                                    onmouseleave={(e) => e.currentTarget.blur()}
                                    onclick={(e) => {e.currentTarget.blur(); editEffect(index)}}>
                                <PencilLine class="w-5 h-5"/>
                            </button>
                        </div>
                    </div>
                </div>
            {/each}
            <div class="flex justify-end items-center w-full h-auto gap-2.5">
                <button class="w-1/8 transition duration-200 hover:bg-navigation-hover dark:hover:bg-navigation-hover-d focus:bg-navigation-focus drop-shadow-navigation-focus-shadow-d px-3 py-1.5 rounded-sm flex flex-row text-center items-center justify-center gap-2 import-button"
                        onclick={async(e) => {e.currentTarget.blur(); await importData()}}
                        onmouseleave={(e) => {e.currentTarget.blur()}}>
                    <Import class="button-icon"/>Import
                </button>
                <button class="w-1/10 transition duration-200 bg-accent hover:bg-accent-secondary dark:hover:bg-accent-tertiary dark:focus:bg-tertiary-d focus:bg-tertiary px-3 py-1.5 rounded-sm flex flex-row text-center items-center justify-center gap-2"
                        onclick={(e) => {e.currentTarget.blur(); editEffect(null)}}
                        onmouseleave={(e) => {e.currentTarget.blur()}}>
                    <Plus class="button-icon"/>New
                </button>
            </div>
        </div>
    </div>
{:else}
    <NoProjectLoaded />
{/if}

<style>
    .import-button:focus {
        @media (prefers-color-scheme: dark) {
            box-shadow: inset 0 0 1em oklch(0.1929 0.0048 325.72 / 60%);
        }
        @media (prefers-color-scheme: light) {
            box-shadow: inset 0 0 1em oklch(0.9228 0.0048 325.72 / 90%);
        }
    }
</style>
