<script lang="ts">
    import { onMount } from "svelte";
  import { setDetails } from "../../Util/DiscordRpc";
    onMount(async() => {
        await setDetails("Settings")
    })
</script>

<h1>Settings Page</h1>