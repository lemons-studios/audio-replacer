using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.IO.Compression;

namespace AudioReplacer2.Util
{
    public class WebRequest
    {
        public async Task<string> GetWebVersion()
        {
            string url = "https://api.github.com/repos/lemons-studios/audio-replacer-2/tags";
            using HttpClient client = new HttpClient();
            
            client.DefaultRequestHeaders.Add("User-Agent", "Audio Replacer 2");
            var apiResponse = await client.GetAsync(url);

            if (apiResponse.IsSuccessStatusCode)
            {
                string responseData = await apiResponse.Content.ReadAsStringAsync();
                var jsonTags = JsonSerializer.Deserialize<Tag[]>(responseData);

                if (jsonTags != null && jsonTags.Length > 0)
                {
                    return jsonTags[0].Name;
                }
                throw new Exception("No tags found in data");
            }
            throw new Exception($"GitHub API responded with non-successful status code {apiResponse.StatusCode}");
        }

        public async Task DownloadAndExtractFile(string url, string extractPath)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            var destinationPath = Path.Combine(extractPath, entry.FullName);
                            entry.ExtractToFile(destinationPath, true);
                        }
                    }
                }
            }
        }
    }

    internal class Tag
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
