<script lang="ts">
  let { children } = $props();
  import { getCurrentWindow } from "@tauri-apps/api/window";
  import "../app.css";
  import "beercss";
  import "material-dynamic-colors";

  const appWindow = getCurrentWindow();
</script>

<div>
  <nav class="left small-space small-elevate">
    <a href="/">
      <i>home</i>
      Home
    </a>
    <a href="/recordPage">
      <i>mic</i>
      Record
    </a>
    <a href="/dataEditor">
      <i>edit</i>
      Editor
    </a>
    <a href="/settingsPage">
      <i>settings</i>
      Settings
    </a>
  </nav>
  <main class="max p-10 w-screen h-screen background" style="user-select: none;">
    {@render children?.()}
  </main>
</div>

<style>
  * {
    user-select: none;
  }
</style>
