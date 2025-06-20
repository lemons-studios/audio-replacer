<script lang="ts">
  import { onMount } from "svelte";
  import { setDetails, setState } from "../../util/DiscordRpc";
  import { selectFile } from "../../util/OsTools";
  import { transcribeFile } from "../../util/WhisperUtils";
  import { currentFileName, currentOutFile, isProjectLoaded, skipFile, submitFile, truncatedCurrentFile } from "../../Util/ProjectManager";
  import { remove } from "@tauri-apps/plugin-fs";
  import { startRecord, stopRecord, stopRecordPremature } from "../../Util/AudioRecordUtils";
  
  // I LOVE RUNES
  let idleState = $state(true);
  let recordingState = $state(false);
  let reviewingState = $state(false);

  let recordTabState = $state("Select A Folder To Begin")
  let audioTranscription = $state("No Transcription Yet");
  

  onMount(async () => {
    await setDetails("Recording");
    if(isProjectLoaded) {
      await projectLoadStateSet();
    }
  });

  async function projectLoadStateSet() {
    recordTabState = truncatedCurrentFile;
    await setState(currentFileName);
  }

  function switchStates() {
    if(idleState) {
      idleState = false;
      recordingState = true;
      recordTabState = "Recording";
      return;
    }
    else if (recordingState) {
      recordingState = false;
      reviewingState = true;
      recordTabState = "Review Your Recording";
      return;
    }
    else if (reviewingState) {
      reviewingState = false;
      idleState = true;
      recordTabState = truncatedCurrentFile;
      return;
    }
  }

  async function cancelRecording() {
    await stopRecordPremature();
    recordingState = false;
    idleState = true;
    recordTabState = truncatedCurrentFile;
  }

  async function stopRecording() {
    await stopRecord(currentOutFile);
    switchStates();
  }

  async function startRecording() {
    switchStates();
    await startRecord();
  }

  async function skipCurrentFile() {
    await skipFile();
    recordTabState = truncatedCurrentFile;
  }

  async function submitRecording() {
    await submitFile();
    switchStates();
  }

  async function rejectRecording() {
    await remove(currentOutFile);
    switchStates();
  }

  async function transcribeAudioFile(path: string) {
    const transcription = await transcribeFile(path);
    return transcription;
  }

  async function transcribeAudioFileDbg() {
    const file = await selectFile();
    const transcription = await transcribeFile(file);
    audioTranscription = transcription;
  }
</script>

<div class="grid grid-cols-[60%_40%] gap-5 w-full h-full">
  <div class="rounded-xl drop-shadow-2xl dark:bg-surface-container-dark p-5">
    <h1 class="text-center text-3xl mb-30">{recordTabState}</h1>
    <div class="flex justify-center gap-7.5 mb-5">
      {#if idleState}
        <md-filled-button class="min-w-35 p-2.5" onclick={skipFile}>Skip file</md-filled-button>
        <md-filled-button class="min-w-40 p-2.5" onclick={startRecording}>Start Recording</md-filled-button>
      {/if}
      {#if recordingState}
        <md-filled-button class="min-w-35 p-2.5" onclick={cancelRecording}>Cancel Recording</md-filled-button>
        <md-filled-button class="min-w-35 p-2.5" onclick={stopRecording}>Stop Recording</md-filled-button>
      {/if}
      {#if reviewingState}
        <md-filled-button class="min-w-35 p-2.5" onclick={submitRecording}>Reject</md-filled-button>
        <md-filled-button class="min-w-35 p-2.5" onclick={rejectRecording}>Accept</md-filled-button>
      {/if}
    </div>
    <div class="text-center">
      <md-text-button trailing-icon class="rounded-lg p-2" onclick={transcribeAudioFileDbg}>TRANSCRIBE TS <svg slot="icon" viewBox="0 0 48 48"><path d="M9 42q-1.2 0-2.1-.9Q6 40.2 6 39V9q0-1.2.9-2.1Q7.8 6 9 6h13.95v3H9v30h30V25.05h3V39q0 1.2-.9 2.1-.9.9-2.1.9Zm10.1-10.95L17 28.9 36.9 9H25.95V6H42v16.05h-3v-10.9Z"/></svg></md-text-button>
      <h1>{audioTranscription}</h1>
    </div>
  </div>
  <div class="rounded-xl drop-shadow-2xl dark:bg-surface-container-dark p-5">
  </div>
</div>