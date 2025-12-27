<script lang="ts">
    import  { ArrowDown } from "@lucide/svelte";
    import { slide } from 'svelte/transition'
    type DropdownValues = { label: string, value: string };
    let { values = [{label: "Test test hi", value: "test"}], onChange = (() => {}) , selectedIndex = null }: { values: DropdownValues[], onChange: any, selectedIndex: number | null } = $props()
    let selectedValueLabel = $state();
    let dropdownVisible = $state(false);
    $effect(() => {

    });
</script>

<div class="flex flex-col w-60 h-auto bg-secondary-d  rounded-md">
    <button
            class={`flex px-4 py-2 flex-row-reverse border-white/20 ${!dropdownVisible ? 'rounded-md border ' : 'rounded-t-md border-t border-l border-r'} bg-accent justify-between items-center`}
            onclick={() => {dropdownVisible = !dropdownVisible}}><ArrowDown class="button-icon"/>Alphabetical (A-Z)
    </button>
    {#if dropdownVisible}
        <div class="h-full w-full flex flex-col justify-start" transition:slide>
            {#each values as value, index}
                <button class={`px-4 py-1 w-full text-left border-white/20 ${index === values.length - 1 ? 'border rounded-b-lg' : 'border-t border-l border-r'}`}>{value.label}</button>
            {/each}
        </div>
    {/if}
</div>
