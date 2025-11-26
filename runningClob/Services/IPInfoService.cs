using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using runningClob.helpers;
using runningClob.Models;
using System.Net;

namespace runningClob.Services
{
    public interface IGeolocationService
    {
        Task<IPInfo> GetLocationByIPAsync(string ipAddress = null);
    }

    public class IPInfoService : IGeolocationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<IPInfoService> _logger;
        private readonly string _apiToken;

        public IPInfoService(HttpClient httpClient, ILogger<IPInfoService> logger, IConfiguration config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiToken = config["IPInfo:Token"]; // Add this line
            // Configure HttpClient for IPInfo API
            _httpClient.BaseAddress = new Uri("https://ipinfo.io/");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "RunningClubApp/1.0");
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task<IPInfo> GetLocationByIPAsync(string ipAddress = null)
        {
            try
            {
                string url = string.IsNullOrEmpty(ipAddress)
                    ? "json"  // Get current IP info
                    : $"{ipAddress}/json"; // Get specific IP info
                if (!string.IsNullOrEmpty(_apiToken))
                {
                    url += $"?token={_apiToken}";
                }

                _logger.LogInformation("Calling IPInfo API for URL: {Url}", url);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var ipInfo = JsonConvert.DeserializeObject<IPInfo>(json);

                    _logger.LogInformation("Successfully retrieved location: {City}, {Region}, {Country}",
                        ipInfo.City, ipInfo.Region, ipInfo.Country);

                    return ipInfo;
                }
                else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning("IPInfo API rate limit exceeded");
                    return CreateFallbackResponse("Rate limit exceeded");
                }
                else
                {
                    _logger.LogWarning("IPInfo API returned status: {StatusCode}", response.StatusCode);
                    return CreateFallbackResponse($"API error: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error calling IPInfo API");
                return CreateFallbackResponse("Network error");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout calling IPInfo API");
                return CreateFallbackResponse("API timeout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling IPInfo API");
                return CreateFallbackResponse("Unexpected error");
            }
        }

        private IPInfo CreateFallbackResponse(string errorReason)
        {
            return new IPInfo
            {
                City = "Unknown",
                Region = "Unknown",
                Country = "Unknown",
                Ip = "127.0.0.1",
                Loc = "0,0"
            };
        }
    }
}