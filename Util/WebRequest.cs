using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AudioReplacer.Util
{
    public class WebRequest
    {
        private readonly HttpClient client = new() 
        { 
            DefaultRequestHeaders =
        {
            {
                "User-Agent", "Audio Replacer"
            }
        }
        };

        public async Task<string> GetWebVersion(string url)
        {
            try
            {
                var apiResponse = await client.GetAsync(url);
                if (!apiResponse.IsSuccessStatusCode) throw new Exception($"API responded with status code {apiResponse.StatusCode}");
                string responseData = await apiResponse.Content.ReadAsStringAsync();

                var jsonTags = JArray.Parse(responseData);
                if (jsonTags.Count == 0) throw new Exception("No valid tags found in response data");

                string name = jsonTags[0]["name"]?.ToString();
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

        public async Task<string> GetWebData(string url) // I sure do love stealing my own code!!
        {
            using HttpClient httpClient = new HttpClient();
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException) { return string.Empty; }
        }

        public void DownloadFile(string url, string outPath, string outName)
        {
            try
            {
                using var webStream = client.GetStreamAsync(url);
                using var fileStream = new FileStream($"{outPath}\\{outName}", FileMode.OpenOrCreate);
                webStream.Result.CopyTo(fileStream);
            }
            catch (AggregateException e)
            {
                throw new AggregateException(e);
            }
        }
    }
}
