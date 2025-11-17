<script lang="ts">
  import AudioPlayer from "../../Components/AudioPlayer.svelte";
  import { onDestroy, onMount } from "svelte";
  import { cancelRecording, effectFilterNames, endRecording, pitchFilterNames, startRecording } from "./AudioManager";
  import { register, unregisterAll } from "@tauri-apps/plugin-global-shortcut";
  import { calculateCompletion, countInputFiles, countOutputFiles, currentFile, discardfile, fileTranscription, localPath, outputFile, skipFile, submitFile } from "../../tools/ProjectHandler";
  
  let activeAudioPath = $state(currentFile || "");
  let currentPathTrunc = $state(localPath || "No File Selected");


  let completionPercentage = $state(`${calculateCompletion().toFixed(2)}%` || "0%");
  let completionValue = $state(calculateCompletion() / 100)

  let filesRemaining = $state(new Intl.NumberFormat().format(countInputFiles() - countOutputFiles()) || 0);
  let extraEditsFlagged = $state(false);
  let transcription = $state(fileTranscription);

  let idle = $state(true);
  let recording = $state(false);
  let reviewing = $state(false);

  let audioPlayer: AudioPlayer;
  let pitchDropdown: HTMLSelectElement;
  let effectDropdown: HTMLSelectElement;

  let selectedPitchIndex = 0; 
  let selectedEffectIndex = 0;

  const discardOptions = {
    discarding: true,
    notDiscarding: false
  } as const

  $effect(() => {
    activeAudioPath = currentFile;
    currentPathTrunc = localPath;
    completionValue = calculateCompletion() / 100;
    completionPercentage = `${calculateCompletion().toFixed(2)}%`;
    filesRemaining = new Intl.NumberFormat().format(countInputFiles() - countOutputFiles());
    transcription = fileTranscription;
  })

  onMount(async() => {
    const shortcuts = [
      {
        keybind: "CommandOrControl+R",
        action: async() => {
          if(idle) {
            await startRecord();
          }
          else if(recording) {
            await stopRecord();
          } 
          else {
            await finalizeRecording(discardOptions.notDiscarding)
          }
        }
      }, 
      {
        keybind: "CommandOrControl+Q",
        action: async() => {
          if(idle) {
            await skipFile();
          }
          else if(recording) {
            cancelRecord();
          }
          else {
            await finalizeRecording(discardOptions.discarding);
          };
        }
      },
      {
        keybind: "CommandOrControl+E",
        action: () => {
          switchAudio();
        }
      },
      {
        keybind: "CommandOrControl+F",
        action: () => {
          extraEditsFlagged = !extraEditsFlagged;
        }
      }
    ];

    for(let i = 0; i < shortcuts.length; i++) {
      await register(shortcuts[i].keybind, shortcuts[i].action);
    }
  })

  onDestroy(async() => {
    await unregisterAll();
  })

  async function startRecord() {
    switchStates();
    await startRecording();
  }

  async function stopRecord() {

  }

  function cancelRecord() {
    cancelRecording();
    recording = false;
    idle = true;
  }

  async function finalizeRecording(isDiscarding: boolean) {
    switchStates();
    isDiscarding ? await discardfile() : await submitFile(extraEditsFlagged);
  }

  async function skipCurrentFile() {
    await skipFile(true); // Hardcoded for now, change later
  }

  function switchAudio() {
    const outputPath = outputFile;
    const inputPath = currentFile;

    activeAudioPath = activeAudioPath == outputPath ? currentFile : outputPath; 
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
    selectedPitchIndex = pitchDropdown.selectedIndex;
  }

  function selectEffectValue() {
    selectedEffectIndex = effectDropdown.selectedIndex;
  }
</script>

<div class="flex grow flex-row h-full gap-4 content-center">
  <fieldset class="w-3/4 pane content-center">
    <legend class="fieldset-legend">Project</legend>
    <h1 class="title-text mb-5 font-semibold">{currentPathTrunc}</h1>
    <h2 class="text-center text-lg font-medium">Files Remaining: {filesRemaining} ({completionPercentage})</h2>
    <progress value={completionValue} class="mb-3.5 progress progress-primary m-auto" max="100"></progress>
    <AudioPlayer source={activeAudioPath} bind:this={audioPlayer}/>
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
        <button class="btn btn-primary w-25" onclick={() => finalizeRecording(discardOptions.discarding)}>Discard</button>
        <button class="btn btn-primary w-25" onclick={() => switchAudio()}>Switch</button>
        <button class="btn btn-primary w-25" onclick={() => finalizeRecording(discardOptions.notDiscarding)}>Submit</button>
      {/if}
    </div>
    <h2 class="text-center text-lg font-medium">{transcription}</h2>
  </fieldset>
  <fieldset class="w-2/4 pane content-center">
    <legend class="fieldset-legend">Recording Filters</legend>
    <h2 class="title-text mb-[2rem] font-semibold">Pitch Filters</h2>
    <select class="select select-primary select-md m-auto" bind:this={pitchDropdown} onchange={selectPitchValue}>
      {#each pitchFilterNames as name }
        <option>{name}</option>
      {/each}
    </select>
    <h2 class="title-text mb-[2rem] font-semibold">Audio Effects</h2>
    <select class="select select-primary select-md m-auto" bind:this={effectDropdown} onchange={selectEffectValue}>
      {#each effectFilterNames as name }
        <option>{name}</option>
      {/each}
    </select>
    <div class="flex flex-row justify-center gap-2">
      <input type="checkbox" checked={extraEditsFlagged}>
      <h4 class="text-xl">Requires Extra Edits?</h4>
    </div> 
  </fieldset>
</div>
