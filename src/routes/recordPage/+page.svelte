<script lang="ts">
  import { onMount } from "svelte";
  import { setDetails } from "../../Util/DiscordRpc";
  import { stat } from "@tauri-apps/plugin-fs";

  // I LOVE RUNES
  let idleState = $state(true);
  let recordingState = $state(false);
  let reviewingState = $state(false);

  onMount(async () => {
    await setDetails("Recording");
  });
</script>

<h1>Record Page</h1>
