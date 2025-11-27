<script lang="ts">
    import { pitchFilters, pitchFilterNames, effectFilters, effectFilterNames } from "../recordPage/AudioManager";
    import { projectLoaded } from "../../tools/ProjectHandler";
    import { goto } from "$app/navigation";
    import IconPenNibRegular from 'phosphor-icons-svelte/IconPenNibRegular.svelte';

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

</script>

<div class="card p-5 h-full">
    {#if projectLoaded}
            <div class="flex flex-col w-full rounded-lg justify-center items-center text-center gap-5">
                    {#if selectedTab === 0}
                        {#each pitchNames as p, index}
                            <div class=" flex flex-row items-center text-center justify-between min-h-15 bg-tertiary-d p-3 w-full rounded-lg">
                                <h2 class="ml-10 text-xl">{p}</h2>
                                <div class="gap-3 flex flex-row justify-center items-center">
                                    <h2 class="text-center">Pitch Multiplier: <p class="text-gray-500 text-sm">{pitchValues[index]}x</p></h2>
                                    <button class="hover:bg-accent focus:bg-accent-tertiary transition p-2 rounded-md"
                                    onmouseleave={(e) => e.currentTarget.blur()}
                                    onclick={(e) => e.currentTarget.blur()}><IconPenNibRegular class="w-5 h-5"></IconPenNibRegular></button>
                                </div>
                            </div>
                        {/each}
                    {:else if selectedTab === 1}
                        {#each effectNames as e, index}
                            <div class="flex flex-row items-center text-center justify-between min-h-15 dark:bg-tertiary-d bg-tertiary p-3 w-full rounded-lg">
                                <h2>{e}</h2>
                                <div class="gap-3 flex flex-row justify-center items-center">
                                    <h2 class="text-center">Effect Value: <p class="text-gray-500 text-sm">{effectValues[index]}</p></h2>
                                    <button class="hover:bg-accent focus:bg-accent-tertiary transition p-2 rounded-md"
                                    onmouseleave={(e) => e.currentTarget.blur()}
                                    onclick={(e) => e.currentTarget.blur()}><IconPenNibRegular class="w-5 h-5"></IconPenNibRegular></button>
                                </div>
                            </div>
                        {/each}
                    {/if}
            </div>
    {:else}
        <div class="flex flex-col justify-center text-center h-full">
            <h1 class="text-2xl">You don't have a project Loaded!</h1>
            <div class="flex flex-row justify-center mt-5 gap-3">
                <button class="app-btn" onclick={() => goto("/")}>Return Home</button>
            </div>
        </div>
    {/if}
</div>