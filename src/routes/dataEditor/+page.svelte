<script lang="ts">
    import { ArrowLeftRight, BadgeQuestionMark, Redo, Save, Undo } from "@lucide/svelte";
    import CodeEditor from "../../Components/CodeEditor.svelte";
    import { onMount } from "svelte";
    import { resolveResource } from "@tauri-apps/api/path";
    let selectedFile = $state();

    onMount(async() => {
      selectedFile = await resolveResource("resources/pitchData.json");
    })
</script>

<div class="flex flex-col grow">
  <div class="bg-base-300 p-2.5 min-h-20 items-center justify-center content-center flex flex-col grow mb-3.5 rounded-lg">
    <div class="flex flex-row gap-2.5">
      <button class="btn btn-sm btn-accent max-h-8 min-w-20"><ArrowLeftRight/> Switch</button>
      <button class="btn btn-sm btn-accent max-h-8 min-w-20"><Save/> Save</button>
      <button class="btn btn-sm btn-accent max-h-8 min-w-20"><Undo/> Undo</button>
      <button class="btn btn-sm btn-accent max-h-8 min-w-20"><Redo/> Redo</button>
      <button class="btn btn-sm btn-accent max-h-8 min-w-20" onclick={() => open("https://github.com/lemons-studios/audio-replacer/wiki")}><BadgeQuestionMark /> Help</button>
    </div>
  </div>
  <CodeEditor currentPath={selectedFile} />
</div>
