<script lang="ts">
  import { House, Mic, PencilLine, Settings, Megaphone } from "@lucide/svelte";
  import { onMount } from "svelte";
  import "../app.css";
  import { loadStore } from "../util/SettingsManager";
  import { goto } from "$app/navigation";
  let { children } = $props();

  onMount(async() => {
    await loadStore();
  })
</script>

<style>
  * {
  user-select: none;
  -webkit-user-select: none;
  -ms-user-select: none;
  }
</style>

<main class="bg-base-300 flex flex-row grow-1 dark:text-white items-stretch w-screen h-screen">
  <div class="flex flex-col items-stretch justify-between bg-base-200 min-w-[10rem] p-1">
    <div class="flex flex-col items-stretch">
      <ul class="menu menu-vertical rounded-box w-full bg-transparent">
        <li><a href="/"><House/>Home</a></li>
        <li><a href="/recordPage"><Mic/>Record</a></li>
        <li><a href="/dataEditor"><PencilLine/>Editor</a></li>
      </ul>
    </div>
    <div class="flex flex-col items-stretch">
      <ul class="menu menu-vertical bg-transparent rounded-box w-full">
        <li><a href="/settingsPage"><Settings/>Settings</a></li>
        <li class="mb-1.5"><a href="/releaseNotes"><Megaphone/>Changes</a></li>
        <h3 class="text-xs text-center text-gray-300">Audio Replacer v5.0</h3>
      </ul>
    </div>
  </div>
    <div class="w-screen h-screen p-5.5">
      {@render children?.()}
    </div>
</main>
