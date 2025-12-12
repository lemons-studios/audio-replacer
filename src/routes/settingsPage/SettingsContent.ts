import { getValue, setValue } from "../../tools/DataInterface";

export const settings = {
  General: [
    {
      name: "Check for updates",
      description: "You will be prompted to install the update after the update is downloaded",
      type: "boolean",
      onChange: async(value: boolean) => {
        await setValue('settings.updateCheck', value);
      },
      getValue: (): boolean => {
        return getValue('settings.updateCheck')
      },
    },
    {
      name: "Enable Audio Transcription",
      description: "Enable Whisper audio-to-text transcription. Runs 100% locally on your device",
      type: "boolean",
      onChange: async(value: boolean) => {
        await setValue("settings.enableTranscription", value);
      },
      getValue: (): boolean => {
        return getValue('settings.enableTranscription');
      },
    },
    {
      name: "Enable Discord Rich Presence",
      description: "Displays general info on what you're doing to all your friends on Discord",
      type: "boolean",
      onChange: async(value: boolean) => {
        await setValue('settings.enableRichPresence', value);
      },
      getValue: (): boolean => {
        return getValue('settings.enableRichPresence');
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
      getValue: (): string => {
        return getValue('settings.recordStartDelay')
      },
    },
    {
      name: "Record End Delay",
      description: "Measured in milliseconds",
      type: "string",
      onChange: async(value: string) => {
        await setValue('settings.recordEndDelay', +value);
      },
      getValue: (): string => {
        return getValue('settings.recordEndDelay');
      },
    },
    {
      name: "Enable Noise Suppression",
      description: "Apply noise suppression on your recordings",
      type: "boolean",
      onChange: async(value: boolean) => {
        await setValue('settings.allowNoiseSuppression', value);
      },
      getValue: (): boolean => {
        return getValue('settings.allowNoiseSuppression');
      },
    },
    {
      name: "Auto-Accept Recordings",
      description: "You are committed!",
      type: "boolean",
      onChange: async(value: boolean) => {
        await setValue('settings.autoAcceptRecordings', value);
      },
      getValue: (): boolean => {
        return getValue('settings.autoAcceptRecordings');
      },
    },
    {
      name: "File Sorting Order",
      description: "What order Audio Replacer will sort audio files in",
      type: "dropdown",
      choices: [
          "Alphabetical (A-Z)",
          "Reverse-Alphabetical (Z-A)",
          "Random"
      ],
      getValue: (): string => {
        return getValue('settings.sortingMethod');
      },
      onChange: async(value: string) => {
        await setValue('settings.sortingMethod', value);
      }
    }
  ]
} as const;
