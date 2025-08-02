import fs from "node:fs";
import https from "node:https";
import path from "node:path";

const releaseUrl = "https://api.github.com/repos/lemons-studios/audio-replacer/releases/latest";
const outputLocation = "src-tauri/resources/releaseData.md";

const options = {
  headers: {
    "User-Agent": "audio-replacer-release-fetcher",
    "Accept": "application/vnd.github+json"
  }
};

https.get(releaseUrl, options, (res) => {
  if (res.statusCode !== 200) {
    console.error(`Request failed with status: ${res.statusCode}`);
    res.resume();
    return;
  }

  let raw = "";
  res.on("data", chunk => raw += chunk);
  res.on("end", () => {
    try {
      const json = JSON.parse(raw);
      const releaseNotes = json.body || "No release notes found.";
      fs.writeFileSync(outputLocation, releaseNotes);
      console.log("Release notes saved.");
    } catch (e) {
      console.error("Failed to parse response:", e);
    }
  });
}).on("error", (e) => {
  console.error("Request error:", e);
});
