import { getValue, resetAll, resetSettings, resetStatistics, setValue } from "../../tools/DataInterface";
import { clearRichPresence, startRichPresence } from "../../tools/DiscordPresenceManager";
import { ask, message } from "@tauri-apps/plugin-dialog";
import { attemptRelaunch } from "../../tools/OsTools";
import { Trash2 } from "@lucide/svelte";
import { setTheme } from "@tauri-apps/api/app";
import { projectLoaded, sortInputFiles } from "../../tools/ProjectHandler";
import type {Theme} from "@tauri-apps/api/window";

export const settings = {
  General: [
    {
      name: 'Theme',
      description: 'Set the app theme',
      type: 'dropdown',
      choices: [
        "Light",
        "Dark"
      ],
      choiceValues: [
        "light",
        "dark"
      ],
      getValue: async(): Promise<string> => {
        return (await getValue('settings.theme'));
      },
      onChange: async(value: string) => {
        await setValue('settings.theme', value);
        const verifiedValue: Theme = value === 'dark' ? 'dark' : 'light';  // Fixes error
        await setTheme(verifiedValue);
      }
    },
    {
      name: "Check for updates",
      description: "",
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
      name: "Enable Usage Statistics",
      description: "Tracks some information about your usage on Audio Replacer to display on the home page. All statistics stay on-device",
      type: "boolean",
      onChange: async(value: boolean) => {
        await setValue('settings.allowStatistics', value);
      },
      getValue: async(): Promise<boolean> => {
        return (await getValue('settings.allowNoiseSuppression'));
      }
    },
    {
      name: "Enable Audio Transcription",
      description: "Enable Whisper audio-to-text transcription. Runs on-device",
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
      description: "Displays custom status on your Discord profile while Audio Replacer is running",
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
      description: "",
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
      description: "",
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
      description: "If a project is loaded, this takes effect on the next file",
      type: "dropdown",
      choices: [
          "Alphabetical",
          "Reverse Alphabetical",
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
        if(projectLoaded) {
          await sortInputFiles();
        }
      }
    }
  ],
  "Danger Zone": [
    {
      name: "Reset Statistics",
      description: "Changes will only occur after a restart",
      type: "button",
      buttonText: "Reset",
      icon: Trash2,
      onClick: async() => {
        await deletionConfirmation('Statistics');
      }
    },
    {
      name: "Reset Settings",
      description: "Changes will only occur after a restart",
      type: "button",
      buttonText: "Reset",
      icon: Trash2,
      onClick: async () => {
        await deletionConfirmation("Settings");
      }
    },
    {
      name: "Reset Settings & Statistics",
      description: "Changes will only occur after a restart",
      type: "button",
      buttonText: "Reset",
      icon: Trash2,
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
