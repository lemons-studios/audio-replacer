<script lang="ts">
    import { Check, Ban } from "@lucide/svelte";
    import { Command } from "@tauri-apps/plugin-shell";
    import Modal from "../../Components/Modal.svelte";
    import { effectFilterNames, effectFilters, pitchFilterNames, pitchFilters } from "../recordPage/AudioManager";
    import { updateArprojStats } from "../../tools/ProjectHandler";
    import { message } from "@tauri-apps/plugin-dialog";
    import { validateFilter } from "../../tools/OsTools";
    import {onMount} from "svelte";

    let { isEffect, selectedIndex = null } = $props();
    let modal: Modal;
    let selectedName: HTMLInputElement;
    let selectedValue: HTMLInputElement;

    onMount(() => {
        if(selectedIndex !== null) {
            selectedValue.value = (isEffect ? effectFilters : pitchFilters)[selectedIndex];
            selectedName.value = (isEffect ? effectFilterNames : pitchFilterNames)[selectedIndex];
        }
    })

    async function modifyEffect() {
        const effectName = selectedName.value;
        const effect = (isEffect ? selectedValue.value : selectedValue.value);
        const filters = isEffect ? effectFilters : pitchFilters ;
        const names = isEffect ? effectFilterNames : pitchFilterNames

        const effectToVerify = (isEffect ? selectedValue.value : `rubberband=pitch=${selectedValue.value}`);
        if(!(await validateFilter(effectToVerify))) {
            await message('Selected filter list is not valid', {
                title: 'Error',
                kind: 'error'
            });
            return;
        }

        if(selectedIndex === null) {
            // Creating Effect
            names.push(effectName);
            filters.push(effect);
        }
        else {
            // Modifying Effect
            names[selectedIndex] = effectName;
            filters[selectedIndex] = effect;
        }

        const newList = [];
        for(let i = 0; i < filters.length; i++) {
            newList.push({
                name: names[i],
                value: filters[i]
            });
        }

        await updateArprojStats((isEffect ? 'effectFilters' : 'pitchFilters'), newList)
        modal.toggleModal();
    }

    function cancelAction() {
        modal.toggleModal();
    }
</script>

<Modal closeable={false} bind:this={modal}>
    <div class="flex flex-col items-stretch justify-between w-full h-full">
        <h1 class="text-center text-4xl font-bold">{selectedIndex === null ? 'Create' : 'Edit'} {isEffect ? 'Effect' : 'Filter'}</h1>
       <!--Options-->
        <div class="flex flex-col justify-center text-left w-full h-full gap-y-5 mb-5">
            <div class="w-full">
                <h1 class="text-xl font-bold mb-2">Name</h1>
                <input bind:this={selectedName} type="text" maxlength="16" placeholder="Epic Filter Name" class="bg-tertiary w-full dark:bg-tertiary-d py-1.5 px-2 rounded-sm">
            </div>
            <div>
                <h1 class="text-xl font-bold mb-2">Value</h1>
                <input bind:this={selectedValue} type="text" placeholder={isEffect ? 'aecho=0.8:0.9:40|50|70:0.4|0.3|0.2' : '1.0'} class="bg-tertiary w-full dark:bg-tertiary-d py-1.5 px-2 rounded-sm">
            </div>
        </div>

       <!--Ok/Cancel Button-->
        <div class="flex flex-row justify-end items-end gap-x-5">
            <button
                class="w-1/5 text-center p-1.5 flex flex-row items-center justify-center gap-2 hover:bg-accent focus:bg-accent-secondary dark:focus:bg-accent-tertiary rounded-md transition"
                onclick={async(e) => { e.currentTarget.blur(); await modifyEffect() }}
                onmouseleave={(e) => { e.currentTarget.blur(); }}>
                <Check class="button-icon"/>Ok
            </button>
            <button
                    class="w-1/5 text-center p-1.5 flex flex-row items-center justify-center gap-2 hover:bg-accent focus:bg-accent-secondary dark:focus:bg-accent-tertiary rounded-md transition"
                    onclick={(e) => {e.currentTarget.blur(); cancelAction()}}
                    onmouseleave={(e) => {e.currentTarget.blur()}}>
                <Ban class="button-icon"/>Cancel
            </button>
        </div>
    </div>
</Modal>
