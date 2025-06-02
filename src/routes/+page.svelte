<script lang="ts">
  import { setTheme } from "@tauri-apps/api/app";
  import { initializeValues, extraApplicationData } from "../generic/AppProperties";
  import { onMount } from "svelte";

  let appDataTest: string = ""

  onMount(async() => {
    await initializeValues();

    appDataTest = extraApplicationData;
    await setTheme("dark");
  })
</script>
 
  <div class="grid">
    <div class="s12 m12 l6">
      <h3>aa</h3>
    </div>
    <div class="s12 m12 l6">
      <h3>{appDataTest}</h3>
    </div>
    <div class="s12 m12 l6">
      <h3>Pane 3</h3>
    </div>
    <div class="s12 m12 l6">
      <h3>Pane 4</h3>
    </div>
  </div>
