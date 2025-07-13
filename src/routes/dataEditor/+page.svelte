<script lang="ts">
    // Uses svelte-codemirror-editor
    import CodeMirror from "svelte-codemirror-editor";
    import { json } from "@codemirror/lang-json";
    import { onMount } from "svelte";
    import { setDetails } from "../../util/DiscordRpc";

    let value = ""; // Fill this in with actual code later on
    onMount(async() => {
        await setDetails("Data Editor")
    })
</script>

<!--Change this to some better component later on-->
<CodeMirror bind:value lang={json()} class="prose dark:prose-invert"/>