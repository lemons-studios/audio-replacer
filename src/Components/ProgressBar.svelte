<!--Adapted component from https://github.com/dave22r/RegionalOutbreakPredictor/blob/main/frontend/src/components/Loading.css and https://github.com/dave22r/RegionalOutbreakPredictor/blob/main/frontend/src/components/Loading.jsx-->
<script lang="ts">
    let { intermediate = false, progress = 10 } = $props();
    let formattedProgress = $derived(() => {
        return progress > 100 ? 100 : progress < 0 ? 0 : progress;
    })
</script>
