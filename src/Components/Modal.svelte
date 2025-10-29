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
        background-color: oklch(0.1574 0 82 / 75%);
    }

    .close-btn {
        position: absolute;
        align-self: top;
        right: 0.25em;
        top: 50%;
        transform: translateY(-50%);
        display: flex;
        justify-content: top;
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

<div class="fixed inset-0 flex justify-center align-center content-center h-max w-max dark:text-white">
    <dialog onclose={() => showModal = false} bind:this={dialog} class=" p-2 min-h-100 min-w-150 dark:border-tertiary-d border-tertiary dark:bg-secondary-d bg-secondary rounded-xl">
        <button class="close-btn rounded-sm align-top" onclick={() => showModal = false} onmouseleave={(e) => e.currentTarget.blur()}><IconXRegular class="dark:text-white h-7 w-7"></IconXRegular></button>
        {@render children?.()}
    </dialog>
</div>


