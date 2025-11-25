<script lang="ts">
  import { onMount } from "svelte";
  import { setPresenceDetails } from "../tools/DiscordPresenceManager";
  import { invoke } from "@tauri-apps/api/core";
  import {loadStats} from "../tools/StatisticManager";

  let username = $state("");
  let stats = $state(null);

  onMount(async() => {
    await setPresenceDetails("");
    username = await invoke("get_username");
    await loadStats();
  });
</script>

<div class="flex flex-col gap-3 h-full w-full p-3">
  <h1 class="text-center text-3xl">Welcome, {username}</h1>
  <div class="flex flex-row gap-5 h-full">
    <div class="flex flex-col w-1/2 card rounded-xl p-3">
      <h1 class="text-center text-3xl font-medium">Load Previous Project</h1>
    </div>
    <div class="flex flex-col gap-y-5 w-1/2">
      <div class="card h-1/2 rounded-xl p-3">
        <h1 class="text-center text-3xl font-medium">Wiki</h1>
      </div>
      <div class="card h-1/2 p-3 rounded-xl">
        <h1 class="text-center text-3xl font-medium">Stats</h1>
      </div>
    </div>
  </div>
</div>