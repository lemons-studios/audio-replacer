using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace AudioReplacer2.Util
{
    public class WebRequest
    {
        public async Task<string> GetWebRequest(string url)
        {
            using var httpClient = new HttpClient();

            // Add User-Agent header
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Audio Replacer 2 Update Checker", "2.x"));

            try
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"HttpRequestException: {e.Message}");
                return string.Empty;
            }
        }

        public string SelectWebData(string data)
        {
            try
            {
                // Parse the JSON data (which is now a single object instead of an array)
                var json = JObject.Parse(data);

                // Return the latest release tag
                return json["tag_name"]?.ToString();
            }
            catch (Exception e)
            {
                // Handle any exceptions that occur during JSON parsing
                Console.WriteLine($"Error parsing the response data: {e.Message}");
                return $"Ruh ruh {e}";
            }
        }
    }
}
