export function getPathSeparator() {
    return navigator.userAgent.includes("Windows") ? "\\" : "/";
}
