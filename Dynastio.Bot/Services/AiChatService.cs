using Discord;
using Discord.WebSocket;
using Dynastio.Bot.Database;
using Dynastio.Bot.Global.Helper;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly DynastioApi _dynastio;

        public AiChatService(IServiceProvider services)
        {
            _dynastio = services.GetRequiredService<DynastioApi>();

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

        // ── Rate-limiting state ───────────────────────────────────────────
        // Holds the UTC timestamps of the last 60 requests.
        /// <summary>
        /// Sliding-window rate limiter: max 60 requests in any 60-minute span.
        /// </summary>
        private readonly object _rateLock = new object();
        private readonly Queue<DateTimeOffset> _timestamps = new Queue<DateTimeOffset>();
        private DateTimeOffset _lastSuccess = DateTimeOffset.MinValue;

        public bool TryAcquireSlot()
        {
            var now = DateTimeOffset.UtcNow;

            lock (_rateLock)
            {
                // Remove timestamps older than 60 minutes
                while (_timestamps.Count > 0 && now - _timestamps.Peek() > TimeSpan.FromMinutes(60))
                {
                    _timestamps.Dequeue();
                }

                // Rule 1: at least 2 minutes since last success
                if (now - _lastSuccess < TimeSpan.FromMinutes(1))
                {
                    return false;
                }

                // Rule 2: no more than 60 in the last 60 minutes
                if (_timestamps.Count >= 60)
                {
                    return false;
                }

                // Passed both checks → success
                _timestamps.Enqueue(now);
                _lastSuccess = now;
                return true;
            }
        }
        public async Task ReplyMessageAsync(SocketUserMessage msg, User user)
        {
            string data = "account not linked";
            try
            {
                if (user.HasLinkedAccount)
                {
                    var profile = await user.GetDefaultAccount()?.GetCachedProfileCardAsync(_dynastio) ?? null;
                    var killed = string.Join(", ", profile.Stat.Kill.OrderBy(a => a.Value).Take(4).Select(a => a.Key.ToString() + " " + a.Value));
                    var killedFrom = string.Join(", ", profile.Stat.Death.OrderBy(a => a.Value).Take(4).Select(a => a.Key.ToString() + " " + a.Value));
                    data = profile is null ? data : JsonSerializer.Serialize(new
                    {
                        personal_chest_items = string.Join(", ", profile.Chest.Items.Select(a => a.ItemType.ToString() + " count " + a.Count)),
                        badges = string.Join(", ", profile.Profile.Badges.Select(a => a.ToString())),
                        coins = profile.Profile.Coins,
                        experience = profile.Profile.Experience,
                        lastestactivity = profile.Profile.LastActiveAt,
                        latestserver = profile.Profile.LatestServer,
                        level = profile.Profile.Level,
                        killed = killed,
                        killedFrom= killedFrom
                    });
                }
            }
            catch
            {
            }

            string systemPrompt = AiHelper.answer + $"data: {data} \n\n User Message= {msg.Author.Mention}\n\n said: {msg.Content}";

            // 3) Query
            string aiResponse = await QueryAsync(null, systemPrompt);

            // 4) Send back
            await msg.ReplyAsync(aiResponse);
        }
        private async Task<string> PostQueryAsync(string systemPrompt, string userPrompt, string userId = "1")
        {
            // Pick a random Origin header per request
            string origin = _origins[Random.Shared.Next(_origins.Length)];

            // Build payload
            var payload = new
            {
                system = systemPrompt,
                prompt = userPrompt,
                userId = $"#/chat/{userId}{Common.Random.Next(99999999)}",
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
