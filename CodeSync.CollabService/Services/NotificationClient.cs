using System.Text;
using System.Text.Json;

namespace CodeSync.CollabService.Services
{
    public class NotificationClient
    {
        public readonly HttpClient _http;
        private readonly string _notifUrl;
        private readonly string _projectUrl;

        public NotificationClient(
            HttpClient http,
            IConfiguration config)
        {
            _http = http;
            _notifUrl = config[
                "Services:NotificationServiceUrl"]!;
            _projectUrl = config[
                "Services:ProjectServiceUrl"]!;
        }

        public async Task CreateAsync(object payload)
        {
            try
            {
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(
                    json, Encoding.UTF8, "application/json");
                await _http.PostAsync(
                    $"{_notifUrl}/api/notifications/create",
                    content);
            }
            catch { }
        }

        public async Task<List<ProjectMemberInfo>>
            GetProjectMembersAsync(
                Guid projectId, string authHeader)
        {
            try
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"{_projectUrl}/api/projects/members/{projectId}");

                if (!string.IsNullOrEmpty(authHeader))
                    request.Headers.Add(
                        "Authorization", authHeader);

                var response = await _http.SendAsync(request);
                var json = await response.Content
                    .ReadAsStringAsync();

                return JsonSerializer
                    .Deserialize<List<ProjectMemberInfo>>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new();
            }
            catch
            {
                return new();
            }
        }
    }

    public class ProjectMemberInfo
    {
        public Guid UserId { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}