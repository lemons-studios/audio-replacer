<script lang="ts">
  import { resolveResource } from '@tauri-apps/api/path';
  import "ace-builds/css/theme/dracula.css";
  import { readTextFile, writeTextFile } from '@tauri-apps/plugin-fs';
  import JsonEditor from 'jsoneditor';
  import "jsoneditor/dist/jsoneditor.css";
  import { onMount, tick } from 'svelte';
    import { convertFileSrc } from '@tauri-apps/api/core';
    import { currentFile } from '../tools/ProjectManager';

  let isLoaded = $state(false);
  let currentPath: string = $state("");

  let jsonEditorContainer: HTMLDivElement;
  let editor: JsonEditor;

  const filePaths = {
    pitchData: "resources/pitchData.json",
    effectData: "resources/effectData.json"
  } as const

  onMount(async() => {
    await tick();
    currentPath = await resolveResource(filePaths.pitchData);
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

  export function reloadEditor() {
    
  }

  export async function saveContentToFile(path: string) {
    const contents = editor.get();
    await writeTextFile(currentPath, contents);
  }

  export function formatEditor() {

  }

  export function switchFiles() {
    currentPath = curren
  }

  
</script>
{#if !isLoaded}
  <div class="flex flex-row grow justify-center items-center gap-3 absolute inset-0">
    <span class="loading loading-spinner text-primary text-xl"></span>
    <h3 class="text-xl">Loading Editor</h3>
  </div>
{/if}
<div bind:this={jsonEditorContainer} class="h-full w-full"></div>
