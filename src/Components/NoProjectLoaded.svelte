<script>
    import { goto } from "$app/navigation";
    import { House } from "@lucide/svelte";
</script>

<div class="w-full h-full rounded-lg card flex flex-col gap-5 justify-center text-center items-center">
  <h2 class="text-3xl font-medium">No Project Loaded!</h2>
  <div class="flex flex-row justify-center gap-3">
    <button class="app-btn"
            onclick={async(e) => {e.currentTarget.blur(); await goto('/')}}
            onmouseleave={(e) => e.currentTarget.blur}><House class="button-icon"/> Return Home
    </button>
  </div>
</div>