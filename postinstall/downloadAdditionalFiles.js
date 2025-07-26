import fs from "node:fs";
import https from "node:https";
import os from "node:os";

const platform = os.platform();
const files = [
    {
        path: "src-tauri/binaries/whisper.bin",
        url: "https://cdn-lfs.hf.co/repos/64/9f/649f4fe1f9bc219d0c748dc374f358a9b97cf2596a098db8ccd83580894181aa/80147c665f9ac54ab8c3c7f55da90ac030ea40610495ba4dd0aa6304fd49d731?response-content-disposition=inline%3B+filename*%3DUTF-8%27%27ggml-base.bin%3B+filename%3D%22ggml-base.bin%22%3B&response-content-type=application%2Foctet-stream&Expires=1753430014&Policy=eyJTdGF0ZW1lbnQiOlt7IkNvbmRpdGlvbiI6eyJEYXRlTGVzc1RoYW4iOnsiQVdTOkVwb2NoVGltZSI6MTc1MzQzMDAxNH19LCJSZXNvdXJjZSI6Imh0dHBzOi8vY2RuLWxmcy5oZi5jby9yZXBvcy82NC85Zi82NDlmNGZlMWY5YmMyMTlkMGM3NDhkYzM3NGYzNThhOWI5N2NmMjU5NmEwOThkYjhjY2Q4MzU4MDg5NDE4MWFhLzgwMTQ3YzY2NWY5YWM1NGFiOGMzYzdmNTVkYTkwYWMwMzBlYTQwNjEwNDk1YmE0ZGQwYWE2MzA0ZmQ0OWQ3MzE%7EcmVzcG9uc2UtY29udGVudC1kaXNwb3NpdGlvbj0qJnJlc3BvbnNlLWNvbnRlbnQtdHlwZT0qIn1dfQ__&Signature=khMeguYYoXCpcwQpehs7ptrzyVETCnm%7EWAaBF48ZI13bslku%7ERFqcZOfrZIzKQR47JNDq7vJv%7E7OaD5s-mipyCCfhGhDk5xuFFXlGCx8ew5coSTED3fsps21dgjbTgLA1DDxoSXHXPcCtIEF-NIFFEYHrFEyFMTbc0V%7EYx%7EgLb5iKyaNoxTXsm4IzJLvp2W46%7E2c%7Ed7Lld3MSc0Tlyg3Ni4YYy-JqFVLc4CkUcm23RfB%7EQutid6M7yC2yniyjHPb5TVCFxRYrhozNYygEGHbrh-%7EQAajT2Z5PNHcPXhG8yl3WxV3cXvRG3gsBFI5az3QpOl3ENo%7Ei7zlMsFvSDEyOA__&Key-Pair-Id=K3RPWS32NSSJCE"
    },
    {
        path: `src-tauri/binaries/${platform === "win32" ? "ffmpeg-windows.exe" : "ffmpeg-linux"}`,
        url: `https://github.com/lemons-studios/audio-replacer-ffmpeg/releases/download/7.1.1/ffmpeg-${platform === "win32" ? "windows.exe" : "linux"}`
    }
];

console.log("Downloading Additional Files (If any are not found)...")
for(let i = 0; i < files.length; i++) {
    if(fs.existsSync(files[i].path)) {
        continue;
    }
    const fileStream = fs.createWriteStream(files[i].path);

    https.get(files[i].url, function(res) {
        res.pipe(fileStream);
        fileStream.on("finish", () => {
            fileStream.close();
        })
    });
}
