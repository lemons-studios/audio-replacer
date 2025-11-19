<script lang="ts">
  import { onMount } from "svelte";
  import { settings } from "./SettingsContent";
  import { setPresenceDetails } from "../../tools/DiscordPresenceManager";

  onMount(async() => {
    await setPresenceDetails("Tweaking Settings");
  });
</script>
<h1 class="text-center text-5xl font-bold mt-10">App Settings</h1>
<!--Ignore any errors below if any are visible, they only show up because not all elements in the javascript object share the same list of properties-->
<div class="flex grow justify-center flex-col gap-y-2.5 items-center overflow-y-auto p-5">
  {#each Object.entries(settings) as [name, settingCategory], sIndex}
    <div class="dark:bg-secondary-d bg-secondary rounded-xl p-3 w-3/4 gap-3">
      <h2 class="text-center">{name}</h2>
      {#each settingCategory as setting}
        <div class="flex justify-between items-center px-4 mb-3 mt-3">
          <div>
            <p class="font-medium">{setting.name}</p>
            <p class="text-sm text-gray-400 text-wrap">{setting.description}</p>
          </div>
          {#if setting.type == "boolean"}
            {#await setting.getValue() then value}
              <input type="checkbox" class="checkbox" checked={value} onchange={() => setting.onChange(value)}> 
            {/await}
          {:else if setting.type == "string"}
            {#await setting.getValue()}
              <input type="text" placeholder={setting.defaultValue} class="max-w-1/12">
            {:then value} 
              <input type="text" value={value} class="max-w-1/12 bg-tertiary dark:bg-tertiary-d py-1.5 px-2 rounded-sm" onchange={() => setting.onChange(value)}>
            {/await}
          {:else if setting.type == "button"}
            <button class="app-btn" onclick={setting.onClick}>{setting.buttonText}</button>
         {:else if setting.type == "dropdown"}
            <div class="dropdown">
              <li>
                {#each setting.choices as choice}
                  <ul>{choice}</ul>
                {/each}
              </li>
            </div>
          {/if}
        </div>
      {/each}
    </div>
  {/each}
</div>
