<script lang="ts">
    import IconXRegular from "phosphor-icons-svelte/IconXRegular.svelte";
    let { children, showModal = $bindable() } = $props();
    let dialog: HTMLDialogElement;

    $effect(() => {
        if(showModal) dialog.showModal();
        if(!showModal) dialog.close();
    })
</script>

<style>
    ::backdrop {
        /* TODO: Figure out background blur */
        filter: blur(10px) ;
        -webkit-filter: blur(8px);
        background-color: oklch(0.1574 0 82 / 65%);
    }

    .close-btn {
        position: absolute;
        align-self: top;
        right: 0.25em;
        top: 50%;
        transform: translateY(-50%);
        display: flex;
    }

    .close-btn:hover {
        background-color: oklch(1 0 0 / 30%);
        box-shadow: none;
    }

    .close-btn:focus {
        background-color: oklch(1 0 0 / 10%);
        box-shadow: inset 0px 0px 1em oklch(0.1929 0.0048 325.72 / 60%);
    }
</style>

<dialog onclose={() => showModal = false} bind:this={dialog} class="flex justify-center align-middle items-center p-5 min-h-1/2 min-w-3/5 dark:text-white dark:border-tertiary-d border-tertiary dark:bg-secondary-d bg-secondary rounded-xl">
    <button class="close-btn rounded-sm align-top mr-2.5"
            onclick={() => showModal = false}
            onmouseleave={(e) => e.currentTarget.blur()}><IconXRegular class=" h-7 w-7" /></button>
    {@render children?.()}
</dialog>
