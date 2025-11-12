<!--Adapted component from https://github.com/dave22r/RegionalOutbreakPredictor/blob/main/frontend/src/components/Loading.css and https://github.com/dave22r/RegionalOutbreakPredictor/blob/main/frontend/src/components/Loading.jsx-->
<script lang="ts">
    let { intermediate = false, progress = 10 } = $props();
    let formattedProgress = $derived(() => {
        return typeof progress === 'number' ? progress > 100 ? 100 : progress < 0 ? 0 : progress : null; 
    })
</script>


<div class="linear-loader">
    {#if !intermediate && formattedProgress !== null}
        <div class="inner"></div>
    {:else}
        <div class="idtm-1">
            <div class="inner"></div>
        </div>
        <div class="idtm-2">
            <div class="inner"></div>
        </div>
    {/if}
</div>

<style>
    @reference "tailwindcss";

    .linear-loader {
        @apply relative dark:bg-tertiary-d bg-tertiary rounded-full w-full h-2 overflow-hidden;

        & .inner {
            @apply top-0 left-0 absolute bg-accent rounded-full h-full;
        }

        & > :is(.idtm-1, .idtm-2) {
            @apply top-0 left-0 absolute w-full h-full;

            & > .inner {
                @apply w-full;
            }
        }
        & > .idtm-1 {
            inset-inline-start: -145%;
            animation: ll-idtm-1-t 2s infinite linear;

            & > .inner {
                animation: ll-idtm-1-s 2s infinite linear;
            }
        }
        & > .idtm-2 {
            inset-inline-start: -55%;
            animation: ll-idtm-2-t 2s infinite linear;

            & > .inner {
                animation: ll-idtm-2-s 2s infinite linear;
            }
        }
    }

    @keyframes ll-idtm-1-t {
        0% {
            transform: translateX(0px);
        }

        20% {
            animation-timing-function: cubic-bezier(0.5, 0, 0.7, 0.5);
            transform: translateX(0px);
        }

        59.15% {
            animation-timing-function: cubic-bezier(0.3, 0.38, 0.55, 0.96);
            transform: translateX(84%);
        }

        100% {
            transform: translateX(200%);
        }
    }

    @keyframes ll-idtm-2-t {
        0% {
            animation-timing-function: cubic-bezier(0.15, 0, 0.52, 0.41);
            transform: translateX(0px);
        }

        25% {
            animation-timing-function: cubic-bezier(0.31, 0.28, 0.8, 0.73);
            transform: translateX(38%);
        }

        48.35% {
            animation-timing-function: cubic-bezier(0.4, 0.63, 0.6, 0.9);
            transform: translateX(84%);
        }

        100% {
            transform: translateX(160%);
        }
    }

    @keyframes ll-idtm-1-s {
        0% {
            transform: scaleX(0.08);
        }

        36.65% {
            animation-timing-function: cubic-bezier(0.33, 0.12, 0.79, 1);
            transform: scaleX(0.08);
        }

        69.15% {
            animation-timing-function: cubic-bezier(0.06, 0.11, 0.6, 1);
            transform: scaleX(0.661479);
        }

        100% {
            transform: scaleX(0.08);
        }
    }

    @keyframes ll-idtm-2-s {
        0% {
            animation-timing-function: cubic-bezier(0.2, 0.06, 0.5766, 0.45);
            transform: scaleX(0.08);
        }

        19.15% {
            animation-timing-function: cubic-bezier(0.1523, 0.1964, 0.6484, 1);
            transform: scaleX(0.457104);
        }

        44.15% {
            animation-timing-function: cubic-bezier(
                0.257759,
                -0.003163,
                0.211762,
                1.38179
            );
            transform: scaleX(0.72796);
        }

        100% {
            transform: scaleX(0.08);
        }
    }
</style>
