<script lang="ts">
  import { Info, Check, TriangleAlert, CircleX } from "@lucide/svelte";
  import { debounce } from "../tools/OsTools";
  let { notificationType, title, message } = $props();
  let isVisible = $state(true);

  let alertType = $derived(() => {
    if (!notificationType || notificationType == "info")
      return "alert-info";
    if (notificationType == "success")
      return "alert-success";
    if (notificationType == "warning")
      return "alert-warning";
    if (notificationType == "error") return " alert-error";
  });

  async function setVisible() {
    isVisible = true;
    const waitTime = 1750; // Milliseconds
    debounce(() => (isVisible = false), waitTime);
  }
</script>

{#if isVisible}
  <div role="alert" class={`alert alert-outline ${alertType} notification-overlay`}>
    {#if notificationType == "info"}
      <Info />
    {:else if notificationType == "success"}
      <Check />
    {:else if notificationType == "warning"}
      <TriangleAlert />
    {:else if notificationType == "error"}
      <CircleX />
    {/if}
    <h3 class="font-bold">{title}</h3>
    <div class="text-xs">{message}</div>
  </div>
{/if}
