<script lang="ts">
    let { children } = $props();
    import "beercss";
    import "material-dynamic-colors";
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
  <main class="max padding purple-border"> 
    {@render children?.()}
  </main>
</div>