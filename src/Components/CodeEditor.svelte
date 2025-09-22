<script lang="ts">
  import { resolveResource } from '@tauri-apps/api/path';
  import { readTextFile, writeTextFile } from '@tauri-apps/plugin-fs';
  import JsonEditor from 'jsoneditor';
  import "jsoneditor/dist/jsoneditor.css";
  import { onMount, tick } from 'svelte';

  let isLoaded = $state(false);
  let currentPath: string = $state("");
  let currentTruncatedPath: string = $state("");

  let jsonEditorContainer: HTMLDivElement;
  let editor: JsonEditor;

  const filePaths = {
    pitchData: "resources/pitchData.json",
    effectData: "resources/effectData.json"
  } as const

  onMount(async() => {
    await tick();
    currentTruncatedPath = filePaths.pitchData;
    currentPath = await resolveResource(currentTruncatedPath);
    const initialJson = JSON.parse(await readTextFile(currentPath));
    editor = new JsonEditor(jsonEditorContainer, {
      mode: 'code',
      ace: window.ace,
      mainMenuBar: false,
      navigationBar: false
    });

    editor.set(initialJson);
    isLoaded = true;
  })

  export async function saveContentToFile() {
    const contents = editor.get();
    await writeTextFile(currentPath, contents);
  }

  export function formatEditor() {
    const contents = editor.get();
    
  }

  export async function switchFiles() {
    const contents = editor.get();
    await writeTextFile(currentPath, contents);

    currentTruncatedPath = currentTruncatedPath === filePaths.pitchData ? filePaths.effectData : filePaths.pitchData;
    currentPath = await resolveResource(currentTruncatedPath);
    const newInitialJson = JSON.parse(await readTextFile(currentPath));
    editor.set(newInitialJson);
  }

  export function getSelectedFile(returnFull: boolean) {
    return returnFull ? currentPath : currentTruncatedPath;
  }
  
</script>
{#if !isLoaded}
  <div class="flex flex-row grow justify-center items-center gap-3 absolute inset-0">
    <span class="loading loading-spinner text-primary text-xl"></span>
    <h3 class="text-xl">Loading Editor</h3>
  </div>
{/if}
<div bind:this={jsonEditorContainer} class="h-full w-full"></div>
