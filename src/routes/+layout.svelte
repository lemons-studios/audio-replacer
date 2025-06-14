<script lang="ts">
  import { onMount } from "svelte";
  import "../app.css";
  import { setTheme } from "@tauri-apps/api/app";
  import '@material/web/all.js';
  import { loadStore } from "../util/SettingsManager";
  let { children } = $props();

  onMount(async() => {
    await loadStore();
    await setTheme('dark');
    document.documentElement.classList.toggle(  
      "dark",  localStorage.theme === "dark" ||    
      (!("theme" in localStorage) && window.matchMedia("(prefers-color-scheme: dark)").matches),);

      localStorage.theme = "dark";
  })
</script>

<style>
  * {
  user-select: none;
  -webkit-user-select: none;
  -ms-user-select: none;
}

</style>

<main>
  <div class="flex dark:text-white bg-bg-light dark:bg-background-dark">
    <div class="w-45 h-screen content-top flex-none drop-shadow-lg bg-menubar-light dark:bg-surface-container-low-dark p-5 rounded-tr-4xl rounded-br-4xl">
      <div class="flex mb-7.5">
        <button><a href="/" class="flex items-center"><p class="font-icons mr-2.5 text-2xl">home</p><p class="text-lg font-lexend">Home</p></a></button>
      </div>
      <div class="flex mb-7.5">
        <button><a href="/recordPage" class="flex items-center"><p class="font-icons mr-2.5 text-2xl">mic</p><p class="text-lg font-lexend">Record</p></a></button>
      </div>
      <div class="flex mb-7.5">
        <a href="/dataEditor" class="flex items-center"><p class="font-icons text-2xl mr-2.5">edit</p><p class="text-lg font-lexend">Data Editor</p></a>
      </div>
      <div class="flex mb-7.5">
        <a href="/settingsPage" class="flex items-center"><p class="font-icons mr-2.5 text-2xl">settings</p><p class="text-lg font-lexend">Settings</p></a>
      </div>
    </div>
    <div class="w-screen h-screen flex-auto p-5.5">
      {@render children?.()}
    </div>
  </div>
</main>
