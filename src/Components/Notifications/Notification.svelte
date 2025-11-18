<script lang="ts">
  import IconInfoRegular from "phosphor-icons-svelte/IconInfoRegular.svelte";
  import IconCheckCricleRegular from "phosphor-icons-svelte/IconCheckCircleRegular.svelte";
  import IconWarningRegular from "phosphor-icons-svelte/IconWarningRegular.svelte";
  import IconWarningOctagonRegular from "phosphor-icons-svelte/IconWarningOctagonRegular.svelte";
  import IconHourglassMediumRegular from "phosphor-icons-svelte/IconHourglassMediumRegular.svelte";
  import IconXNormal from "phosphor-icons-svelte/IconXRegular.svelte";
  import { fade, fly } from "svelte/transition";

  let queue: any[] = $state([]); // Too lazy to figure out the type
  let notificationId: number = $state(0);

  export function addToNotification(
    type: number = 0,
    title: string = "Title",
    message: string = "",
    closable: boolean = true,
    timeout: number = 5000
  ) {
    const iconType = (() => {
      switch (type) {
        case 0:
          return IconInfoRegular;
        case 1:
          return IconCheckCricleRegular;
        case 2:
          return IconWarningRegular;
        case 3:
          return IconWarningOctagonRegular;
        case 4:
          return IconHourglassMediumRegular;
      }
    })();

    queue.push({
      type: type,
      iconType: iconType,
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
        break;
      }
    }
    return 0; // fallback
  }

  function closeNotification(id: number) {
    queue.splice(getIdIndex(id), 1);
  }

  function getColourClasses(id: number): string {
    const index = getIdIndex(id);
    switch (queue[index].type) {
      case 0: // Info
        return "bg-info drop-shadow-info-shadow";
      case 1: // Success
        return "bg-success drop-shadow-success-shadow";
      case 2: // Warninr
        return "bg-warning drop-shadow-warning-shadow";
      case 3: // Error
        return "bg-error drop-shadow-error-shadow";
      case 4:
        return "bg-progress drop-shadow-progress-shadow";
      default:
        return "bg-info drop-shadow-info-shadow";
    }
  }
</script>

<div class="h-auto w-120 flex flex-col justify-center gap-y-2.5" out:fly={{duration: 300}}>
  {#each queue as n, index (n)}
    <div class={`notification flex flex-row justify-apart h-auto min-w-120 gap-x-1.5 p-2.5 rounded-lg text-white drop-shadow-xl ${getColourClasses(n.id)}`} in:fade={{duration: 175}} out:fade={{duration: 175}}>
      {#if n.closable}
        <div class="close-btn items-center">
          <button
            class="close-btn w-8 h-8 rounded-lg"
            onmouseleave={(e) => e.currentTarget.blur()}
            onclick={() => closeNotification(n.id)}
            ><IconXNormal class="h-8 w-8 p-1 text-center" /></button
          >
        </div>
      {/if}

      <!--Left Side-->
      <div class="flex flex-row gap-x-0.5 items-center mr-4">
        <n.iconType class="mr-1.5 h-8 w-8" />
        <h2>{n.title}</h2>
      </div>

      <!--Right Side-->
      <div class="items-center flex">
        <p class="text-sm text-center">{n.message}</p>
      </div>
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
    box-shadow: inset 0px 0px 1em ooklch(0.1929 0.0048 325.72 / 60%);
  }
</style>
