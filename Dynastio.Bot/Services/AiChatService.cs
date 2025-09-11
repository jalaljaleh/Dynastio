using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    /// <summary>
    /// High-performance wrapper for the Binjie GPT endpoint.
    /// Reuses a single HttpClient and minimizes allocations.
    /// </summary>
    public class AiChatService
    {
        private const string BaseUrl = "https://api.binjie.fun/";
        private static readonly string[] _origins = new[]
        {
            "https://c2.binjie.fun/",
            "https://c.binjie.fun/",
            "https://cht18.aichatosd2.com",
            "https://chat18.aichatos68.com/"
        };

        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions;

        public AiChatService()
        {
            _http = new();
            _http.BaseAddress = new Uri(BaseUrl);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            _http.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            _http.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            _http.DefaultRequestHeaders.Add("Access-Control-Allow-Origin", "*");

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        /// <summary>
        /// Posts system+user prompts to Binjie and returns the AI’s raw text.
        /// </summary>
        /// <param name="systemPrompt">Your system instructions.</param>
        /// <param name="userPrompt">The user’s message.</param>
        /// <param name="userId">Unique Chat ID (default “1”).</param>
        /// 
        public async Task<string> QueryAsync(string systemPrompt, string userPrompt, string userId = "1")
        {
            var aiReply = await PostQueryAsync(systemPrompt, userPrompt, userId).TryAsync();
            if (aiReply.isSuccessful is false || string.IsNullOrWhiteSpace(aiReply.result) || aiReply.result.Contains(".com") || aiReply.result.Contains("https://"))
            {
                return "I can't answer to this !";
            }
            else return aiReply.result;
        }
        private async Task<string> PostQueryAsync(string systemPrompt, string userPrompt, string userId = "1")
        {
            // Pick a random Origin header per request
            string origin = _origins[Random.Shared.Next(_origins.Length)];

            // Build payload
            var payload = new
            {
                system = systemPrompt,
                prompt = $"User prompt:\n{userPrompt}",
                userId = $"#/chat/{userId}",
                network = true,
                stream = false
            };
            string json = JsonSerializer.Serialize(payload, _jsonOptions);

            // Prepare HTTP request
            using var request = new HttpRequestMessage(
                HttpMethod.Post, "api/generateStream");
            request.Headers.UserAgent.ParseAdd(
                UserAgentGenerator.GenerateUserAgent());
            request.Headers.Add("Origin", origin);
            request.Content = new StringContent(
                json, Encoding.UTF8, "application/json");

            // Send and read
            using var response = await _http
                .SendAsync(request)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false);
        }
    }
}
