<script lang="ts">
  import { onMount } from "svelte";
  import { settings } from "./SettingsContent";
  import { setPresenceDetails } from "../../tools/DiscordPresenceManager";
  import ToggleSwitch from "../../Components/ToggleSwitch.svelte";

  onMount(async() => {
    await setPresenceDetails("Tweaking Settings");
  });
</script>

<div class="flex grow justify-center flex-col gap-y-2.5 items-center overflow-y-auto">
  {#each Object.entries(settings) as [name, settingCategory]}
    <div class="card w-3/4 p-2">
      <h2 class="text-center text-3xl mb-2.5">{name}</h2>
      <hr class="border-accent-secondary mb-1">
      {#each settingCategory as setting}
        <div class="flex justify-between items-center px-4 mb-3 mt-3">
          <div class="w-full">
            <p class="font-medium">{setting.name}</p>
            <p class="text-sm text-gray-400 text-wrap max-w-2/3">{setting.description}</p>
          </div>
          {#if setting.type === "boolean"}
            {#await setting.getValue() then value}
              <ToggleSwitch onClick={async() => {await setting.onChange(value)}} enabled={value}></ToggleSwitch>
            {/await}
          {:else if setting.type === "string"}
            {#await setting.getValue() then value}
              <input type="text" value={value} class="max-w-1/12 bg-tertiary dark:bg-tertiary-d py-1.5 px-2 rounded-sm"
                     onchange={async(e) => await setting.onChange(e.currentTarget.value)}>
            {/await}
          {:else if setting.type === "button"}
            <button class="app-btn" onclick={setting.onClick}>{setting.buttonText}</button>
          {:else if setting.type === "dropdown"}
            <div class="dropdown">
              {#await setting.getValue() then value}
                <select class="w-45" value={value} onchange={async(e) => {await setting.onChange(e.currentTarget.value)}}>
                  {#each setting.choices as choice, i}
                    <option value={setting.choiceValues[i]}>{choice}</option>
                  {/each}
                </select>
              {/await}
            </div>
          {/if}
        </div>
      {/each}
    </div>
  {/each}
</div>
