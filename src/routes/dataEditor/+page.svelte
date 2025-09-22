<script lang="ts">
    import { ArrowLeftRight, BadgeQuestionMark, Save } from "@lucide/svelte";
    import CodeEditor from "../../Components/CodeEditor.svelte";
    import { onMount, tick } from "svelte";

    let selectedFile = $state();
    let codeEditor: CodeEditor;

    onMount(async() => {
      await tick();

      const returnTruncated = false;
      selectedFile = codeEditor.getSelectedFile(returnTruncated);
    })

    async function applyChanges() {
      await codeEditor.saveContentToFile();
    }

    async function switchFiles() {
      await codeEditor.switchFiles();
    }
</script>

<div class="flex flex-col grow">
  <div class="bg-base-300 p-2.5 min-h-20 items-center justify-center content-center flex flex-col grow mb-3.5 rounded-lg">
    <h3>{selectedFile}</h3>
    <div class="flex flex-row gap-2.5">
      <button class="btn btn-sm btn-accent max-h-8 min-w-20" onclick={async() => await switchFiles()}><ArrowLeftRight/> Switch</button>
      <button class="btn btn-sm btn-accent max-h-8 min-w-20" onclick={async() => await applyChanges()}><Save/> Save</button>
      <button class="btn btn-sm btn-accent max-h-8 min-w-20" onclick={() => open("https://github.com/lemons-studios/audio-replacer/wiki")}><BadgeQuestionMark /> Help</button>
    </div>
  </div>
  <CodeEditor bind:this={codeEditor} />
</div>
