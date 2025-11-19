<script lang="ts">
    import { onMount } from "svelte";
    import { setPresenceDetails } from "../tools/DiscordPresenceManager";


  onMount(async() => {
    await setPresenceDetails("");
  })
</script>

<div class="flex flex-row">

</div>