<script lang="ts">
  import { onDestroy, onMount } from "svelte";
  import { setPresenceDetails, setPresenceState } from "../../tools/DiscordPresenceManager";
  import {
    calculateCompletion, countInputFiles, currentFile, discardFile,
    fileTranscription, getArprojProperty, localPath, openOutputFolder,
    outputFile, projectLoaded, skipFile, submitFile
  } from "../../tools/ProjectHandler";
  import { cancelRecording, effectFilterNames, endRecording, pitchFilterNames, startCapture } from "./AudioManager";
  import { format } from "../../tools/OsTools";
  import { register, unregisterAll } from "@tauri-apps/plugin-global-shortcut";
  import { exists } from "@tauri-apps/plugin-fs";
  import { getValue } from "../../tools/DataInterface";
  import { ArrowRightLeft, X, Mic, SkipForward, Slash, Square, Check, ExternalLink } from "@lucide/svelte";
  import AudioPlayer from "../../Components/AudioPlayer.svelte";
  import NoProjectLoaded from "../../Components/NoProjectLoaded.svelte";
  import ToggleSwitch from "../../Components/ToggleSwitch.svelte";
  import ProgressBar from "../../Components/ProgressBar.svelte";
  import Notification from "../../Components/Notification.svelte";
  import Dropdown from "../../Components/Dropdown.svelte";
  import { ask } from "@tauri-apps/plugin-dialog";

  let file = $state("No Project Opened");
  let audioSource = $state("");
  let rawProgress = $state(0);
  let progressPercentage = $state("0%");
  let filesRemaining = $state("0");
  let transcription = $state("Transcription Unavailable");
  let idle = $state(true);
  let recording = $state(false);
  let reviewing = $state(false);

  let effects: string[] = $state([]);
  let pitch: string[] = $state([]);

  let selectedPitch = 0;
  let selectedEffect = 0;

  // svelte-ignore non_reactive_update
  let extraEdits = false;

  // svelte-ignore non_reactive_update
  let notificationManager: Notification;

  const buttons = {
    idle: [
      {
        label: "Skip",
        icon: SkipForward,
        action: async() => {
          const confirmation = await ask('Are you sure you want to skip this file?', { title: 'Skip File', kind: 'warning' });
          if(!confirmation) return;
          await skipFile();
          notificationManager.addToNotification('success', 'Success!', 'Skipped File', true, 5000);
          updateContent();
        }
      },
      {
        label: "Record",
        icon: Mic,
        action: async() => {
          idle = false;
          recording = true;
          notificationManager.addToNotification('info', "Recording Started!", "Good Luck!", false, 5000);
          await startCapture();
        }
      }
    ],
    recording: [
      {
        label: 'Cancel',
        icon: Slash,
        action: async() => {
          recording = false;
          idle = true;
          await cancelRecording();
          notificationManager.addToNotification('success', "Recording Canceled", "", true, 5000);
        }
      },
      {
        label: 'End',
        icon: Square,
        action: async() => {
          recording = false;
          await endRecording(selectedPitch, selectedEffect);
          const autoAcceptRecordings = await getValue('settings.autoAcceptRecordings');
          if(autoAcceptRecordings) {
            await submitFile(extraEdits);
            notificationManager.addToNotification('success', "Accepted!", "You Have Auto-accept Enabled!", true, 5000);
            idle = true;
          }
          else {
            reviewing = true;
            notificationManager.addToNotification('info', "Recording Ended", "Entering Review Phase", true, 5000);
            audioSource = outputFile; // Automatically switch to output file
          }
        }
      }
    ],
    reviewing: [
      {
        label: 'Reject',
        icon: X,
        action: async() => {
          reviewing = false;
          idle = true;
          await discardFile();
          notificationManager.addToNotification('info', "File Rejected", "", true, 5000);
          audioSource = currentFile; // If cancelled whilst reviewing recorded audio
        }
      },
      {
        label: 'Switch',
        icon: ArrowRightLeft,
        action: async() => {
          if(!(await exists(outputFile))) return;
          audioSource = (audioSource === outputFile) ? currentFile : outputFile;
          notificationManager.addToNotification('info', "Switched", `You are now viewing ${audioSource === outputFile ? 'your recording' : 'the original file'}`, true, 5000);
        }
      },
      {
        label: 'Accept',
        icon: Check,
        action: async() => {
          reviewing = false;
          idle = true;
          await submitFile(extraEdits);
          notificationManager.addToNotification('success', "File Accepted!", "Congratulations!", true, 5000);
          audioSource = currentFile;
          updateContent();
        }
      }
    ]
  }

  const shortcuts = [
    {
      combination: 'CommandOrControl+R',
      action: async() => {
        if(idle) {
          await buttons.idle[1].action();
        }
        else if(recording) {
          await buttons.recording[1].action();
        }
        else {
          await buttons.reviewing[2].action();
        }
      }
    },
    {
      combination: 'CommandOrControl+Q',
      action: async() => {
        if(idle) {
          await buttons.idle[0].action();
        }
        else if(recording) {
          await buttons.recording[0].action();
        }
        else {
          await buttons.reviewing[0].action();
        }
      }
    },
    {
      combination: 'CommandOrControl+E',
      action: async() => {
        if(reviewing) {
          await buttons.reviewing[1].action();
        }
      }
    },
    {
      combination: 'CommandOrControl+F',
      action: () => {
        extraEdits = true;
      }
    }
  ];

  function updateContent() {
    if(!projectLoaded) return;
    file = localPath.replaceAll("\\", "/").substring(1);

    rawProgress = calculateCompletion();
    progressPercentage = `${rawProgress.toFixed(2)}%`;
    filesRemaining = format(countInputFiles());
    transcription = fileTranscription;
    audioSource = currentFile;
    setPresenceState(`Files Remaining: ${filesRemaining} (${progressPercentage})`);
  }

  onMount(async() => {
    if(!projectLoaded) {
      await setPresenceDetails("Projectless")
      return;
    }
    await setPresenceDetails(`Recording Audio in ${await getArprojProperty("name")}`);

    // Only refresh on page refresh since the only way they would change is if someone navigated to the editor route
    effects = effectFilterNames;
    pitch = pitchFilterNames;

    updateContent();

    for(let i = 0; i < shortcuts.length; i++) {
      await register(shortcuts[i].combination, shortcuts[i].action);
    }
  });

  onDestroy(async() => {
    await setPresenceState(""); // Remove presence state as the rest of the app doesn't use it
    await unregisterAll(); // Unregister all shortcuts
  })

  function getActiveState() {
    return (idle ? buttons.idle : recording ? buttons.recording : buttons.reviewing)
  }
