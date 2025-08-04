<script lang="ts">
  // Code yoinked from https://www.codelantis.com/blog/sveltekit-monaco-editor. same article as Monaco.ts
  import { onDestroy, onMount } from "svelte";
  import { editorTheme } from "../routes/dataEditor/CodeEditorTheme";
  import type * as Monaco from "monaco-editor/esm/vs/editor/editor.api.js"; // If an error pops up here, ignore it. the file most certainly exists
  import { exists, readTextFile, writeTextFile } from "@tauri-apps/plugin-fs";
  import { populateCustomData } from "../routes/recordPage/EffectManager";

  let editor: any = $state();
  let monaco: typeof Monaco;
  let editorContainer: HTMLElement;

  let { filePath } = $props();
  let trueFilePath = $derived(filePath);
  let contents = (async() => {
    if(!trueFilePath || !(await exists(trueFilePath))) {
      return "";
    }
    console.log("File Path valid")
    const contents = await readTextFile(filePath);
    console.log(contents);
    return contents;
  })

  let isEditorLoaded = $state(false);

  onMount(async () => {
    // Import our 'monaco.ts' file here
    // (onMount() will only be executed in the browser, which is what we want)
    console.log("Setting Monaco");
    monaco = (await import("../routes/dataEditor/Monaco")).default;

    // Your monaco instance is ready, let's display some code!
    console.log("Setting Monaco Theme");
    monaco.editor.defineTheme("catppuccin", editorTheme); // Ignore error, it works as intended
    console.log("Creating monaco editor and setting syntax");

    editor = monaco.editor.create(editorContainer, {
      theme: "catppuccin",
      automaticLayout: true,
      language: "json",
      minimap: { enabled: false },
      hideCursorInOverviewRuler: true,
      fontFamily: "'source code pro', monospace",
      fontLigatures: true,
      fontSize: 14,
      formatOnPaste: true
    });


    const fileContents = await contents();
    const model = monaco.editor.createModel(fileContents, "json");
    console.log("Final loading steps");
    editor.setModel(model);
    isEditorLoaded = true;
  });

  onDestroy(() => {
    monaco?.editor.getModels().forEach((model: any) => model.dispose());
    editor?.dispose();
    isEditorLoaded = false;
  });

  export async function saveContentToData(path: string) {
    const content = editor?.getValue();
    await writeTextFile(path, content);
    
    // repopulate the pitch/effect data json variables
    await populateCustomData();
  }
</script>

<div class="relative w-full h-full">
  <div class="w-full h-full" bind:this={editorContainer}></div>

  {#if !isEditorLoaded}
    <div class="flex flex-row grow justify-center items-center gap-3 absolute inset-0">
      <span class="loading loading-spinner text-primary text-xl"></span>
      <h3 class="text-xl">Loading Data Editor...</h3>
    </div>
  {/if}
</div>
