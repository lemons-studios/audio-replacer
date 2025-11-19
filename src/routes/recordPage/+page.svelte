<script lang="ts">
  import { onDestroy, onMount } from "svelte";
  import { setPresenceDetails, setPresenceState } from "../../tools/DiscordPresenceManager";

  $effect(() => {

  })

  onMount(async() => {
    await setPresenceDetails("Recording Audio");
  });

  onDestroy(async() => {
    await setPresenceState("");
  });
</script>

<div class="flex flex-row justify-center gap-5">
  <div>

  </div>
  <div>

  </div>
</div>
