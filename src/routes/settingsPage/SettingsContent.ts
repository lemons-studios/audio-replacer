import { getValue, setValue } from "../../tools/SettingsManager";
import { resolveResource } from "@tauri-apps/api/path";
import { relaunch } from "@tauri-apps/plugin-process";
import { readTextFile, writeTextFile } from "@tauri-apps/plugin-fs";
import { selectFile } from "../../tools/OsTools";

export const settings = {
  General: [
    {
      name: "Check for updates",
      description: "You will be prompted to install the update after the update is downloaded",
      type: "boolean",
      onChange: (value: boolean) => {
        setValue("updateCheck", value);
      },
      getValue: async (): Promise<boolean> => {
        return (await getValue("updateCheck")) as boolean;
      },
    },
    {
      name: "Autoload last project",
      description: "Load your last project automatically on app launch instead of loading into the home menu",
      type: "boolean",
      onChange: (value: boolean) => {
        setValue("autoloadProject", value);
      },
      getValue: async (): Promise<boolean> => {
        return (await getValue("autoloadProject")) as boolean;
      },
    },
    {
      name: "Enable Audio Transcription",
      description: "Enable Whisper audio-to-text transcription. Runs 100% locally on your device",
      type: "boolean",
      onChange: (value: boolean) => {
        setValue("enableTranscription", value);
      },
      getValue: async (): Promise<boolean> => {
        return (await getValue("enableTranscription")) as boolean;
      },
    },
    {
      name: "Enabe Discord Rich Presence",
      description: "Displays general info on what you're doing to all your friends on Discord",
      type: "boolean",
      onChange: (value: boolean) => {
        setValue("enableRichPresence", value);
      },
      getValue: async (): Promise<boolean> => {
        return (await getValue("enableRichPresence")) as boolean;
      },
    },
    {
      name: "Track Statistics",
      description: "Tracks basic statistics about your app usage to display on home screen. All data stays on device",
      type: "boolean",
      onchange: (value: boolean) => {
        setValue("trackStats", value);
      },
      getValue: async (): Promise<boolean> => {
        return (await getValue("trackStats")) as boolean;
      }
    }
  ],
  Recording: [
    {
      name: "Record Start Delay",
      description: "Measured in milliseconds",
      type: "string",
      defaultValue: "10",
      onChange: (value: string) => {
        setValue("recordStartDelay", value);
      },
      getValue: async (): Promise<string> => {
        return (await getValue("recordStartDelay")) as string;
      },
    },
    {
      name: "Record End Delay",
      description: "Measured in milliseconds",
      type: "string",
      defaultValue: "50",
      onChange: (value: string) => {
        setValue("recordEndDelay", value);
      },
      getValue: async (): Promise<string> => {
        return (await getValue("recordEndDelay")) as string;
      },
    },
    {
      name: "Enable Noise Suppression",
      description: "Apply noise suppression on your recordings",
      type: "boolean",
      onchange: (value: boolean) => {
        setValue("allowNoiseSuppression", value);
      },
      getValue: async (): Promise<boolean> => {
        return (await getValue("allowNoiseSuppression")) as boolean;
      },
    },
    {
      name: "Auto-Accept Recordings",
      description: "You are committed!",
      type: "boolean",
      onChange: (value: boolean) => {
        setValue("autoAcceptRecordings", value);
      },
      getValue: async (): Promise<boolean> => {
        return (await getValue("autoAcceptRecordings")) as boolean;
      },
    },
  ]
} as const;
