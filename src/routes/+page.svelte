<script>
    import { invoke } from "@tauri-apps/api/core";

    function getSystemTime() {
        const date = new Date();
        const hours = date.getHours();
        if(hours >= 5 && hours < 12) {
            return "Morning";
        }
        else if(hours >= 12 && hours < 18) {
            return "Afternoon";
        }
        else return "Evening"
    }

    async function getUsername() {
        const res = await invoke("get_username");
        return res;
    }

</script>

<p class="text-5xl text-center">Good {getSystemTime()}</p>

<div class="grid grid-cols-2 grid-rows-2 w-screen">
    <div class="col-span-1 col-start-1 row-span-1 row-start-1">
        <p>Load Project</p>
    </div>
    <div class="col-span-1 col-start-1 row-span-1 row-start-2">
        <p>Tips</p>
    </div>
    <div class="col-span-1 col-start-2 row-span-2 row-start-1">
        <p>Latest Changes</p>
    </div>
</div>

