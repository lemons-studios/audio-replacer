<script lang="ts">
    import { Pause, Play, Infinity, Volume2 } from "@lucide/svelte";
    import { exists, readFile } from "@tauri-apps/plugin-fs";
    import { error, warn } from "@tauri-apps/plugin-log";
    import { onMount, untrack } from "svelte";
    import { slide } from "svelte/transition";

    let { source } = $props();
    let audioCompletion = $state(0.0);
    let audioVolume = $state(0.5);
    let currentAudioTime = $state("00:00");
    let audioPlaying = $state(false);
    let loopEnabled = $state(true);
    let volumeVisible = $state(false);

    let currentAudioURL: string | undefined = undefined;

    let audioPlayer: HTMLAudioElement;
    let seekBar: HTMLInputElement;

    // svelte-ignore non_reactive_update
    let volumeSlider: HTMLInputElement;

    // svelte-ignore non_reactive_update
    let volumeSliderDiv: HTMLDivElement;

    onMount(() => {
        const handleOutsideClick = (e: MouseEvent) => {
            const target = e.target as Node;
            if (
                volumeSliderDiv &&
                !volumeSliderDiv.contains(target) &&
                volumeVisible
            ) {
                volumeVisible = false;
            }
        };

        document.addEventListener("click", handleOutsideClick);
        return () => {
            document.removeEventListener("click", handleOutsideClick);
        };
    });

    $effect(() => {
        source;
        try {
            URL.revokeObjectURL(currentAudioURL as string);
            currentAudioURL = undefined;
        } catch (e: any) {
            warn(
                `Unable to clear audio object URL (Likely because audio was never played) ${e}`,
            );
        }

        untrack(() => {
            if (audioPlayer) {
                audioPlayer.pause();
                audioPlayer.src = "";

                audioPlaying = false;
                audioCompletion = 0.0;
                loadAudioChunks();
            }
        });
    });

    function updateSeekbar() {
        if (isAudioValid()) {
            audioPlayer.currentTime =
                audioPlayer.duration * parseFloat(seekBar.value);
        }
    }

    function audioPlayerTimeUpdate() {
        if (isAudioValid()) {
            audioCompletion = audioPlayer.currentTime / audioPlayer.duration;
            seekBar.value = String(Math.round(audioCompletion));

            const min = Math.floor(audioPlayer.currentTime / 60);
            const sec = Math.floor(audioPlayer.currentTime % 60);
            currentAudioTime = `${String(min).padStart(2, "0")}:${String(sec).padStart(2, "0")}`;
        }
    }

    export async function loadAudioChunks() {
        const mimeType = () => {
            return source.endsWith(".wav")
                ? "audio/wav"
                : source.endsWith(".mp3")
                  ? "audio/mpeg"
                  : source.endsWith(".ogg")
                    ? "audio/ogg"
                    : source.endsWith(".flac")
                      ? "audio/flac"
                      : "application/octet-stream"; // Unknown type
        };
        try {
            const fileContents = await readFile(source);
            const type = mimeType();
            if (type === "application/octet-stream") {
                await error(
                    `${source} does not contain a mime type supported by this component`,
                );
                return;
            }
            const audioBlob = new Blob([fileContents], { type: type });
            currentAudioURL = URL.createObjectURL(audioBlob);
            audioPlayer.src = currentAudioURL;

            await audioPlayer.play();
            audioPlaying = true;
        } catch (e: any) {
            await error(`Error while loading audio at path ${source}: ${e}`);
            return;
        }
    }

    async function toggleAudio() {
        if (!source || !(await doesAudioExist())) {
            await warn("Audio Doesn't Exist");
            audioPlaying = false;
            return;
        }

        if (typeof currentAudioURL === "undefined") {
            await loadAudioChunks(); // Only if audio chunks don't exist by now should they be loaded. From my testing, this should never happen. This is only here in case it somehow does
        }

        if (audioPlayer.readyState >= HTMLMediaElement.HAVE_FUTURE_DATA) {
            if (audioPlayer.paused) {
                audioPlaying = true;
                try {
                    await audioPlayer.play();
                } catch (e: any) {
                    await error(`Audio playback failed: ${e}`);
                }
            } else {
                audioPlaying = false;
                audioPlayer.pause();
            }
        } else {
            await warn(`Ready state too low (${audioPlayer.readyState})`);
        }
    }

    function onAudioEnded() {
        if (loopEnabled) {
            audioPlayer.currentTime = 0;
            audioPlayer.play();
            return;
        }

        audioPlaying = false;
    }

    function isAudioValid(): boolean {
        return !isNaN(audioPlayer.duration) && audioPlayer.duration >= 0;
    }

    async function doesAudioExist(): Promise<boolean> {
        return source !== "" || (await exists(source));
    }

    function updateVolume() {
        audioVolume = +volumeSlider.value / 100;
    }
</script>

<div
    class="flex flex-row justify-center gap-4 bg-tertiary dark:bg-tertiary-d rounded-lg mx-auto items-center shadow-lg pl-3 pr-4 py-2 mb-2 w-3/4"
>
    <audio
        ontimeupdate={audioPlayerTimeUpdate}
        onended={onAudioEnded}
        bind:this={audioPlayer}
        preload="auto"
        volume={audioVolume}
    ></audio>
    <div class="flex flex-row gap-3 items-center">
        <!--Play/Pause Button-->
        <div
            class="transition hover:text-neutral-400 focus:text-navigation-focus rounded-sm p-0.5"
        >
            {#if audioPlaying}
                <Pause class="w-5 h-5" onclick={toggleAudio}></Pause>
            {:else}
                <Play class="w-5 h-5" onclick={toggleAudio}></Play>
            {/if}
        </div>
        <!--Volume Slider-->
        <div class="relative flex items-center" bind:this={volumeSliderDiv}>
            <Volume2
                class="w-5 h-5 hover:text-neutral-400"
                onclick={() => (volumeVisible = !volumeVisible)}
            />
            {#if volumeVisible}
                <div
                    class="absolute bottom-full left-1/2 -translate-x-1/2 min-w-50 rounded-lg bg-tertiary dark:bg-tertiary-d p-2 flex flex-row justify-between items-center text-center mb-2"
                    transition:slide={{ duration: 75 }}
                >
                    <input
                        type="range"
                        class="w-3/4"
                        min="0"
                        max="100"
                        step="1"
                        value={audioVolume * 100}
                        oninput={updateVolume}
                        bind:this={volumeSlider}
                    />
                    <p>{Math.round(audioVolume * 100)}%</p>
                </div>
            {/if}
        </div>

        <!--Loop Button-->
        {#if loopEnabled}
            <Infinity
                class="w-5.5 h-5.5 text-accent-secondary hover:text-accent transition duration-200"
                onclick={() => (loopEnabled = false)}
            />
        {:else}
            <Infinity
                class="w-5.5 h-5.5 text-white hover:text-gray-400 transition duration-200"
                onclick={() => (loopEnabled = true)}
            />
        {/if}
    </div>
    <div class="flex flex-row gap-5 w-full">
        <input
            type="range"
            class="w-[95%]"
            min="0"
            max="1"
            step="0.001"
            value={audioCompletion}
            oninput={updateSeekbar}
            bind:this={seekBar}
        />
        <h2>{currentAudioTime}</h2>
    </div>
</div>
