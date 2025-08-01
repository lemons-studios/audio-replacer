<script lang="ts">
  import { onMount } from "svelte";
  import { setDetails } from "../../app/DiscordRpc";
  import { settings } from "./SettingsContent";
  onMount(async() => {
      await setDetails("Settings")
  });
</script>

<div class="flex flex-grow flex-col gap-y-2.5 items-center overflow-y-auto">
  {#each Object.entries(settings) as [name, settingCategory], sIndex}
    <fieldset class={`${sIndex == 2 ? 'pane-error' : 'pane'} w-3/4`}>
      <legend class="fieldset-legend">{name}</legend>
      {#each settingCategory as setting}
        <div class="flex justify-between items-center px-4">
          <div>
            <p class="font-semibold">{setting.name}</p>
            <p class="text-sm text-gray-400 text-wrap">{setting.description}</p>
          </div>
          {#if setting.type == "boolean"}
            {#await setting.getValue() then value}
              <input type="checkbox" class="toggle" checked={value} onchange={async() => await setting.onChange(value)}> 
            {/await}
          {:else if setting.type == "string"}
            {#await setting.getValue()}
              <input type="text" placeholder={setting.defaultValue} class="input max-w-1/12">
            {:then value} 
              <input type="text" value={value} class="input max-w-1/12" onchange={async() => setting.onChange(value)}>
            {/await}
          {:else if setting.type == "button"}
            <button class="btn btn-primary btn-md" onclick={setting.onClick}>{setting.buttonText}</button>
         {:else if setting.type == "dropdown"}
            <select class="select">
              {#each setting.choices as choice}
                <option>{choice}</option>
              {/each}
            </select>
          {/if}
        </div>
      {/each}
    </fieldset>
  {/each}
</div>
