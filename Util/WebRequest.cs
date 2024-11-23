using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AudioReplacer2.Util
{
    public class WebRequest
    {
        public async Task<string> GetWebVersion(string url)
        {
            using var client = new HttpClient();
            
            client.DefaultRequestHeaders.Add("User-Agent", "Audio Replacer 2");
            var apiResponse = await client.GetAsync(url);

            if (apiResponse.IsSuccessStatusCode)
            {
                var responseData = await apiResponse.Content.ReadAsStringAsync();
                var jsonTags = JsonSerializer.Deserialize<Tag[]>(responseData);
                if (IsJsonValid(jsonTags)) return jsonTags[0].Name;

                throw new Exception("No tags found in data");
            }
            throw new Exception($"GitHub API responded with non-successful status code {apiResponse.StatusCode}");
        }

        private bool IsJsonValid(Tag[] jsonTags)
        {
            // Yummy boolean pattern
            return jsonTags is { Length: > 0 };
        }
    }

    internal class Tag
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
