<script lang="ts">
	// Code yoinked from https://www.codelantis.com/blog/sveltekit-monaco-editor. same article as Monaco.ts
	import { onDestroy, onMount } from 'svelte';
	import { editorTheme } from '../app/CodeEditorTheme';
	import type * as Monaco from 'monaco-editor/esm/vs/editor/editor.api.js'; // If an error pops up here, ignore it. the file most certainly exists

	let editor: Monaco.editor.IStandaloneCodeEditor;
	let monaco: typeof Monaco;
	let editorContainer: HTMLElement;

	let content = $props();
	let trueContent = $derived(content);

	onMount(async () => {
		// Import our 'monaco.ts' file here
		// (onMount() will only be executed in the browser, which is what we want)
		monaco = (await import("../tools/Monaco")).default;

		// Your monaco instance is ready, let's display some code!
		monaco.editor.defineTheme('catppuccin', editorTheme);
		const editor = monaco.editor.create(editorContainer, {
			theme: 'catppuccin',
			automaticLayout: true,
			language: 'json',
			minimap: { enabled: false },
			hideCursorInOverviewRuler: true
		});
		const model = monaco.editor.createModel(
			trueContent,
			'json'
		);
		editor.setModel(model);
	});

	onDestroy(() => {
		monaco?.editor.getModels().forEach((model: any) => model.dispose());
		editor?.dispose();
	});
</script>

<div class="w-full h-full" bind:this={editorContainer}></div>

