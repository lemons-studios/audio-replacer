<script lang="ts">
  let { children } = $props();
  import "../app.css";
</script>

<main>
  <div class="flex text-white bg-bg">
    <div class="w-40 h-screen content-top flex-none drop-shadow-lg bg-menubar p-5 rounded-tr-4xl rounded-br-4xl">
      <div class="flex mb-5">
        <a href="/" class="flex-auto"><p class="icon mr-2.5 text-2xl">home</p><p class="text-lg">Home</p></a>
      </div>
      <div class="flex mb-5">
        <a href="/recordPage" class="flex-auto"><p class="icon mr-2.5 text-2xl">mic</p><p class="text-lg">Record</p></a>
      </div>
      <div class="flex mb-5">
        <a href="/dataEditor" class="flex-auto"><p class="icon text-2xl mr-2.5">edit</p><p class="text-lg">Data Editor</p></a>
      </div>
      <div class="flex mb-5">
        <a href="/settingsPage" class="flex-auto"><p class="icon mr-2.5 text-2xl">settings</p><p class="text-lg">Settings</p></a>
      </div>
    </div>
    <div class="w-50 h-screen flex-auto bg-bg p-5">
      {@render children?.()}
    </div>
  </div>
</main>
