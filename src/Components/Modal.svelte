<script lang="ts">
    import { X } from "@lucide/svelte";

    let { children, closeable } = $props();
    let modalEnabled = $state(true);
    
    let dialog: HTMLDialogElement;

    export function toggleModal() {
        modalEnabled = !modalEnabled;
    }

    $effect(() => {
        if(modalEnabled) dialog.showModal();
        if(!modalEnabled) dialog.close();
    });
</script>

<style>
    dialog {
        position: fixed;
        top: 50%;
        left: 50%;
        transform: translate(-50%, -50%);
        margin: 0
    }

    ::backdrop {
        backdrop-filter: blur(7px);
        -webkit-backdrop-filter: blur(7px); /*For Linux (Webkit2Gtk) and MacOS (Regular Webkit)*/
        background-color: oklch(0.1574 0 82 / 65%);
        z-index: 1000;
    }

    .close-btn {
        position: absolute;
        align-self: auto;
        right: 0.25em;
        top: 50%;
        transform: translateY(-50%);
        display: flex;
        z-index: 10;
    }

    .close-btn:hover {
        background-color: oklch(1 0 0 / 30%);
        box-shadow: none;
    }

    .close-btn:focus {
        background-color: oklch(1 0 0 / 10%);
        box-shadow: inset 0 0 1em oklch(0.1929 0.0048 325.72 / 60%);
    }
</style>

{#if modalEnabled}
    <dialog bind:this={dialog} class="flex border-2 text-wrap justify-center align-middle items-center p-5 h-auto w-auto dark:text-white dark:border-tertiary-d border-tertiary dark:bg-secondary-d bg-secondary rounded-xl">
        {#if closeable}
        <button class="close-btn rounded-sm align-top mr-2.5"
                onclick={() => modalEnabled = false}
                onmouseleave={(e) => e.currentTarget.blur()}><X/></button>
        {/if}
        <div class="text-wrap px-2 py-4 w-full h-full">
            {@render children?.()}
        </div>
    </dialog>
{/if}
