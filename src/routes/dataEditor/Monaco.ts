// I Yoinked the monaco editor code from here: https://www.codelantis.com/blog/sveltekit-monaco-editor

import * as monaco from 'monaco-editor';
import jsonWorker from 'monaco-editor/esm/vs/language/json/json.worker?worker';

self.MonacoEnvironment = {
	getWorker: function (_: string, label: string) {
		return new jsonWorker();
	}
};

export default monaco;
