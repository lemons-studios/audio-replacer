<script lang="ts">
  import * as ProjectManager from "../../app/ProjectManager"
  import AudioPlayer from "../../Components/AudioPlayer.svelte";
  import { onMount } from "svelte";
  import { transcribeFile } from "../../tools/WhisperUtils";
  import { effectDataNames, effectDataValues, pitchDataNames, pitchDataValues } from "../../tools/EffectManager";
  import { setEffect, setPitch } from "../../app/FFMpegManager";
  import { setDetails } from "../../app/DiscordRpc";
  import { cancelRecording, endRecording, startRecording } from "../../app/AudioRecorder";
  
  let currentPathTrunc = $state(ProjectManager.currentFileLocalPath || "Select a folder to begin");
  let currentAudioPath = $state(ProjectManager.currentFile || "");

  let completionValue = $state(0);
  let completionPercentage = $state("0%");
  let filesRemaining = $state(ProjectManager.filesRemaining || 0);
  let currentTranscription = $state("Transcription Unavailable")

  let idle = $state(true);
  let recording = $state(false);
  let reviewing = $state(false);

  let audioPlayer: AudioPlayer;
  let pitchDropdown: HTMLSelectElement;
  let effectDropdown: HTMLSelectElement;

  onMount(async() => {
    setFileData();
    await setDetails("Recording");

    currentTranscription = await transcribeFile(currentAudioPath);
  })

  async function startRecord() {
    console.log("Recording Started!");
    switchStates();
    await startRecording();
  }

  async function stopRecord() {
    console.log("Recording Stopped, attempting to save to: ", ProjectManager.currentOutFile);
    await endRecording(ProjectManager.currentOutFile);
    currentAudioPath = ProjectManager.currentOutFile;
    switchStates();
  }

  function cancelRecord() {
    console.log("Recording Canceled");
    cancelRecording();
    recording = false;
    idle = true;
  }

  async function finalizeRecording(isDiscarding: boolean) {
    switchStates();
    if(isDiscarding) {
      await ProjectManager.rejectFile();
    }
    else {
      await ProjectManager.submitFile();
      currentTranscription = await transcribeFile(currentAudioPath);
    }
    setFileData();
  }

  async function skipFile() {
    await ProjectManager.skipFile();
    setFileData();
    currentTranscription = await transcribeFile(currentAudioPath);
  }

  function setFileData() {
    if(ProjectManager.isProjectLoaded) {
      currentAudioPath = ProjectManager.currentFile;
      currentPathTrunc = ProjectManager.currentFileLocalPath.replaceAll("\\", "/").slice(1);
      completionValue = ProjectManager.completionPercentage;
      filesRemaining = ProjectManager.filesRemaining;
      completionPercentage = `${completionValue.toFixed(2)}%`;
      audioPlayer.playAudio();
    }
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

  function selectPitchValue() {
    const index = pitchDropdown.selectedIndex;
    setPitch(pitchDataValues[index]);
  }

  function selectEffectValue() {
    const index = effectDropdown.selectedIndex;
    setEffect(effectDataValues[index]);
  }
</script>

<div class="flex grow flex-row h-full gap-4 content-center">
  <fieldset class="w-3/4 pane content-center">
    <legend class="fieldset-legend">Project</legend>
    <h1 class="title-text mb-5 font-semibold">{currentPathTrunc}</h1>
    <h2 class="text-center text-lg font-medium">Files Remaining: {filesRemaining} ({completionPercentage})</h2>
    <progress value={completionValue} class="mb-3.5 progress progress-primary m-auto" max="100"></progress>
    <AudioPlayer source={currentAudioPath} bind:this={audioPlayer}/>
    <div class="flex flex-row justify-center gap-5 mb-2.5 mt-2.5">
      {#if idle}
        <button class="btn btn-primary w-25" onclick={() => skipFile()}>Skip</button>
        <button class="btn btn-primary w-25" onclick={async() => await startRecord()}>Record</button>
      {/if}
      {#if recording}
        <button class="btn btn-primary w-25" onclick={() => cancelRecord()}>Cancel</button>
        <button class="btn btn-primary w-25" onclick={async() => await stopRecord()}>End</button>
      {/if}
      {#if reviewing}
        <button class="btn btn-primary w-25" onclick={() => finalizeRecording(true)}>Discard</button>
        <button class="btn btn-primary w-25" onclick={() => finalizeRecording(false)}>Submit</button>
      {/if}
    </div>
    <h2 class="text-center text-lg font-medium">Transcription: {currentTranscription}</h2>
  </fieldset>
  <fieldset class="w-2/4 pane content-center">
    <legend class="fieldset-legend">Recording Filters</legend>
    <h2 class="title-text mb-[2rem] font-semibold">Pitch Filters</h2>
    <select class="select select-primary select-md m-auto" bind:this={pitchDropdown} onchange={selectPitchValue}>
      {#each pitchDataNames as name }
        <option>{name}</option>
      {/each}
    </select>
    <h2 class="title-text mb-[2rem] font-semibold">Audio Effects</h2>
    <select class="select select-primary select-md m-auto" bind:this={effectDropdown} onchange={selectEffectValue}>
      {#each effectDataNames as name }
        <option>{name}</option>
      {/each}
    </select>
    <div class="flex flex-row justify-center gap-2">
      <input type="checkbox">
      <h4 class="text-xl">Requires Extra Edits?</h4>
    </div> 
  </fieldset>
</div>
