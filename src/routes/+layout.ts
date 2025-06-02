export const prerender = true;
export const ssr = false;

import { setTheme } from "@tauri-apps/api/app";
import "beercss";
import "material-dynamic-colors";
import { onMount } from "svelte";


setTheme("dark");