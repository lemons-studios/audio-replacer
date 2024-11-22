using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    }

    internal class Tag
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
