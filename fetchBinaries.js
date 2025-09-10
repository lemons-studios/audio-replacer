import fs from "node:fs";
import https from "node:https";
import os from "node:os";

https.globalAgent.keepAlive = false;

async function download() {
  const platform = os.platform();
  const binDir = "src-tauri/binaries";

  const files = [
    {
      path: `${binDir}/whisper.bin`,
      url: "https://huggingface.co/sandrohanea/whisper.net/resolve/main/q5_1/ggml-base.bin",
      name: "whisper.bin"
    },
    {
      path: `${binDir}/${platform === "win32" ? "ffmpeg-x86_64-pc-windows-msvc.exe" : "ffmpeg-x86_64-unknown-linux-gnu"}`,
      url: platform === "win32" 
      ? "https://github.com/ChakornK/audio-min-ffmpeg-win-crosscompile/releases/download/v8.0/ffmpeg.exe" 
      : "https://github.com/ChakornK/audio-min-ffmpeg-win-crosscompile/releases/download/v8.0/ffmpeg",
      name: `ffmpeg-${platform === "win32" ? "windows" : "linux"}`
    },
    {
      path: `${binDir}/noiseSuppression.rnnn`,
      url:  "https://raw.githubusercontent.com/richardpl/arnndn-models/refs/heads/master/std.rnnn",
      name: "noiseSuppression.rnnn"
    }
  ];
  if(!fs.existsSync(binDir)) {
    fs.mkdirSync("src-tauri/binaries");
  }

  for (let i = 0; i < files.length; i++) {
    if (fs.existsSync(files[i].path)) {
      continue;
    }
    console.log(`Downloading ${files[i].name} (${i + 1}/${files.length})`);
    try {
        await downloadFile(files[i].url, files[i].path);
    }
    catch (e) {
        console.error(`Failed to download file: ${e}`);
    }
  }
}

function downloadFile(url, dest) {
    return new Promise((resolve, reject) => {
        const file = fs.createWriteStream(dest);
        https.get(url, (res) => {
            if(res.statusCode >= 300 && res.statusCode < 400 && res.headers.location) {
                return downloadFile(res.headers.location, dest).then(resolve).catch(reject);
            } else if(res.statusCode !== 200) {
                reject(new Error(`Download of URL ${url} failed with status code ${res.statusCode}`));
                return;
            }
            res.pipe(file);
            file.on("finish", () => {
                file.close();
                resolve();
            }).on("error", (err) => {
                fs.unlink(dest, () => reject(err));
            })
        })
    })
}

download();
