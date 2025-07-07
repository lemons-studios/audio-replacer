<script lang="ts">
  import { onMount } from "svelte";
  import { setDetails, setState } from "../../util/DiscordRpc";
  import { selectFile } from "../../util/OsTools";
  import { transcribeFile } from "../../util/WhisperUtils";
  import { completionPercentage, currentFile, currentFileName, currentOutFile, isProjectLoaded, skipFile, submitFile, truncatedCurrentFile } from "../../Util/ProjectManager";
  import { remove } from "@tauri-apps/plugin-fs";
  import { startRecord, stopRecord, stopRecordPremature } from "../../Util/AudioRecordUtils";
  
  // I LOVE RUNES
  let idleState = $state(true);
  let recordingState = $state(false);
  let reviewingState = $state(false);
  let currentCompletion = $state(completionPercentage)

  let recordTabState = $state("Select A Folder To Begin")
  let audioTranscription = $state("No Transcription Yet");
  

  onMount(async () => {
    await setDetails("Recording");
    if(isProjectLoaded) {
      await projectLoadStateSet();
      audioTranscription = `Transcription: ${await transcribeAudioFile(currentFile)}`;
    }
  });

  async function projectLoadStateSet() {
    recordTabState = truncatedCurrentFile;
    await setState(currentFileName);
  }

  async function switchStates() {
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
      recordTabState = currentFile;
      audioTranscription = `Transcription: ${await transcribeAudioFile(currentFile)}`;
      return;
    }
  }

  async function cancelRecording() {
    await stopRecordPremature();
    recordingState = false;
    idleState = true;
    recordTabState = currentFile;
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
    recordTabState = currentFile;
    audioTranscription = `Transcription: ${await transcribeAudioFile(currentFile)}`;
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
  
</div>