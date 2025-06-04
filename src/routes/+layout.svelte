<script lang="ts">
  let { children } = $props();
  import { onMount } from "svelte";

  import "../app.css";
</script>

<div>
  <main class="max background ">
    {@render children?.()}
  </main>
</div>
