<script lang="ts">
    import "../app.css";
    let isExpanded = $state(false);
</script>

<div class="gap-5 p-2.5">
    {#if isExpanded}
        <p class="font-icons mb-10 ">menu-open</p>

        <p class="font-icons mb-5">home</p> <p class="font-inter">Home</p>
    {/if}
    {#if !isExpanded}
        <p class="font-icons mb-10">menu</p>
    {/if}
</div>