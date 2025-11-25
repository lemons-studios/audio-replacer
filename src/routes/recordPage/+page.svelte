<script lang="ts">
  import { onDestroy, onMount } from "svelte";
  import { setPresenceDetails, setPresenceState } from "../../tools/DiscordPresenceManager";
  import { calculateCompletion, countInputFiles, countOutputFiles, currentFile, discardfile, fileTranscription, projectLoaded, skipFile, submitFile } from "../../tools/ProjectHandler";
  import { cancelRecording, effectFilterNames, endRecording, pitchFilterNames, startRecording } from "./AudioManager";
  import AudioPlayer from "../../Components/AudioPlayer.svelte";
  import ProgressBar from "../../Components/ProgressBar.svelte";
  import { goto } from "$app/navigation";
  import { selectFile } from "../../tools/OsTools";

  let file = $state("No Project Opened");
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

  $effect(() => {
    if(!projectLoaded) return;
    file = currentFile.replaceAll("\\", "/");
    progressDecimal = calculateCompletion() / 100;
    progressPercentage = `${calculateCompletion()}%`;
    filesRemaining = new Intl.NumberFormat().format(countInputFiles() - countOutputFiles());
    transcription = fileTranscription;
    setPresenceState(`Files Remaining: ${filesRemaining}`);
  });

  onMount(() => {
    // Only refresh on page refresh since the only way they would change is if someone navigated to the editor route
    effects = effectFilterNames;
    pitch = pitchFilterNames;
  });

  onMount(async() => {
    await setPresenceDetails("Recording Audio");
  });

  onDestroy(async() => {
    await setPresenceState("");
  });
</script>

{#if projectLoaded}
<div class="flex flex-row gap-5 h-full">
  <div class="flex flex-col justify-center items-center w-5/8 dark:bg-secondary-d bg-secondary rounded-lg">
    <h1 class="font-medium text-2xl">{file}</h1>
    <h3 class="font-light text-sm text-gray-400 mb-25">Files Remaining: {filesRemaining}</h3>

    <div class="flex flex-row gap-1.5">
      <ProgressBar progress={progressDecimal}></ProgressBar>
      <h2>Files Remaining: {filesRemaining} ({progressPercentage})</h2>
    </div>
    <AudioPlayer source={currentFile}></AudioPlayer>
    <h3 class="font-light text-gray-300 mb-5">{transcription}</h3>
    <div class="flex flex-row gap-5">
      {#if idle}
      <button class="app-btn min-w-30" disabled onclick={() => {skipFile()}}>Skip</button>
      <button class="app-btn min-w-30" onclick={async() => {
        idle = false;
        recording = true;
        await startRecording();
      }}>Record</button>
      {:else if recording}
      <button class="app-btn min-w-30" onclick={() => {
        recording = false;
        idle = true;
        cancelRecording();
      }}>Discard</button>
      <button class="app-btn min-w-30" onclick={async() => {
        recording = false;
        reviewing = true;
        await endRecording(selectedPitch, selectedEffect)
      }}>End</button>
      {:else if reviewing}
      <button class="app-btn min-w-30" onclick={async() => {
        reviewing = false;
        idle = true;
        await discardfile();
      }}>Reject</button>
      <button class="app-btn min-w-30" onclick={async() => {
        reviewing = false;
        idle = true;
        await submitFile(extraEdits);
      }}>Accept</button>
      {/if}
    </div>
  </div>
  <div class="flex flex-col justify-center items-center w-3/8 dark:bg-secondary-d bg-secondary rounded-lg">
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
    <button class="app-btn" onclick={async() => {
      // File popup for project selection
      const file = await selectFile(['arproj'], 'Audio Replacer Projects');
    }} onmouseleave={(e) => e.currentTarget.blur} onmouseup={(e) => e.currentTarget.blur()}>Load Project</button>
    <button class="app-btn" onclick={() => goto('/')} onmouseleave={(e) => e.currentTarget.blur} onfocus={(e) => e.currentTarget.blur()}>Return Home</button>
  </div>
</div>
{/if}