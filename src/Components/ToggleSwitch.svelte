<script lang="ts">
    let { onClick, enabled = false } = $props();

</script>

<button role="switch" aria-checked={enabled} onclick={() => { onClick(); enabled = !enabled }} class="rounded-lg dark:bg-tertiary-d bg-tertiary p-2 w-20">
    {#if enabled}
        <p>aaaa</p>
    {:else}
        <p>bbbb</p>
    {/if}
</button>
