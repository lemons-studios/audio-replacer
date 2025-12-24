<script lang="ts">
    let { onClick, enabled = false } = $props();
</script>

<button aria-label="toggle-switch" class="relative inline-block w-11 h-5" onclick={() => {enabled = !enabled; onClick()}}>
    <input checked={enabled} type="checkbox" class="peer appearance-none w-11 h-5 dark:bg-tertiary-d bg-tertiary rounded-full checked:bg-accent-secondary dark:checked:bg-accent-tertiary cursor-pointer transition-colors duration-300">
    <label for="switch-component" class="absolute top-0 left-0 w-5 h-5 bg-accent rounded-full shadow-sm transition-transform duration-300 peer-checked:translate-x-6 peer-checked:border-slate-800 cursor-pointer">
    </label>
</button>
