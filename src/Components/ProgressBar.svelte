<script lang="ts">
    let { intermediate = false, progress = 10 } = $props();
    let formattedProgress = $derived(() => {
        return progress > 100 ? 100 : progress < 0 ? 0 : progress;
    })

    function getWidth() {
        return `w-${progress}/100`
    }
</script>

<div class="w-full dark:bg-tertiary-d bg-tertiary">
    <div class={`${getWidth()} bg-accent h-full`}>

    </div>
</div>