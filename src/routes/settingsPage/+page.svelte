<script lang="ts">
  import { onMount } from "svelte";
  import { setDetails } from "../../app/DiscordRpc";
  import { setValue } from "../../tools/SettingsManager";
  import { resolveResource } from "@tauri-apps/api/path";
  import { relaunch } from "@tauri-apps/plugin-process";
  import { writeTextFile } from "@tauri-apps/plugin-fs";
  onMount(async() => {
      await setDetails("Settings")
  });

  const defaultEffectData = [
    [
        "",
        "Default"
    ],
    [
        "aecho=0.8:0.35:17",
        "Flashback"
    ]
  ]

  const defaultPitchData = [
    [
        "1.00",
        "Default"
    ],
    [
        "2.00",
        "Super High Pitch"
    ],
    [
        "0.25",
        "Super Low Pitch"
    ]
  ]

  export const settings = {
    "General": [
      {
        name: "Check for updates",
        description: "You will be prompted to install the update after the update is downloaded",
        type: "boolean",
        onChange: (value: boolean) => {
          setValue("updateCheck", value);
        }
      },
      {
        name: "Autoload last project",
        description: "Load your last project automatically on app launch instead of loading into the home menu",
        type: "boolean",
        onChange: (value: boolean) => {
          setValue("autoloadProject", value);
        }
      },
      {
        name: "Enable Audio Transcription",
        description: "Uses the whisper text transcription model to transcribe the current file you are on. Little to no performance impact",
        type: "boolean",
        onChange: (value: boolean) => {
          setValue("enableTranscription", value);
        }
      },
      {
        name: "Enabe Discord Rich Presence",
        description: "Displays general info on what you're doing to all your friends on Discord",
        type: "boolean",
        onChange: (value: boolean) => {
          setValue("enableRichPresence", value);
        }
      }
    ],
    "Recording": [
      {
        name: "Record Start Delay",
        description: "Measured in milliseconds",
        type: "string",
        defaultValue: "10",
        onChange: (value: boolean) => {
          setValue("recordStartDelay", value);
        }
      },
      {
        name: "Record End Delay",
        description: "Measured in milliseconds",
        type: "string",
        defaultValue: "50",
        onChange: (value: boolean) => {
          setValue("recordEndDelay", value);
        }
      },
      {
        name: "Auto-Accept Recordings",
        description: "You are committed!",
        type: "boolean",
        onChange: (value: boolean) => {
          setValue("autoAcceptRecordings", value);
        }
      }
    ],
    "Danger Zone": [
      {
        name: "Delete custom pitch data",
        description: "Pitch data file will reset to default",
        type: "button",
        buttonText: "Delete",
        onclick: async() => {
          const file = await resolveResource("resources/pitchData.json");
          await writeTextFile(file, JSON.stringify(defaultPitchData));
          await relaunch();
        }
      },
      {
        name: "Delete custom effect data",
        description: "Effect data file will reset to default",
        type: "button",
        buttonText: "Delete",
        onclick: async() => {
          const file = await resolveResource("resources/effectData.json");
          await writeTextFile(file, JSON.stringify(defaultEffectData));
          await relaunch();
        }
      },
      {
        name: "Delete All Custom Data",
        description: "Deletes custom effect and pitch data",
        type: "button",
        buttonText: "Delete",
        onclick: async() => {
          const pitchData = await resolveResource("resources/pitchData.json");
          const effectData = await resolveResource("resources/effectData.json");
          await writeTextFile(pitchData, JSON.stringify(defaultPitchData));
          await writeTextFile(effectData, JSON.stringify(defaultEffectData));
          await relaunch();
        }
      },
      {
        name: "Delete EVERYTHING",
        description: "Deletes custom effect and pitch data, and resets your statistics",
        type: "button",
        buttonText: "Delete",
        onclick: async() => {
          const pitchData = await resolveResource("resources/pitchData.json");
          const effectData = await resolveResource("resources/effectData.json");
          await writeTextFile(pitchData, JSON.stringify(defaultPitchData));
          await writeTextFile(effectData, JSON.stringify(defaultEffectData));

          await relaunch();
        }
      }
    ]
  };

</script>

<div class="flex flex-grow flex-col items-center overflow-y-auto">
  {#each Object.entries(settings) as [name, settingCategory], sIndex}
    <fieldset class="pane w-3/4 ${sIndex > 0 ? "mt-4" : ""} ${sIndex == 2 ? "border-error" : ""}">
      <legend class="fieldset-legend">{name}</legend>
      {#each settingCategory as setting, index }
        <div class="flex justify-between items-center px-4">
          <div>
            <p class="font-semibold">{setting.name}</p>
            <p class="text-sm text-gray-400">{setting.description}</p>
          </div>
          {#if setting.type == "boolean"}
            <input type="checkbox" class="toggle"> 
          {:else if setting.type == "string"}
            <input type="text" placeholder={setting.defaultValue} class="input max-w-1/12">
          {:else if setting.type == "button"}
            <button class="btn btn-primary btn-md" onclick={async() => await setting.onClick}>{setting.buttonText}</button>
         <!-- {:else if setting.type == "dropdown"}
            <select class="select">
              {#each setting.choices as choice}
                <option>{choice}</option>
              {/each}
            </select> -->
          {/if}
        </div>
      {/each}
    </fieldset>
  {/each}
</div>
