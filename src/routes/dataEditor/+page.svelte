<script lang="ts">
    // Uses svelte-codemirror-editor
    import { onMount } from "svelte";
    import { setDetails } from "../../app/DiscordRpc";
    import CodeEditor from "../../Components/CodeEditor.svelte";
    import { resolveResource } from "@tauri-apps/api/path";
    const data = {
      pitchData: 0,
      effectData: 1,
    }
    
    let selectedFile = $state(data.pitchData); // 0 being the pitch data file and 1 being the effect data file. There's probably a better way to represent this

    let value = ""; // Fill this in with actual code later on
    onMount(async() => {
    })

    async function writeContent() {

    }
    async function getSelectedPath(): Promise<string> {
      const file = selectedFile == 0 
      ? await resolveResource("resources/pitchData.json")
      : await resolveResource("resources/effectData.json");
      return file;
    }
</script>

<div class="flex flex-col grow">
  <div class="bg-white">
    asdsadsadsa
  </div>
  <CodeEditor content={async() => await getSelectedPath()}></CodeEditor>
</div>
