<script lang="ts">
  import { onMount } from "svelte";
  import { setDetails, setState } from "../../util/DiscordRpc";
  import { selectFile } from "../../util/OsTools";
  import { transcribeFile } from "../../util/WhisperUtils";
  import { currentFileLocalPath, currentFileName, currentOutFile, isProjectLoaded, skipFile, submitFile } from "../../Util/ProjectManager";
  import { remove } from "@tauri-apps/plugin-fs";
  import { startRecord, stopRecord } from "../../Util/AudioRecordUtils";
  
  // I LOVE RUNES
  let idleState = $state(true);
  let recordingState = $state(false);
  let reviewingState = $state(false);

  let recordTabState = $state("Select A Folder To Begin")
  let audioTranscription = $state("No Transcription Yet");
  

  onMount(async () => {
    await setDetails("Recording");
    if(isProjectLoaded) {
      recordTabState = currentFileName;
      await setState(currentFileName);
    }
  });

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
      recordTabState = currentFileName;
      return;
    }
  }

  async function cancelRecording() {
    await remove(currentOutFile);
    recordingState = false;
    idleState = true;
    recordTabState = currentFileName;
  }

  async function stopRecording() {
    const outFile = currentOutFile;
    await stopRecord(outFile);
    switchStates();
  }

  async function startRecording() {
    switchStates();
    await startRecord();
  }

  async function skipCurrentFile() {
    await skipFile();
    recordTabState = currentFileLocalPath;
  }

  async function submitRecording() {
    await submitFile();
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

<div class="grid grid-cols-[60%_40%] gap-5 w-full h-full">
  <div class="rounded-xl drop-shadow-2xl dark:bg-surface-container-dark p-5">
    <h1 class="text-center text-3xl mb-30">{recordTabState}</h1>
    <div class="flex justify-center gap-7.5">
      {#if idleState}
        <md-filled-button class="w-40 p-2.5" onclick={skipFile}>Skip file</md-filled-button>
        <md-filled-button class="w-40 p-2.5" onclick={startRecording}>Start Recording</md-filled-button>
      {/if}
      {#if recordingState}
        <md-filled-button class="w-40 p-2.5" onclick={cancelRecording}>Cancel Recording</md-filled-button>
        <md-filled-button class="w-40 p-2.5" onclick={stopRecording}>Stop Recording</md-filled-button>
      {/if}
      {#if reviewingState}
        <md-filled-button class="w-40 p-2.5" onclick={submitRecording}>Reject</md-filled-button>
        <md-filled-button class="w-40 p-2.5" onclick={rejectRecording}>Accept</md-filled-button>
      {/if}
    </div>
  </div>
  <div class="rounded-xl drop-shadow-2xl dark:bg-surface-container-dark p-5">
    <button onclick={transcribeAudioFile}>TRANSCRIBE TS</button>
    <h1>{audioTranscription}</h1>
  </div>
</div>