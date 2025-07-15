<script lang="ts">
  import { onMount } from "svelte";
  import { setDetails } from "../../app/DiscordRpc";
  onMount(async() => {
      await setDetails("Settings")
  })
</script>

<div class="flex flex-grow flex-col items-center">
  <fieldset class="fieldset bg-base-200 border-base-300 rounded-box w-3/4 border p-4 gap-5">
    <legend class="fieldset-legend">General Options</legend>
    <label class="label">
      <input type="checkbox" checked class="toggle">
      Check For Updates
    </label>

    <label class="label">
      <input type="checkbox" checked class="toggle">
      <p>Autoload previous project</p>
    </label>
    <label class="label">
      <input type="checkbox" checked class="toggle">
      <p>Enable Audio Transcription</p>
    </label>
  </fieldset>
  <fieldset class="fieldset bg-base-200 border-base-300 rounded-box w-3/4 border p-4 gap-5">
    <legend class="fieldset-legend">Recording Options</legend>
    <label class="label">
      <input type="text" class="p-2 bg-neutral-800 rounded-md" placeholder="35">
      Record Start Delay (ms)
    </label>
    <label class="label">
      <input type="text" class="p-2 bg-neutral-800 rounded-md" placeholder="35">
      Record End Delay (ms)
    </label>
    <label class="label">
      <input type="checkbox" class="toggle">
      Auto-accept recordings
    </label>
  </fieldset>
  <fieldset class="fieldset bg-base-200 border-base-300 rounded-box w-3/4 border p-4 gap-5">
    <legend class="fieldset-legend">Custom Data</legend>
    <label class="label">
      <p><b>Import Custom Data</b></p>
      <button>Temp</button>
    </label>
    <label class="label">
      <p><b>Open In External Editor</b></p>
      <button>Temp</button>
    </label>
  </fieldset>
  <fieldset class="fieldset bg-base-200 border-error rounded-box w-3/4 border p-4 gap-5">
    <legend class="fieldset-legend text-error">Danger Zone</legend>
    <label class="label">
      <p><b>Reset Stats</b></p>
      <button class="btn btn-error">Delete</button>
    </label>
    <label class="label">
      <p><b>Delete Pitch Data</b></p>
      <button class="btn btn-error">Delete</button>
    </label>
    <label class="label">
      <p><b>Delete Effect Data</b></p>
      <button class="btn btn-error">Delete</button>
    </label>
    <label class="label">
      <p><b>Delete Custom Data</b></p>
      <button class="btn btn-error">Delete</button>
    </label>
    <label class="label">
      <p><b>Delete ALL DATA</b></p>
      <button class="btn btn-error">Delete</button>
    </label>
  </fieldset>
</div>
