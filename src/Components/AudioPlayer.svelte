<script lang="ts">
  import { Pause, Play } from "@lucide/svelte";
  import { onMount } from "svelte";
  let source = $props();
  let audioCompletion = $state(0.0);
  let progress = $state("00:00")
  let audioPlaying = $state(false);

  const audio = document.getElementById("audioPlayer") as HTMLAudioElement;
  const slider = document.getElementById("audioProgress") as HTMLInputElement;


  onMount(() => {
    
  })

  $effect(() => {

  })

  function playAudio() {
    audioPlaying = true;
    audio.play();
  }

  function pauseAudio() {
    audioPlaying = false;
    audio.pause();
  }

  function setAudioTime() {
    
  }
</script>

<style>

</style>

<div class="flex flex-row justify-center gap-4 bg-neutral-950 rounded-lg mx-auto items-center shadow-lg pl-3 pr-4 py-2">
    <audio id="audioPlayer">
        <source src={source}>
    </audio>
    <div>
        {#if !audioPlaying}
        <Play size="20" onclick={playAudio}/>
        {/if}
        {#if audioPlaying}
        <Pause size="20" onclick={pauseAudio}/>
        {/if}
    </div>
    <div class="flex flex-row gap-5">
        <input id="audioProgress" type="range" min="0" max="1" step="0.01" value={audioCompletion} onchange={setAudioTime}>
        <h2>{progress}</h2>
    </div>
</div>
