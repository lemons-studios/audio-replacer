<script lang="ts">
  import { onMount } from "svelte";
  import { setDetails } from "../../util/DiscordRpc";
  import { selectFile } from "../../util/OsTools";
  import { transcribeFile } from "../../util/WhisperUtils";
  
  // I LOVE RUNES
  let idleState = $state(true);
  let recordingState = $state(false);
  let reviewingState = $state(false);

  let currentFilePath = $state("Select A Folder To Begin")
  let audioTranscription = $state("No Transcription Yet");
  

  onMount(async () => {
    await setDetails("Recording");
  });

  function switchStates() {
    if(idleState) {
      idleState = false;
      recordingState = true;
    }
    else if (recordingState) {
      recordingState = false;
      reviewingState = true;
    }
    else if (reviewingState) {
      reviewingState = false;
      idleState = true;
    }
  }

  function cancelRecording() {
    recordingState = false;
    idleState = true;
  }

  async function stopRecording() {
    switchStates();
  }

  async function startRecording() {
    switchStates();
  }

  async function skipFile() {
    
  }

  async function submitRecording() {
    switchStates();
  }

  function rejectRecording() {
    switchStates();
  }

  async function transcribeAudioFile() {
    const file = await selectFile();
    const transcription = await transcribeFile(file);
    audioTranscription = transcription;
  }
</script>
<div class="grid grid-cols-2 gap-5 w-full h-full">
  <div class="rounded-xl drop-shadow-2xl dark:bg-surface-container-dark p-5">
    <h1 class="text-center text-3xl mb-30">{currentFilePath}</h1>
    <div class="flex justify-center gap-7.5">
      {#if idleState}
        <button class="w-40 p-2.5" onclick={startRecording}>Start Recording</button>
        <button class="w-40 p-2.5" onclick={skipFile}>Skip File</button>
      {/if}
      {#if recordingState}
        <button class="w-40 p-2.5" onclick={cancelRecording}>Start Recording</button>
        <button class="w-40 p-2.5" onclick={stopRecording}>Skip File</button>
      {/if}
      {#if reviewingState}
        <button class="w-40 p-2.5" onclick={submitRecording}>Start Recording</button>
        <button class="w-40 p-2.5" onclick={rejectRecording}>Skip File</button>
      {/if}
    </div>
  </div>
  <div class="rounded-xl drop-shadow-2xl dark:bg-surface-container-dark">
    <button onclick={transcribeAudioFile}>TRANSCRIBE TS</button>
    <h1>{audioTranscription}</h1>
  </div>
</div>