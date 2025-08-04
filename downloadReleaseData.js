import fs from "node:fs";
import https from "node:https";

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
    } catch (e) {
      console.error("Failed to parse response:", e);
      // This is just in case the download fails for some reason (or if offline). App will always launch regardless of weather or not latest release markdown is present
      fs.writeFileSync(outputLocation, '# Failed to embed latest release notes. Submit a bug report on the GitHub page');
    }
  });
}).on("error", (e) => {
  console.error("Request error:", e);
  fs.writeFileSync(outputLocation, '# Failed to embed latest release notes. Submit a bug report on the GitHub page');
});


