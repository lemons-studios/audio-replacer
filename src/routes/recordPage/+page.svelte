<script lang="ts">
    import { register } from '@tauri-apps/plugin-global-shortcut';
    import { onMount } from 'svelte';

    async function registerShortcuts() {
        await register('CommandOrControl+Shift+R', () => toggleRecord())
    }

    async function toggleRecord() {
        
    }

    onMount(async() => {
        await registerShortcuts();  
    })
</script>
