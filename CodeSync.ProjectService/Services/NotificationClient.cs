using System.Text;
using System.Text.Json;

namespace CodeSync.ProjectService.Services
{
    public class NotificationClient
    {
        public readonly HttpClient _http;
        private readonly string _baseUrl;

        public NotificationClient(
            HttpClient http,
            IConfiguration config)
        {
            _http = http;
            _baseUrl = config["Services:NotificationServiceUrl"]!;
        }

        public async Task CreateAsync(object payload)
        {
            try
            {
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(
                    json, Encoding.UTF8, "application/json");

                await _http.PostAsync(
                    $"{_baseUrl}/api/notifications/create",
                    content);
            }
            catch
            {
                // do not break main flow if notification fails
            }
        }
    }
}