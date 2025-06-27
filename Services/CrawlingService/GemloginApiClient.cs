using Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace Services.CrawlingService
{
    public class GemloginApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public GemloginApiClient(HttpClient httpClient, string apiBaseUrl)
        {
            _httpClient = httpClient;
            _apiBaseUrl = apiBaseUrl;
        }

        public async Task<StartProfileResponse?> StartProfileAsync(int profileId, StartProfileOptions? options = null)
        {
            var requestUrl = $"{_apiBaseUrl}/api/Profile/start/{profileId}?";
            var queryParams = new List<string>();

            if (options != null)
            {
                if (!string.IsNullOrEmpty(options.AdditionalArgs))
                {
                    queryParams.Add($"addination_args={HttpUtility.UrlEncode(options.AdditionalArgs)}");
                }
                if (!string.IsNullOrEmpty(options.WindowPosition))
                {
                    queryParams.Add($"win_pos={HttpUtility.UrlEncode(options.WindowPosition)}");
                }
                if (!string.IsNullOrEmpty(options.WindowSize))
                {
                    queryParams.Add($"win_size={HttpUtility.UrlEncode(options.WindowSize)}");
                }
                if (options.WindowScale.HasValue)
                {
                    queryParams.Add($"win_scale={options.WindowScale.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                }
            }

            if (queryParams.Any())
            {
                requestUrl += string.Join("&", queryParams);
            }

            Console.WriteLine($"Sending GET request to start profile {profileId}: {requestUrl}");

            try
            {
                var response = await _httpClient.GetAsync(requestUrl);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Profile start successful! Response: {responseBody}");
                    var jsonOptions = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    };
                    return JsonSerializer.Deserialize<StartProfileResponse>(responseBody, jsonOptions);
                }
                else
                {
                    Console.WriteLine($"Error starting Gemlogin profile. Status: {response.StatusCode}");
                    Console.WriteLine($"Response Body: {responseBody}");
                    return null;
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Network error during profile start: {e.Message}");
                return null;
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Error deserializing start profile response from Gemlogin: {e.Message}");
                return null;
            }
        }
    }
}
