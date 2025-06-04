<script lang="ts">
    // Uses svelte-codemirror-editor
    import CodeMirror from "svelte-codemirror-editor";
    import { json } from "@codemirror/lang-json";

    let value = ""; // Fill this in with actual code later on
</script>

<CodeMirror bind:value lang={json()} />