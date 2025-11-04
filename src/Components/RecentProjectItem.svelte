<script lang="ts">
    import ArrowRightRegular from "phosphor-icons-svelte/IconArrowRightRegular.svelte";
    import { timestampToLegible } from "../tools/OsTools";
    import { setProjectData } from "../tools/ProjectManager";
    import { goto } from "$app/navigation";

    let { project, projectFilePath } = $props();

    async function loadProject() {
        await setProjectData(projectFilePath, project);
        goto('/recordPage');
    }

</script>

<button 
  class="save-btn rounded-sm w-full dark:hover:bg-tertiary-d hover:bg-tertiary dark:focus:bg-tertiary-d dark:focus:drop-shadow-xl transition duration-75" 
  onmouseleave={(e) => e.currentTarget.blur()} 
  onmouseup={(e) => e.currentTarget.blur()}>
    <div class="flex justify-between items-center text-left p-3 mb-1.5 mt-1.5">
      <div class="flex flex-col">
        <p class="text-lg">{project.name}</p>
        <p class="text-gray-400 text-sm">Last Opened: {timestampToLegible(project.lastOpened)}</p>
        <p class="text-gray-400 text-xs">Files Remaining: {Intl.NumberFormat().format(project.fileCount)}</p>
      </div>
      <ArrowRightRegular class="arrow w-5 h-5"></ArrowRightRegular>
    </div>
</button>
