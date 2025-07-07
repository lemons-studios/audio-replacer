<script lang="ts">
  import { House, Mic, PencilLine, Settings, Megaphone } from "@lucide/svelte";
  import { onMount } from "svelte";
  import "../app.css";
  import { setTheme } from "@tauri-apps/api/app";
  import { loadStore } from "../util/SettingsManager";
  import { goto } from "$app/navigation";
  let { children } = $props();

  onMount(async() => {
    await loadStore();
    await setTheme('dark');
    document.documentElement.classList.toggle(  
      "dark",  localStorage.theme === "dark" ||    
      (!("theme" in localStorage) && window.matchMedia("(prefers-color-scheme: dark)").matches),);

      localStorage.theme = "dark";
  })

  function navigate(toPage: string) {
    goto(toPage);
  }
</script>

<style>
  * {
  user-select: none;
  -webkit-user-select: none;
  -ms-user-select: none;
  }
</style>

<main class="dark:bg-zinc-950 bg-zinc-100 flex flex-row grow-1 dark:text-white items-stretch w-screen h-screen">
  <div class="flex flex-col items-stretch justify-between dark:bg-zinc-900 min-w-[10rem] p-1">
    <div class="flex flex-col items-stretch">
      <button class="menu-button"><House/>Home</button>
      <button class="menu-button"><Mic/>Record</button>
      <button class="menu-button"><PencilLine/>Editor</button>
    </div>
    <div class="flex flex-col items-stretch">
      <button class="menu-button"><Settings/>Settings</button>
      <button class="menu-button"><Megaphone/>Changes</button>
      <h3 class="text-xs text-center text-gray-300">Audio Replacer v5.0</h3>
    </div>
  </div>
    <div class="w-screen h-screen p-5.5">
      {@render children?.()}
    </div>
</main>
