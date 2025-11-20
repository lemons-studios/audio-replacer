<script lang="ts">
    import { pitchFilters, pitchFilterNames, effectFilters, effectFilterNames } from "../recordPage/AudioManager";
    let pitchValues = $state();
    let pitchNames = $state();

    let effectValues = $state();
    let effectNames = $state();

    $effect(() => {
        pitchValues = pitchFilters;
        pitchNames = pitchFilterNames;
        
        effectValues = effectFilters;
        effectNames = effectFilterNames;
    })
</script>

<div class="dark:bg-secondary-d bg-secondary rounded-lg w-full h-full p-2">
    
</div>