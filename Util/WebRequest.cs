using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AudioReplacer2.Util
{
    public class WebRequest
    {
        private readonly HttpClient client = new HttpClient
        {
            DefaultRequestHeaders = { { "User-Agent", "Audio Replacer 2" } }
        };

        public async Task<string> GetWebVersion(string url)
        {
            try
            {
                var apiResponse = await client.GetAsync(url);
                if (!apiResponse.IsSuccessStatusCode) throw new Exception($"API responded with status code {apiResponse.StatusCode}");
                var responseData = await apiResponse.Content.ReadAsStringAsync();
                
                var jsonTags = JArray.Parse(responseData);
                if (jsonTags.Count == 0) throw new Exception("No valid tags found in response data");

                var name = jsonTags[0]["name"]?.ToString();
                if (string.IsNullOrEmpty(name)) throw new Exception("The 'name' property is missing or empty in the first tag.");
                return name;
            }
            catch (JsonException ex)
            {
                throw new Exception($"Failed to parse JSON: {ex}");
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred during the web request: {ex.Message}");
            }
        }
    }
}
