<script lang="ts">
    import { onMount } from 'svelte';
  import { setDetails } from '../../Util/DiscordRpc';

    onMount(async() => {
        await setDetails("Recording")
    })
</script>

<h1>Record Page</h1>