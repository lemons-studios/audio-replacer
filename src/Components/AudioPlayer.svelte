<script lang="ts">
  
  import { Pause, Play, Repeat } from "@lucide/svelte";
  import { convertFileSrc } from "@tauri-apps/api/core";
  import { exists } from "@tauri-apps/plugin-fs";
  import { error, warn } from "@tauri-apps/plugin-log";

  let { source } = $props();
  let audioCompletion = $state(0.0);
  let currentAudioTime = $state("00:00")
  let audioPlaying = $state(false);
  let loopEnabled = $state(true);
  
  let trueSource = $derived(() => { 
    if(!source) return null;
    return convertFileSrc(source);
  });

  let audioPlayer: HTMLAudioElement;
  let seekBar: HTMLInputElement;

  $effect(() => {
    if(audioPlayer && trueSource()) {
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
      warn("Audio Doesn't Exist")
      audioPlaying = false;
      return;
    }

    if(audioPlayer.readyState >= HTMLMediaElement.HAVE_FUTURE_DATA) {
        if(audioPlayer.paused ) {
        audioPlaying = true;
        try {
          audioPlayer.play();
        }
        catch(e: any) {
          error(`Audio playback failed: ${e}`);
        }
      }
      else {
        audioPlaying = false;
        audioPlayer.pause();
      }
    }
    else {
      warn(`Ready state too low ${audioPlayer.readyState}`)
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

  export function playAudio() {
    audioPlayer.play();
    audioPlaying = true;
  }

  export function pauseAudio() {
    audioPlayer.pause();
    audioPlaying = false;
  }
</script>

<div class="flex flex-row justify-center gap-4 bg-neutral-900 rounded-lg mx-auto items-center shadow-lg pl-3 pr-4 py-2">
    <audio ontimeupdate={audioPlayerTimeUpdate} onended={onAudioEnded} bind:this={audioPlayer}>
        <source src={trueSource()}>
    </audio>
    <div class="flex flex-row gap-3 items-center">
        {#if audioPlaying}
          <Pause size="23" onclick={toggleAudio} class="media-control-button hover:fill-white"/>
          {:else}
          <Play size="23" onclick={toggleAudio} class="media-control-button hover:fill-white"/>
        {/if}
        {#if loopEnabled}
          <Repeat size="19" color="#25ef1a" onclick={() => loopEnabled = false} class="media-control-button hover:fill-green-500"/>
          {:else}
          <Repeat size="19" color="white" onclick={() => loopEnabled = true} class="media-control-button hover:fill-white"/>
        {/if}
    </div>
    <div class="flex flex-row gap-5">
        <input type="range" class="range range-primary range-xs" min="0" max="1" step="0.01" value={audioCompletion} oninput={updateSeekbar} bind:this={seekBar}>
        <h2>{currentAudioTime}</h2>
    </div>
</div>
