using System.Text.Json;

namespace Common.Helpers;

public static class GeoLocationHelper
{
    private static readonly HttpClient _httpClient = new();

    public static async Task<string> GetLocationFromIpAsync(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
            return "IP không hợp lệ";

        try
        {
            var url = $"https://ipapi.co/{ip}/json/";
            var response = await _httpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);

            var city = json.RootElement.TryGetProperty("city", out var cityElement) ? cityElement.GetString() : null;
            var country = json.RootElement.TryGetProperty("country_name", out var countryElement)
                ? countryElement.GetString()
                : null;

            if (!string.IsNullOrWhiteSpace(city) && !string.IsNullOrWhiteSpace(country))
                return $"{city}, {country}";

            return "Không xác định vị trí";
        }
        catch
        {
            return "Lỗi lấy vị trí từ IP";
        }
    }
}