<div class="min-w-full">
    <div class="h-2.5 min-w-full dark:bg-tertiary-d bg-tertiary overflow-hidden rounded-xl">
        <div class="progress min-w-full h-full bg-accent left-right rounded-xl"></div>
    </div>
</div>

<style>
    .progress {
        animation: progress 1s infinite linear;
    }

    .left-right {
        transform-origin: 0 50%;
    }

    @keyframes progress {
        0% {
            transform: translateX(0) scaleX(0);
        }
        40% {
            transform: translateX(0) scaleX(0.4);
        }
        100% {
            transform: translateX(100%) scaleX(0.5);
        }
    }
</style>