</script>

{#if projectLoaded}
  <div class="notification-overlay">
    <Notification bind:this={notificationManager} />
  </div>

  <div class="flex flex-row gap-5 h-full">
  <div class="flex flex-col justify-center items-center w-5/8 card rounded-lg">
    <h1 class="font-medium text-2xl text-center">{file}</h1>
    <h3 class="font-light text-sm text-gray-400 mb-5">Files Remaining: {filesRemaining} ({progressPercentage})</h3>
    <div class="flex flex-row gap-1.5 w-8/10 justify-center mb-15 text-center items-center dark:text-gray-500 text-gray-700 text-xs">
     0% <ProgressBar completion={rawProgress}/> 100%
    </div>
    <AudioPlayer source={audioSource}/>
    <h3 class=" text-gray-800 dark:text-gray-300 text-center w-4/5 mb-5">{transcription}</h3>
    <div class="flex flex-row gap-5">
      {#each getActiveState() as button}
        <button class="app-btn min-w-30"
                onmouseleave={(e) => e.currentTarget.blur}
                onclick={async(e) => {e.currentTarget.blur(); await button.action()}}>
                <button.icon class="button-icon"/>{button.label}
        </button>
      {/each}
    </div>
  </div>
  <div class="flex flex-col justify-center items-center w-3/8 card rounded-lg gap-y-10">
    <div>
      <h1 class="text-2xl text-center mb-2.5">Pitch Shift</h1>
        <Dropdown values={pitch}
                  startingIndex={0}
                  onChange={(index) => {
                    selectedPitch = index
                  }}/>
    </div>
    <div>
    <h1 class="text-2xl text-center mb-2.5">Effect Filters</h1>
      <Dropdown values={effects}
                startingIndex={0}
                onChange={(index) => {
                  selectedEffect = index;
                }}/>
    </div>
    <div class="flex flex-row gap-x-3.5">
      <ToggleSwitch onClick={extraEdits = !extraEdits} enabled={extraEdits}/>
      <p>Extra Edits Required?</p>
    </div>
    <button class="app-btn-secondary min-w-60"
            onmouseleave={(e) => e.currentTarget.blur}
            onclick={async(e) => {e.currentTarget.blur(); await openOutputFolder()}}>
      <ExternalLink class="button-icon"/>Open Output Folder
    </button>
  </div>
</div>
{:else}
  <NoProjectLoaded/>
{/if}
