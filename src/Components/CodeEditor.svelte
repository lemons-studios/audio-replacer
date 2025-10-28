<script lang="ts">
  import { resolveResource } from '@tauri-apps/api/path';
  import { readTextFile } from '@tauri-apps/plugin-fs';
  import { onMount } from 'svelte';
  import { JSONEditor, type Content } from 'svelte-jsoneditor';
  
  let json: Content = $state({
    text: undefined,
    json: undefined,
  });
  
  const dataFiles = {
    pitchData: 'resources/pitchData.json',
    effectData: 'resources/effectData.json',
  } as const;

  onMount(async() => {
    const dataPath = await resolveResource(dataFiles.effectData);
    const data = await readTextFile(dataPath);
    json = { text: data, json: data };
  });
  
</script>


<JSONEditor bind:json />