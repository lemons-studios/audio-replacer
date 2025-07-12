<script lang="ts">
  import { Pause, Play } from "@lucide/svelte";
  import { convertFileSrc } from "@tauri-apps/api/core";
  import { exists } from "@tauri-apps/plugin-fs";
  let { source } = $props();
  let audioCompletion = $state(0.0);
  let currentAudioTime = $state("00:00")
  let audioPlaying = $state(false);
  
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
      audioPlayer.addEventListener('loadedmetadata', () => {
        console.log("Metadata Loaded", audioPlayer.duration);
      }, {once: true});

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
    console.log(trueSource());
    // Magic numbers? more like, poo poo idk
    if(!trueSource || !(await doesAudioExist())) {
      console.warn("Audio Doesn't Exist")
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
          console.error(`Audio playback failed: ${e}`);
        }
      }
      else {
        audioPlaying = false;
        audioPlayer.pause();
      }
    }
    else {
      console.warn(audioPlayer.readyState)
    }
  }

  function isAudioValid(): boolean {
    return !isNaN(audioPlayer.duration);
  }

  async function doesAudioExist(): Promise<boolean> {
    return source !== "" || await exists(source);
  }
</script>

<div class="flex flex-row justify-center gap-4 bg-neutral-950 rounded-lg mx-auto items-center shadow-lg pl-3 pr-4 py-2">
    <audio ontimeupdate={audioPlayerTimeUpdate} onended={() => { audioPlaying = false; }} bind:this={audioPlayer}>
        <source src={trueSource()}>
    </audio>
    <div>
        {#if !audioPlaying}
          <Play size="20" onclick={toggleAudio}/>
        {/if}
        {#if audioPlaying}
          <Pause size="20" onclick={toggleAudio}/>
        {/if}
    </div>
    <div class="flex flex-row gap-5">
        <input type="range" min="0" max="1" step="0.01" value={audioCompletion} oninput={updateSeekbar} bind:this={seekBar}>
        <h2>{currentAudioTime}</h2>
    </div>
</div>
