<script lang="ts">
    // Uses svelte-codemirror-editor
    import { onMount } from "svelte";
    import { setDetails } from "../../app/DiscordRpc";
    import CodeEditor from "../../Components/CodeEditor.svelte";
    import { resolveResource } from "@tauri-apps/api/path";
    import { readTextFile } from "@tauri-apps/plugin-fs";
    let selectedFile = $state(0); // 0 being the pitch data file and 1 being the effect data file. There's probably a better way to represent this

    let value = ""; // Fill this in with actual code later on
    onMount(async() => {
        await setDetails("Data Editor")
    })

    async function writeContent() {

    }
    async function readContent(): Promise<string> {
      const file = selectedFile == 0 ? await resolveResource("resources/pitchData.json") : await resolveResource("resources/effectData.json");
      const content = await readTextFile(file);
      return content;
    }
</script>

<CodeEditor content={async() => await readContent()}></CodeEditor>
