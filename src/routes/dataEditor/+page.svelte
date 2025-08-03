<script lang="ts">
    // Uses svelte-codemirror-editor
    import { onDestroy, onMount } from "svelte";
    import { setDetails } from "../../tools/DiscordRpc";
    import CodeEditor from "../../Components/CodeEditor.svelte";
    import { resolveResource } from "@tauri-apps/api/path";
  import { register, unregisterAll } from "@tauri-apps/plugin-global-shortcut";
    const data = {
      pitchData: 0,
      effectData: 1,
    }
    
    let codeEditor: CodeEditor;
    let selectedFilePath;
    

    let value = ""; // Fill this in with actual code later on
    onMount(async() => {
      await setDetails("Data Editor");
      const hotkeys = [
        {
          keybind: "CommandOrControl+S",
          action: async() => {
            await writeContent();
          }
        },
        {
          keybind: "CommandOrControl+F",
          action: async() => {
            await switchFiles();
          }
        }
      ];

      for(let i = 0; i < hotkeys.length; i++) {
        await register(hotkeys[i].keybind, hotkeys[i].action);
      }
    });

    onDestroy(async() => {
      await unregisterAll();
    })

    async function writeContent() {
      const file = await getSelectedPath();
      await codeEditor.saveContentToData(file);
    }

    async function switchFiles() {

    }

    async function getSelectedPath(): Promise<string> {
      const file = selectedFile == 0 
      ? await resolveResource("resources/pitchData.json")
      : await resolveResource("resources/effectData.json");
      return file;
    }
</script>

<div class="flex flex-col grow">
  <div class="bg-base-300 p-2.5 min-h-20 items-center justify-center content-center flex flex-col grow mb-3.5 rounded-lg">
    <h3 class="font-bold mb-1">File Name*</h3>
    <button class="btn btn-sm btn-accent max-h-8 min-w-20">Switch</button>
  </div>
  <CodeEditor bind:this={codeEditor}></CodeEditor>
</div>
