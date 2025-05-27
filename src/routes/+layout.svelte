<script lang="ts">
    let { children } = $props();
    import "beercss";
    import "material-dynamic-colors";
</script>

<nav class="left small-space small-elevate ">
  <a href="/">
    <i>home</i>
    <div>Home</div>
  </a>
  <a href="/recordPage">
    <i>mic</i>
    <div>Record</div>
  </a>
  <a href="/dataEditor">
    <i>edit</i>
    <div>Editor</div>
  </a>
  <a href="/settingsPage">
    <i>settings</i>
    <div>Settings</div>
  </a>
</nav>

{@render children()}