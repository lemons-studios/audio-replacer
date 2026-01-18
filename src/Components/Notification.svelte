<script lang="ts">
    import {
        Info,
        Check,
        TriangleAlert,
        OctagonAlert,
        X,
        Hourglass,
    } from "@lucide/svelte";
    import { fade, fly } from "svelte/transition";
    import IntermediateProgressBar from "./IntermediateProgressBar.svelte";

    let queue: any[] = $state([]);
    let notificationId: number = $state(0);

    type NotificationType =
        | "info"
        | "success"
        | "warning"
        | "error"
        | "progress";

    export function addToNotification(
        type: NotificationType = "info",
        title: string = "Title",
        message: string = "",
        closable: boolean = true,
        timeout: number = 5000,
    ) {
        const iconType = () => {
            switch (type) {
                case "info":
                    return Info;
                case "success":
                    return Check;
                case "warning":
                    return TriangleAlert;
                case "error":
                    return OctagonAlert;
                case "progress":
                    return Hourglass;
            }
        };

        queue.push({
            type: type,
            iconType: iconType(),
            title: title,
            message: message,
            closable: closable,
            id: notificationId,
        });

        // My idea here is that every notification box has its own specific "id", allowing the program to easily identify specific notifications when they are to be manually closed
        // Upper limit is over 2 billion so it's not that much of a worry icl
        // increment after pushing to notification queue because I want ids to start at zero
        notificationId++;

        setTimeout(() => {
            queue.shift();
        }, timeout);
    }

    function getIdIndex(id: number): number {
        console.log(id); // remove after testing
        for (let i = 0; i < queue.length; i++) {
            if (queue[i].id === id) {
                return i;
            }
        }
        return 0; // fallback
    }

    function closeNotification(id: number) {
        queue.splice(getIdIndex(id), 1);
    }

    function getColourClasses(id: number): string {
        const index = getIdIndex(id);
        const type: NotificationType = queue[index].type;
        switch (type) {
            case "info": // Info
                return "bg-info drop-shadow-info-shadow";
            case "success": // Success
                return "bg-success drop-shadow-success-shadow";
            case "warning": // Warning
                return "bg-warning drop-shadow-warning-shadow";
            case "error": // Error
                return "bg-error drop-shadow-error-shadow";
            case "progress":
                return "bg-progress drop-shadow-progress-shadow";
        }
    }
</script>

<div
    class="h-auto w-120 flex flex-col justify-center gap-y-2.5"
    out:fly={{ duration: 300 }}
>
    {#each queue as n}
        <div
            class={`notification h-auto min-w-120 p-2.5 rounded-lg text-white drop-shadow-xl flex flex-col ${getColourClasses(n.id)}`}
            in:fade={{ duration: 175 }}
            out:fade={{ duration: 175 }}
        >
            <div class="flex flex-row justify-apart items-center gap-x-1.5">
                {#if n.closable}
                    <div class="close-btn items-center">
                        <button
                            class="close-btn w-8 h-8 rounded-lg flex justify-center items-center"
                            onmouseleave={(e) => e.currentTarget.blur()}
                            onclick={() => closeNotification(n.id)}
                        >
                            <X class="w-5 h-5 items-center" />
                        </button>
                    </div>
                {/if}

                <!--Left Side-->
                <div class="flex flex-row gap-x-2.5 items-center mr-4">
                    <n.iconType class="button-icon" />
                    <h2>{n.title}</h2>
                </div>

                <!--Right Side-->
                <div class="items-center flex">
                    <p class="text-sm text-center">{n.message}</p>
                </div>
            </div>
            {#if n.type === "progress"}
                <div class="flex justify-center items-center">
                    <div class="w-3/4 mt-2">
                        <IntermediateProgressBar />
                    </div>
                </div>
            {/if}
        </div>
    {/each}
</div>

<style>
    .close-btn {
        position: absolute;
        align-self: center;
        right: 0.25em;
        top: 50%;
        transform: translateY(-50%);
        display: flex;
        justify-content: center;
    }
    .close-btn:hover {
        background-color: oklch(1 0 0 / 30%);
        box-shadow: none;
    }

    .close-btn:focus {
        background-color: oklch(1 0 0 / 10%);
        box-shadow: inset 0 0 1em oklch(0.1929 0.0048 325.72 / 60%);
    }
</style>
