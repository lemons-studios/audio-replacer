<script lang="ts">
  import * as ProjectManager from "../../util/ProjectManager"
  import AudioPlayer from "../../Components/AudioPlayer.svelte";
  import { record } from "extendable-media-recorder-wav-encoder";
  
  let currentPathTrunc = $state(ProjectManager.currentFileLocalPath || "Select a folder to begin");
  let currentPathFull = $state(ProjectManager.currentFile || "");

  let completionValue = $state(0.25);
  let completionPercentage = $state("0%");
  let filesRemaining = $state(ProjectManager.filesRemaining || 0);

  // I WISH enums were supported in svelte
  let idle = $state(true);
  let recording = $state(false);
  let reviewing = $state(false);

  function startRecord() {
    switchStates();
  }

  function stopRecord() {
    switchStates();
  }

  function cancelRecord() {
    recording = false;
    idle = true;
  }

  function finalizeRecording(isDiscarding: boolean) {
    switchStates();
  }

  // Best solution I can think of for now. There is most certainly a better way to do this
  function switchStates() {
    if(idle) {
      idle = false; 
      recording = true;
    }
    else if(recording) {
      recording = false;
      reviewing = true;
    }
    else if(reviewing) {
      reviewing = false;
      idle = true;
    }
  }

</script>

<div class="flex grow flex-row h-full gap-4 content-center">
  <div class="w-3/4 card">
    <h1 class="title-text mb-5"><b>{currentPathTrunc}</b></h1>
    <h2>Files Remaining: {filesRemaining} ({completionPercentage})</h2>
    <progress value={completionValue} class="text-2xl mb-15 rounded-3xl">AAA</progress>
    <AudioPlayer source={currentPathFull}/>
    <div class="flex flex-row justify-center grow gap-5">
      {#if idle}
        <button class="btn btn-primary w-25">Skip</button>
        <button class="btn btn-primary w-25" onclick={startRecord}>Record</button>
      {/if}
      {#if recording}
        <button class="btn btn-primary w-25" onclick={cancelRecord}>Cancel</button>
        <button class="btn btn-primary w-25" onclick={stopRecord}>End</button>
      {/if}
      {#if reviewing}
        <button class="btn btn-primary w-25" onclick={() => finalizeRecording(true)}>Discard</button>
        <button class="btn btn-primary w-25" onclick={() => finalizeRecording(false)}>Submit</button>
      {/if}
    </div>
  </div>
  <div class="w-1/2 card">
    <h1 class="title-text mb-[5rem]"><b>Recording Filters</b></h1>
    <h2 class="title-text mb-[2rem]">Pitch Filters</h2>
    <select class="bg-black h-[2.5rem] rounded-lg p-2 mb-[15rem]">
      <option>Default</option>
      <option>Some other option</option>
    </select>
    <h2 class="title-text mb-[2rem]">Audio Effects</h2>
    <select class="bg-black h-[2.5rem] rounded-lg p-2 mb-[5.5rem]">
      <option>Default</option>
      <option>Some Other Option</option>
    </select>
    <div class="flex flex-row justify-center gap-2">
      <input type="checkbox">
      <h4 class="text-xl">Requires Extra Edits?</h4>
    </div> 
  </div>
</div>
