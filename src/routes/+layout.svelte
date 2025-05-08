<script>
    let { children } = $props();
    import { Icon } from "svelte-awesome";
    import bars from 'svelte-awesome/icons/bars';
    import house from 'svelte-awesome/icons/home';
    import microphone from "svelte-awesome/icons/microphone";
    import pencil from "svelte-awesome/icons/pencil";
    import cog from "svelte-awesome/icons/cog";
    import "../app.css"

    function toggleMenu()
    {
        console.log("Clicked")
    }
</script>

<div class="fixed top-0 left-0 right-0 h-15 bg-black/80 text-white flex items-center px-4 z-50">
    <button onclick={toggleMenu}><Icon data={bars} class="mr-7 h-5 w-5"/></button>
    <nav class="items-center flex justify-center">
        <a href="/" class="text-center text-xl mr-5"><Icon data={house} class="mr-2 h-5 w-5"/>Home</a>
        <a href="/record" class="text-center text-xl mr-5"><Icon data={microphone} class="mr-2 h-5 w-5"/>Record</a>
        <a href="/dataEditor" class="text-center text-xl mr-5"><Icon data={pencil} class="mr-2 h-5 w-5"/>Data Editor</a>
        <a href="/settings" class="text-center text-xl mr-5"><Icon data={cog} class="mr-2 h-5 w-5"/>Options</a>
    </nav>
</div>

{@render children()}

