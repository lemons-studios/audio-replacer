<script lang="ts">
    import { getValue, initializeData } from "../tools/DataInterface";
    import { invoke } from "@tauri-apps/api/core";
    import { onMount } from "svelte";
    import { onNavigate } from "$app/navigation";
    import { attemptRelaunch, formatVersion, getMic } from "../tools/OsTools";
    import { info, warn } from "@tauri-apps/plugin-log";
    import { createAdditionalData } from "../tools/ProjectHandler";
    import { setTheme } from "@tauri-apps/api/app";
    import { check, Update } from "@tauri-apps/plugin-updater";
    import { ask, message } from "@tauri-apps/plugin-dialog";
    import "../app.css";
    import NavBar from "../Components/NavBar.svelte";

    let { children } = $props();

    async function checkForUpdates() {
        try {
            const update: Update | null = await check();
            if (update) {
                const confirmation = await ask(
                    `Version Update Found\nCurrent: ${await formatVersion()}\nLatest: ${update.version}`,
                    {
                        title: "Update Found",
                        kind: "info",
                    },
                );
                if (confirmation) {
                    await update.downloadAndInstall();
                    const restart = await ask(
                        "Update downloaded. Restart now?",
                        {
                            title: "Update Complete",
                            kind: "info",
                        },
                    );

                    if (restart) await attemptRelaunch();
                    else
                        await message("App will update on close.", {
                            kind: "info",
                        });
                }
            }
        } catch (e: any) {
            await warn(
                "Checking for updates has failed! it might be due to a misconfigured update server",
            );
        }
    }

    onMount(async () => {
        await setTheme(await getValue("settings.theme"));
        await initializeData();

        // Populate additional variables
        await info("Getting/Asking for Microphone Permission");
        await getMic();

        await info("Creating Additional Directories");
        await createAdditionalData();

        // Prevent right click context menu from showing up (unneeded in production builds)
        const isDev = (await invoke("in_dev_env")) as boolean;
        if (!isDev) {
            document.addEventListener("contextmenu", (e) => {
                e.preventDefault();
            });
        }

        const allowUpdates = await getValue("settings.updateCheck");
        if (allowUpdates && !isDev) await checkForUpdates();
    });

    onNavigate((navigation) => {
        if (!document.startViewTransition) return;

        return new Promise((resolve) => {
            document.startViewTransition(async () => {
                resolve();
                await navigation.complete;
            });
        });
    });
</script>

<main
    class="dark:bg-primary-d bg-primary flex flex-row grow dark:text-white items-stretch w-screen h-screen overflow-y-hidden"
>
    <NavBar />
    <div class="flex-1 flex flex-col overflow-hidden w-screen h-screen p-5.5">
        {@render children?.()}
    </div>
</main>

<style>
    * {
        user-select: none;
        -webkit-user-select: none;
        -ms-user-select: none;
    }
</style>
