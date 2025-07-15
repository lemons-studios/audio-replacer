<script lang="ts">
    // Uses svelte-codemirror-editor
    import { onMount } from "svelte";
    import { setDetails } from "../../util/DiscordRpc";

    let value = ""; // Fill this in with actual code later on
    onMount(async() => {
        await setDetails("Data Editor")
    })
</script>

