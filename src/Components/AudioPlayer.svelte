<script lang="ts">
  import IconPlayRegular from 'phosphor-icons-svelte/IconPlayRegular.svelte';
  import IconPauseRegular from 'phosphor-icons-svelte/IconPauseRegular.svelte';
  import IconRepeatRegular from 'phosphor-icons-svelte/IconRepeatRegular.svelte';

  import { convertFileSrc } from "@tauri-apps/api/core";
  import { exists } from "@tauri-apps/plugin-fs";
  import { error, warn } from "@tauri-apps/plugin-log";

  let { source } = $props();
  let audioCompletion = $state(0.0);
  let currentAudioTime = $state("00:00")
  let audioPlaying = $state(false);
  let loopEnabled = $state(true);
  
  let trueSource = $derived(source ? convertFileSrc(source) : null);

  let audioPlayer: HTMLAudioElement;
  let seekBar: HTMLInputElement;

  $effect(() => {
    if(audioPlayer && trueSource) {
      audioPlayer.pause();
      audioPlayer.load();
      
      audioPlaying = false;
      audioCompletion = 0.0;
    }
  })

  function updateSeekbar() {
    if(isAudioValid()) {
      audioPlayer.currentTime = audioPlayer.duration * parseFloat(seekBar.value);
    }
  }

  function audioPlayerTimeUpdate() {
    if(isAudioValid()) {
      audioCompletion = audioPlayer.currentTime / audioPlayer.duration;
      seekBar.value = String(audioCompletion);

      const min = Math.floor(audioPlayer.currentTime / 60);
      const sec = Math.floor(audioPlayer.currentTime % 60);
      currentAudioTime = `${String(min).padStart(2, '0')}:${String(sec).padStart(2, '0')}`;
    }
  }

  async function toggleAudio() {    
    if(!trueSource || !(await doesAudioExist())) {
      await warn("Audio Doesn't Exist")
      audioPlaying = false;
      return;
    }

    if(audioPlayer.readyState >= HTMLMediaElement.HAVE_FUTURE_DATA) {
        if(audioPlayer.paused ) {
        audioPlaying = true;
        try {
          await audioPlayer.play();
        }
        catch(e: any) {
          await error(`Audio playback failed: ${e}`);
        }
      }
      else {
        audioPlaying = false;
        audioPlayer.pause();
      }
    }
    else {
      await warn(`Ready state too low ${audioPlayer.readyState}`)
    }
  }

  function onAudioEnded() {
    if(loopEnabled) {
      audioPlayer.currentTime = 0;
      audioPlayer.play();
      return;
    }
    
    audioPlaying = false;
  }

  function isAudioValid(): boolean {
    return !isNaN(audioPlayer.duration);
  }

  async function doesAudioExist(): Promise<boolean> {
    return source !== "" || await exists(source);
  }
</script>

<div class="flex flex-row justify-center gap-4 bg-neutral-900 rounded-lg mx-auto items-center shadow-lg pl-3 pr-4 py-2">
    <audio ontimeupdate={audioPlayerTimeUpdate} onended={onAudioEnded} bind:this={audioPlayer} preload="auto">
        <source src={trueSource}>
    </audio>
    <div class="flex flex-row gap-3 items-center">
        {#if audioPlaying}
          <button onclick={toggleAudio}><IconPauseRegular class="media-control-button hover:fill-white w-5.5 h-5.5"/></button>
        {:else}
          <button onclick={toggleAudio}><IconPlayRegular class="media-control-button hover:fill-white w-5.5 h-5.5"/></button>
        {/if}
        {#if loopEnabled}
          <button onclick={() => loopEnabled = false}><IconRepeatRegular class="media-control-button hover:fill-green-500 fill-green-400 w-5.5 h-5.5"/></button>
          {:else}
          <button onclick={() => loopEnabled = true}><IconRepeatRegular class="media-control-button hover:fill-white fill-gray-200 w-5.5 h-5.5"/></button>
        {/if}
    </div>
    <div class="flex flex-row gap-5">
        <input type="range" class="range range-primary range-xs" min="0" max="1" step="0.01" value={audioCompletion} oninput={updateSeekbar} bind:this={seekBar}>
        <h2>{currentAudioTime}</h2>
    </div>
</div>
