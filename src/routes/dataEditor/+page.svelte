<script lang="ts">
    // Uses svelte-codemirror-editor
    import { onMount } from "svelte";
    import { setDetails } from "../../app/DiscordRpc";
  import CodeEditor from "../../Components/CodeEditor.svelte";

    let value = ""; // Fill this in with actual code later on
    onMount(async() => {
        await setDetails("Data Editor")
    })
</script>

<CodeEditor></CodeEditor>
