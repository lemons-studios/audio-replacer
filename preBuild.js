import fs from "node:fs";
import os from "node:os";
import path from "node:path";

const binaryName = `ffmpeg-x86_64-${os.platform() === "linux" ? "unknown-linux-gnu" : os.platform() === "win32" ? "pc-windows-msvc.exe" : ""}`;
const pathToBinary = path.join("src-tauri", "platform-binaries", binaryName);

const binDir = "./src-tauri/binaries";
const copiedPath = path.join(binDir, binaryName);

if (fs.existsSync(copiedPath)) {
  fs.unlinkSync(copiedPath);
}

fs.copyFileSync(pathToBinary, copiedPath);
