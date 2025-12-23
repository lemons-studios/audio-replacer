<script lang="ts">
    import { House, Mic, PencilLine, Megaphone, Settings } from '@lucide/svelte';
    import { onMount, tick } from 'svelte';
    import { getVersion } from "@tauri-apps/api/app";
    import { goto } from '$app/navigation';
    import {error} from "@tauri-apps/plugin-log";

    let buildText = $state('Audio Replacer 5');

    const navbarContents = {
        top: [
            {
                name: 'Home',
                icon: House,
                route: '/'
            }, 
            {
                name: 'Record',
                icon: Mic,
                route: '/recordPage'
            },
            {
                name: 'Effect Editor',
                icon: PencilLine,
                route: '/effectEditor',
            }
        ],
        bottom: [
            {
                name: 'Release Notes',
                icon: Megaphone,
                route: '/releaseNotes'
            },
            {
                name: 'Settings',
                icon: Settings,
                route: '/settingsPage'
            }
        ]
    } as const;

    onMount(async() => {
        await tick();
        buildText = `Audio Replacer ${await getBuildNumber()}`;
    });

    async function getBuildNumber(): Promise<string> {
        const [mj, mn, p] = (await getVersion()).split(".");
        return `${mj}.${mn}${p == "0" ? '' : `.${p}`}`;
    }

    async function navigateToPage(route: string) {
        try {
            await goto(route);
        } catch(e: any) {
            await error(`error whilst loading ${route}: ${e}`);
        }
    }

</script>

<style>
    .menu-container {
        display: flex;
        flex-direction: column;
        align-items: center;
        text-align: center;
        vertical-align: center;
        padding: 3px;
    }
</style>

<div class="flex flex-col items-stretch justify-between dark:bg-secondary-d bg-secondary min-w-40 px-1 py-2 rounded-r-lg drop-shadow-xl ">
    <!--Top Menu Items-->
    <div class="menu-container gap-0.5">
        {#each navbarContents.top as item}
            <button class="nav-btn min-w-30 flex flex-row text-center items-center justify-stretch gap-2" onmouseleave={(e) => e.currentTarget.blur()} onclick={async() => await navigateToPage(item.route)}><item.icon class="w-5 h-5" />{item.name}</button>
        {/each}
    </div>
    <!--Bottom Menu Items-->
    <div class="menu-container gap-0.5">
        {#each navbarContents.bottom as item}
            <button class="nav-btn min-w-30 flex flex-row text-center items-center justify-start gap-2" onmouseleave={(e) => e.currentTarget.blur()} onclick={async() => await navigateToPage(item.route)}><item.icon class="w-5 h-5" />{item.name}</button>
        {/each}
        <p class="text-xs text-gray-400">{buildText}</p>
    </div>
</div>
