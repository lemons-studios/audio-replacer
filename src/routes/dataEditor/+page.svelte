<script lang="ts">
    // Uses svelte-codemirror-editor
    import { onDestroy, onMount } from "svelte";
    import { setDetails } from "../../tools/DiscordRpc";
    import CodeEditor from "../../Components/CodeEditor.svelte";
    import { resolveResource } from "@tauri-apps/api/path";
    import { register, unregisterAll } from "@tauri-apps/plugin-global-shortcut";
    import { readTextFile } from "@tauri-apps/plugin-fs";
    import { ArrowLeftRight, BadgeQuestionMark, Redo, Save, Undo } from "@lucide/svelte";

    let codeEditor: CodeEditor;
    let selectedFilePath = $state("");
    let effectPath: string;
    let pitchPath: string;

    onMount(async() => {
      await setDetails("Data Editor");
      pitchPath = await resolveResource("resources/pitchData.json");
      console.log(pitchPath);
      effectPath = await resolveResource("resources/effectData.json");
      console.log(effectPath);

      const hotkeys = [
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
      
      selectedFilePath = pitchPath;
      const data = await readTextFile(selectedFilePath);
    });

    onDestroy(async() => {
      await unregisterAll();
      selectedFilePath = "";
    })

    async function switchFiles() {
      // First, save the current content in the editor to the current file
      await codeEditor.saveContentToData(selectedFilePath);

      // Now, switch files
      selectedFilePath = (selectedFilePath == pitchPath) ? effectPath : pitchPath;
      console.log(`Switched file to ${selectedFilePath}`);
    }

    function getFileName() {
      return selectedFilePath.split("/")[-1];
    }
</script>

<div class="flex flex-col grow">
  <div class="bg-base-300 p-2.5 min-h-20 items-center justify-center content-center flex flex-col grow mb-3.5 rounded-lg">
    <h3 class="font-bold mb-1">Current File: {getFileName()}</h3>
    <div class="flex flex-row gap-2.5">
      <button class="btn btn-sm btn-accent max-h-8 min-w-20" onclick={async() => await switchFiles()}><ArrowLeftRight/> Switch</button>
      <button class="btn btn-sm btn-accent max-h-8 min-w-20"><Save/> Save</button>
      <button class="btn btn-sm btn-accent max-h-8 min-w-20"><Undo/> Undo</button>
      <button class="btn btn-sm btn-accent max-h-8 min-w-20"><Redo/> Redo</button>
      <button class="btn btn-sm btn-accent max-h-8 min-w-20" onclick={() => open("https://github.com/lemons-studios/audio-replacer/wiki")}><BadgeQuestionMark /> Help</button>
    </div>
  </div>
  <CodeEditor bind:this={codeEditor} filePath={selectedFilePath}></CodeEditor>
</div>
