<script lang="ts">
    let { completion }: { completion: number } = $props();
</script>

<div class="min-w-full">
    <div class="h-2.5 min-w-full dark:bg-tertiary-d bg-tertiary overflow-hidden rounded-xl">
        <div class="h-full bg-accent rounded-xl" style={`width: ${completion}%`}></div>
    </div>
</div>
