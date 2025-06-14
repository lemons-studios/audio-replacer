<script lang="ts">
  import { onMount } from "svelte";
  import { setDetails } from "../../util/DiscordRpc";
  
  // I LOVE RUNES
  let idleState = $state(true);
  let recordingState = $state(false);
  let reviewingState = $state(false);

  onMount(async () => {
    await setDetails("Recording");
  });

  function switchStates() {
    if(idleState) {
      idleState = false;
      recordingState = true;
    }
    else if (recordingState) {
      recordingState = false;
      reviewingState = true;
    }
    else if (reviewingState) {
      reviewingState = false;
      idleState = true;
    }
  }

  function cancelRecording() {
    recordingState = false;
    idleState = true;
  }
</script>


<div class="grid grid-cols-2 gap-5">
  <div>

  </div>
  <div>

  </div>
</div>