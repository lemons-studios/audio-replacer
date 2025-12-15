<script lang="ts">
  import { onDestroy, onMount } from "svelte";
  import { setPresenceDetails, setPresenceState } from "../../tools/DiscordPresenceManager";
  import {
    calculateCompletion,
    countInputFiles,
    countOutputFiles,
    currentFile,
    discardFile,
    fileTranscription,
    localPath,
    outputFile,
    projectLoaded,
    setActiveProject,
    skipFile,
    submitFile
  } from "../../tools/ProjectHandler";
  import { cancelRecording, effectFilterNames, endRecording, pitchFilterNames, startRecording } from "./AudioManager";
  import { goto } from "$app/navigation";
  import { selectFile } from "../../tools/OsTools";
  import AudioPlayer from "../../Components/AudioPlayer.svelte";
  import ProgressBar from "../../Components/ProgressBar.svelte";
  import { register, unregisterAll } from "@tauri-apps/plugin-global-shortcut";
  import { exists } from "@tauri-apps/plugin-fs";
  import { getValue } from "../../tools/DataInterface";

  let file = $state("No Project Opened");
  let audioSource = $state("");
  let progressDecimal = $state(0);
  let progressPercentage = $state("0%");
  let filesRemaining = $state("0");
  let transcription = $state("Transcription Unavailable");
  let idle = $state(true);
  let recording = $state(false);
  let reviewing = $state(false);
  let extraEdits = false;

  let effects: string[] = $state([]);
  let pitch: string[] = $state([]);

  let selectedPitch = 0;
  let selectedEffect = 0;

  // svelte-ignore non_reactive_update
  let audioPlayer: AudioPlayer | undefined = undefined;

  const shortcuts = [
    {
      combination: 'CommandOrControl+R',
      action: async() => {
        if(idle) {

        }
        else if(recording) {

        }
        else if(reviewing) {

        }
      }
    }
  ];

  const buttons = {
    idle: [
      {
        label: "Skip",
        action: async() => {
          await skipFile();
          updateContent();
        }
      },
      {
        label: "Record",
        action: async() => {
          idle = false;
          recording = true;
          await startRecording();
        }
      }
    ],
    recording: [
      {
        label: 'Discard',
        action: async() => {
          recording = false;
          idle = true;
          await cancelRecording();
        }
      },
      {
        label: 'End',
        action: async() => {
          recording = false;
          await endRecording(selectedPitch, selectedEffect);
          const autoAcceptRecordings = getValue('settings.autoAcceptRecordings')
          if(autoAcceptRecordings) {
            await submitFile(extraEdits);
            idle = true;
          }
          else reviewing = true;
        }
      }
    ],
    reviewing: [
      {
        label: 'Reject',
        action: async() => {
          reviewing = false;
          idle = true;
          await discardFile();
        }
      },
      {
        label: 'Switch',
        action: async() => {
          if(!(await exists(outputFile))) return;
          audioSource = (audioSource === outputFile) ? currentFile : outputFile;
        }
      },
      {
        label: 'Accept',
        action: async() => {
          reviewing = false;
          idle = true;
          await submitFile(extraEdits);
          audioSource = currentFile;
          updateContent();
        }
      }
    ]
  }

  function updateContent() {
    if(!projectLoaded) return;
    file = localPath.replaceAll("\\", "/").substring(1);
    progressDecimal = calculateCompletion() / 100;
    progressPercentage = `${calculateCompletion().toFixed(2)}%`;
    filesRemaining = new Intl.NumberFormat().format(countInputFiles() - countOutputFiles());
    transcription = fileTranscription;
    audioSource = currentFile;
    setPresenceState(`Files Remaining: ${filesRemaining}`);
  }

  onMount(async() => {
    if(!projectLoaded) return;
    await setPresenceDetails("Recording Audio");

    // Only refresh on page refresh since the only way they would change is if someone navigated to the editor route
    effects = effectFilterNames;
    pitch = pitchFilterNames;

    for(let i = 0; i < shortcuts.length; i++) {
      await register(shortcuts[i].combination, shortcuts[i].action);
    }

    updateContent();
  });

  onDestroy(async() => {
    await setPresenceState(""); // Remove presence state as the rest of the app doesn't use it
    await unregisterAll(); // Unregister all shortcuts
  });

  function getActiveState() {
    return (idle ? buttons.idle : recording ? buttons.recording : buttons.reviewing)
  }
</script>

{#if projectLoaded}
<div class="flex flex-row gap-5 h-full">
  <div class="flex flex-col justify-center items-center w-5/8 card rounded-lg">
    <h1 class="font-medium text-2xl text-center">{file}</h1>
    <h3 class="font-light text-sm text-gray-400 mb-25">Files Remaining: {filesRemaining} ({progressPercentage})</h3>

    <div class="flex flex-row gap-1.5">
      <ProgressBar progress={progressDecimal}></ProgressBar>
    </div>
    <AudioPlayer bind:this={audioPlayer} source={audioSource}></AudioPlayer>
    <h3 class="font-light text-gray-300 mb-5">{transcription}</h3>
    <div class="flex flex-row gap-5">
      {#each getActiveState() as button}
        <button class="app-btn min-w-30"
                onmouseleave={(e) => e.currentTarget.blur}
                onclick={async(e) => {e.currentTarget.blur(); await button.action()
                }}>
          {button.label}</button>
      {/each}
    </div>
  </div>
  <div class="flex flex-col justify-center items-center w-3/8 card rounded-lg">
    <h1 class="text-2xl">Filters</h1>
    <h3>Pitch</h3>
    {#if pitch.length !== 0}
    <select class="min-w-50 dropdown" onchange={(e) => {selectedPitch = e.currentTarget.selectedIndex}}>
      {#each pitch as p}
        <option>{p}</option>
      {/each}
    </select>
    {:else}
    <select class="min-w-50 dropdown" disabled>
      <option>No Pitch Filters</option>
    </select>
    {/if}
    <h3 class="mt-25 text-lg">Effects</h3>
    {#if effects.length !== 0}
      <select class="min-w-50 dropdown" onchange={(e) => {selectedEffect = e.currentTarget.selectedIndex}}>
        {#each effects as e}
          <option>{e}</option>
        {/each}
      </select>
    {:else}
    <select class="min-w-50 dropdown" disabled>
      <option>No Effect Filters</option>
    </select>
    {/if}
  </div>
</div>
{:else}
<div class="w-full h-full rounded-lg card flex flex-col gap-5 justify-center text-center items-center">
  <h1 class="text-center text-5xl font-bold">No Project Loaded</h1>
  <h2 class="text-3xl font-medium">Go Load One</h2>
  <div class="flex flex-row justify-center gap-3">
    <button class="app-btn"
            onclick={async(e) => {
              e.currentTarget.blur();
              const file = await selectFile(['arproj'], 'Audio Replacer Projects');
              await setActiveProject(file);
            }}
            onmouseleave={(e) => e.currentTarget.blur}>
            Load Project
    </button>
    <button class="app-btn"
            onclick={async(e) => {e.currentTarget.blur(); await goto('/')}}
            onmouseleave={(e) => e.currentTarget.blur}>Return Home
    </button>
  </div>
</div>
{/if}