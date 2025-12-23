import {getValue, resetAll, resetSettings, resetStatistics, setValue} from "../../tools/DataInterface";
import { clearRichPresence, startRichPresence } from "../../tools/DiscordPresenceManager";
import {ask, message} from "@tauri-apps/plugin-dialog";
import {attemptRelaunch} from "../../tools/OsTools";

export const settings = {
  General: [
    {
      name: "Check for updates",
      description: "You will be prompted to install the update after the update is downloaded",
      type: "boolean",
      onChange: async(value: boolean) => {
        // Updates checked on next restart, no need to check
        await setValue('settings.updateCheck', value);
      },
      getValue: async(): Promise<boolean> => {
        return (await getValue('settings.updateCheck'));
      },
    },
    {
      name: "Enable Audio Transcription",
      description: "Enable Whisper audio-to-text transcription. Runs 100% locally on your device",
      type: "boolean",
      onChange: async(value: boolean) => {
        await setValue("settings.enableTranscription", value);
      },
      getValue: async(): Promise<boolean> => {
        return (await getValue('settings.enableTranscription'));
      },
    },
    {
      name: "Enable Discord Rich Presence",
      description: "Displays general info on what you're doing to all your friends on Discord",
      type: "boolean",
      onChange: async(value: boolean) => {
        await setValue('settings.enableRichPresence', value);
        if(value) {
          await startRichPresence();
        }
        else {
          await clearRichPresence();
        }
      },
      getValue: async(): Promise<boolean> => {
        return (await getValue('settings.enableRichPresence'));
      },
    }
  ],
  Recording: [
    {
      name: "Record Start Delay",
      description: "Measured in milliseconds",
      type: "string",
      onChange: async(value: string) => {
        await setValue('settings.recordStartDelay', +value);
      },
      getValue: async(): Promise<string> => {
        return (await getValue('settings.recordStartDelay'));
      },
    },
    {
      name: "Record End Delay",
      description: "Measured in milliseconds",
      type: "string",
      onChange: async(value: string) => {
        await setValue('settings.recordEndDelay', +value);
      },
      getValue: async(): Promise<string> => {
        return (await getValue('settings.recordEndDelay'));
      },
    },
    {
      name: "Enable Noise Suppression",
      description: "Apply noise suppression on your recordings",
      type: "boolean",
      onChange: async(value: boolean) => {
        await setValue('settings.allowNoiseSuppression', value);
      },
      getValue: async(): Promise<boolean> => {
        return (await getValue('settings.allowNoiseSuppression'));
      },
    },
    {
      name: "Auto-Accept Recordings",
      description: "You are committed!",
      type: "boolean",
      onChange: async(value: boolean) => {
        await setValue('settings.autoAcceptRecordings', value);
      },
      getValue: async(): Promise<boolean> => {
        return (await getValue('settings.autoAcceptRecordings'));
      },
    },
    {
      name: "File Sorting Order",
      description: "What order Audio Replacer will sort audio files in. Takes effect after reloading a project",
      type: "dropdown",
      choices: [
          "Alphabetical (A-Z)",
          "Reverse-Alphabetical (Z-A)",
          "Random"
      ],
      choiceValues: [
          "alphabetical",
          "reverseAlphabetical",
          "random"
      ],
      getValue: async(): Promise<string> => {
        return (await getValue('settings.sortingMethod'));
      },
      onChange: async(value: string) => {
        console.log(`Setting sorting method to ${value}`)
        await setValue('settings.sortingMethod', value);
      }
    }
  ],
  "Danger Zone": [
    {
      name: "Reset Statistics",
      description: "Changes will only occur after a restart",
      type: "button",
      buttonText: "Reset",
      onClick: async() => {
        await deletionConfirmation('Statistics');
      }
    },
    {
      name: "Reset Settings",
      description: "Changes will only occur after a restart",
      type: "button",
      buttonText: "Reset",
      onClick: async () => {
        await deletionConfirmation("Settings");
      }
    },
    {
      name: "Reset Settings & Statistics",
      description: "Changes will only occur after a restart",
      type: "button",
      buttonText: "Reset",
      onClick: async () => {
        await deletionConfirmation("All data");
      }
    }
  ]
} as const;

const deletionConfirmation = async(dataType: string) => {
  const confirmation = await ask('Are you sure? This action cannot be reverted', {
    title: 'Confirm?',
    kind: 'warning'
  });
  if(confirmation) {
    switch(dataType) {
      case "Statistics":
        await resetStatistics();
        break;
      case "Settings":
        await resetSettings();
        break;
      case "All Data":
        await resetAll();
        break;
    }
    await message(`${dataType} reset completed. App will now restart`);
    await attemptRelaunch();
  }
}
