<script lang="ts">
  import { onMount } from "svelte";
  import { setDetails } from "../../util/DiscordRpc";
  import { selectFile } from "../../util/OsTools";
  import { invoke } from "@tauri-apps/api/core";
  import { transcribeFile } from "../../util/WhisperUtils";
  
  // I LOVE RUNES
  let idleState = $state(true);
  let recordingState = $state(false);
  let reviewingState = $state(false);

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

  async function transcribeAudioFile() {
    const file = await selectFile();
    const transcription = await transcribeFile(file);
    audioTranscription = transcription;
  }
</script>

<button on:click={transcribeAudioFile}>TRANSCRIBE TS</button>
<h1>{audioTranscription}</h1>

<div class="grid grid-cols-2 gap-5">
  <div>

  </div>
  <div>

  </div>
</div>