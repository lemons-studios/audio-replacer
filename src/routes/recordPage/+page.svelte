<script lang="ts">
  import * as ProjectManager from "../../util/ProjectManager"
  import AudioPlayer from "../../Components/AudioPlayer.svelte";

  let currentPathTrunc = $state("Select a folder to begin");
  let currentPathFull = $state("");

  let completionValue = $state(0.25);
  let completionPercentage = $state("0%");
  let filesRemaining = $state(0);

  $effect(() => {
    currentPathFull = ProjectManager.currentFile
    currentPathTrunc = ProjectManager.currentFileLocalPath;
    filesRemaining = ProjectManager.filesRemaining
  })
</script>

<div class="flex grow flex-row h-full gap-4 content-center">
  <div class="w-3/4 card">
    <h1 class="title-text mb-5"><b>{currentPathTrunc}</b></h1>
    <h2>Files Remaining: {filesRemaining} ({completionPercentage})</h2>
    <progress value={completionValue} class="text-2xl mb-15 rounded-3xl">AAA</progress>
    <AudioPlayer source={currentPathFull}/>
  </div>
  <div class="w-1/2 card">
    <h1 class="title-text mb-40"><b>Recording Filters</b></h1>
    <h2 class="title-text mb-10">Pitch Filters</h2>
    <select class="bg-black h-[2.5rem] rounded-lg p-2 mb-50">
      <option>Default</option>
      <option>Some other option</option>
    </select>
    <h2 class="title-text mb-10">Audio Effects</h2>
    <select class="bg-black h-[2.5rem] rounded-lg p-2 mb-25">
      <option>Default</option>
      <option>Some Other Option</option>
    </select>
    <div class="flex flex-row justify-center gap-2">
      <input type="checkbox">
      <h4 class="text-xl">Requires Extra Edits?</h4>
    </div> 
  </div>
</div>
