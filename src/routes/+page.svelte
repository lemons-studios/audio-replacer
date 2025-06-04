<script lang="ts">
  import { initializeValues, extraApplicationData } from "../generic/AppProperties";
  import { onMount } from "svelte";

  let appDataTest: string = ""

  onMount(async() => {
    await initializeValues();

    appDataTest = extraApplicationData;
  })
</script>
