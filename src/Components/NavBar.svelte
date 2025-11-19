<script lang="ts">
    import IconHouseRegular from 'phosphor-icons-svelte/IconHouseRegular.svelte';
    import IconMicrophoneRegular from 'phosphor-icons-svelte/IconMicrophoneRegular.svelte';
    import IconMegaphoneRegular from 'phosphor-icons-svelte/IconMegaphoneRegular.svelte';
    import IconGearSixRegular from 'phosphor-icons-svelte/IconGearSixRegular.svelte';
    import IconPencilRegular from 'phosphor-icons-svelte/IconPencilRegular.svelte';
    import { onMount, tick } from 'svelte';
    import { getVersion } from "@tauri-apps/api/app";
    import { goto } from '$app/navigation';

    let buildText = $state('Audio Replacer 5');

    const navbarContents = {
        top: [
            {
                name: 'Home',
                icon: IconHouseRegular,
                route: '/'
            }, 
            {
                name: 'Record',
                icon: IconMicrophoneRegular,
                route: '/recordPage'
            },
            {
                name: 'Effect Editor',
                icon: IconPencilRegular,
                route: '/effectEditor',
            }
        ],
        bottom: [
            {
                name: 'Release Notes',
                icon: IconMegaphoneRegular,
                route: '/releaseNotes'
            },
            {
                name: 'Settings',
                icon: IconGearSixRegular,
                route: '/settingsPage'
            }
        ]
    } as const;

    onMount(async() => {
        await tick();
        buildText = `Audio Replacer ${getBuildNumber()}`;
    });

    async function getBuildNumber(): Promise<string> {
        const [mj, mn, p] = (await getVersion()).split(".");
        return `${mj}.${mn}${p == "0" ? '' : `.${p}`}`;
    }
</script>

<style>
    .menu-container {
        display: flex;
        flex-direction: column;
        align-items: center;
        text-align: center;
        vertical-align: center;
        padding: 2.5px;
    }
</style>

<!--TODO: Implement icon rendering (probably by just downloading svgs i need and ditching the full icon lib installs that are taking up unneccesary space)-->
<div class="flex flex-col items-stretch justify-between dark:bg-secondary-d bg-secondary min-w-40 px-1 py-2 rounded-r-lg drop-shadow-xl ">
    <!--Top Menu Items-->
    <div class="menu-container gap-0.5">
        {#each navbarContents.top as item}
            <button class="nav-btn transition" onmouseleave={(e) => e.currentTarget.blur()} onclick={async() => await goto(item.route)}><item.icon class="w-5 h-5" />{item.name}</button>
        {/each}
    </div>
    <!--Bottom Menu Items-->
    <div class="menu-container gap-0.5">
        {#each navbarContents.bottom as item}
            <button class="nav-btn transition" onmouseleave={(e) => e.currentTarget.blur()} onclick={async() => await goto(item.route)}><item.icon />{item.name}</button>
        {/each}
    </div>
</div>
