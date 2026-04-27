using System.Text;
using System.Text.Json;

namespace CodeSync.ExecutionService.Services
{
    public class Judge0Client
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _apiHost;
        private readonly string _baseUrl;

        public Judge0Client(IConfiguration config)
        {
            _http = new HttpClient();
            _baseUrl = config["Judge0:BaseUrl"]!;
            _apiKey = config["Judge0:ApiKey"]!;
            _apiHost = config["Judge0:ApiHost"]!;
        }

        public async Task<Judge0Result> SubmitAndWait(
            int languageId,
            string sourceCode,
            string? stdin)
        {
            // Submit
            var token = await Submit(
                languageId, sourceCode, stdin);

            // Poll until done (max 30 seconds)
            for (int i = 0; i < 15; i++)
            {
                await Task.Delay(2000);
                var result = await GetResult(token);

                // Status 1 = In Queue, 2 = Processing
                if (result.StatusId != 1 && result.StatusId != 2)
                    return result;
            }

            return new Judge0Result
            {
                StatusId = 5,
                StatusDescription = "Time Limit Exceeded",
                Stderr = "Execution timed out"
            };
        }

        private async Task<string> Submit(
            int languageId,
            string sourceCode,
            string? stdin)
        {
            var body = new
            {
                language_id = languageId,
                source_code = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(sourceCode)),
                stdin = stdin != null
                    ? Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(stdin))
                    : null,
                base64_encoded = true,
                wait = false
            };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_baseUrl}/submissions?base64_encoded=true");

            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8, "application/json");

            request.Headers.Add("X-RapidAPI-Key", _apiKey);
            request.Headers.Add("X-RapidAPI-Host", _apiHost);

            var response = await _http.SendAsync(request);
            var json = await response.Content
                .ReadAsStringAsync();

            var doc = JsonDocument.Parse(json);
            return doc.RootElement
                .GetProperty("token").GetString()!;
        }

        private async Task<Judge0Result> GetResult(string token)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_baseUrl}/submissions/{token}?base64_encoded=true&fields=*");

            request.Headers.Add("X-RapidAPI-Key", _apiKey);
            request.Headers.Add("X-RapidAPI-Host", _apiHost);

            var response = await _http.SendAsync(request);
            var json = await response.Content
                .ReadAsStringAsync();

            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            return new Judge0Result
            {
                StatusId = root.GetProperty("status")
                    .GetProperty("id").GetInt32(),
                StatusDescription = root.GetProperty("status")
                    .GetProperty("description").GetString()
                    ?? "Unknown",
                Stdout = DecodeBase64(
                    GetStringOrNull(root, "stdout")),
                Stderr = DecodeBase64(
                    GetStringOrNull(root, "stderr")),
                CompileOutput = DecodeBase64(
                    GetStringOrNull(root, "compile_output")),
                Time = GetStringOrNull(root, "time"),
                Memory = GetIntOrNull(root, "memory")
            };
        }

        private static string? GetStringOrNull(
            JsonElement el, string prop)
        {
            if (el.TryGetProperty(prop, out var val)
                && val.ValueKind != JsonValueKind.Null)
                return val.GetString();
            return null;
        }

        private static int? GetIntOrNull(
            JsonElement el, string prop)
        {
            if (el.TryGetProperty(prop, out var val)
                && val.ValueKind != JsonValueKind.Null)
                return val.GetInt32();
            return null;
        }

        private static string? DecodeBase64(string? value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            try
            {
                var bytes = Convert.FromBase64String(value);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return value;
            }
        }
    }

    public class Judge0Result
    {
        public int StatusId { get; set; }
        public string StatusDescription { get; set; } = "";
        public string? Stdout { get; set; }
        public string? Stderr { get; set; }
        public string? CompileOutput { get; set; }
        public string? Time { get; set; }
        public int? Memory { get; set; }
    }
}