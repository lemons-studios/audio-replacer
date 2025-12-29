<script lang="ts">
    import  { ArrowDown, ArrowUp } from "@lucide/svelte";
    import { slide } from 'svelte/transition'
    import { onMount } from "svelte";
    let {
        values = [],
        startingIndex = 0,
        onChange = (() => {}),
    }: {
        values: string[],
        startingIndex: number,
        onChange: (index: number, label: string) => void,
    } = $props();

    let selectedIndex = $state(startingIndex);

    let dropdownOpen = $state(false);
    let dropdownElement: HTMLDivElement;

    async function changeSelection(index: number) {
        onChange(index, values[index]);
        selectedIndex = index;
        dropdownOpen = false;
    }

    onMount(() => {
        const handleOutsideClick = (e: MouseEvent) => {
            const target = e.target as Node;
            if(dropdownElement && !dropdownElement.contains(target)) {
                dropdownOpen = false;
            }
        }

        document.addEventListener('click', handleOutsideClick);
        return () => {
            document.removeEventListener('click', handleOutsideClick);
        }
    });
</script>

<div class="relative w-60">
    <div class="flex flex-col h-auto bg-tertiary dark:bg-tertiary-d overflow-y-auto" bind:this={dropdownElement}>
        <button
                class={` dark:text-white font-medium flex px-4 py-2 flex-row-reverse border-white/20 ${!dropdownOpen ? 'rounded-md border ' : 'rounded-t-md border-t border-l border-r'} transition-colors duration-300 focus:bg-accent hover:bg-accent justify-between items-center`}
                onclick={(e) => {e.currentTarget.blur() ;dropdownOpen = !dropdownOpen}}>
            {#if !dropdownOpen}
                <ArrowDown class="button-icon"/>
            {:else}
                <ArrowUp class="button-icon"/>
            {/if}
            {values[selectedIndex] ?? "No Values"}
        </button>
        {#if dropdownOpen}
            <div class="absolute dark:bg-tertiary-d bg-tertiary top-full left-0 w-full z-50 flex flex-col max-h-60 overflow-y-auto " transition:slide={{duration: 175}}>
                {#each values as label, index}
                    <button class={` dark:text-white font-medium hover:bg-accent focus:bg-accent-secondary transition-colors duration-200 px-4 py-2 w-full text-left border-white/20 ${index === values.length - 1 ? 'border-b border-l border-r rounded-b-lg' : 'border-l border-r'}`}
                            onclick={async() => {await changeSelection(index)}}
                            onmouseleave={(e) => {e.currentTarget.blur()}}>
                        {label}
                    </button>
                {/each}
            </div>
        {/if}
    </div>
</div>
