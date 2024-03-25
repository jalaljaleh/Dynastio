
using Dynastio.Net.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Dynastio.Net
{
    public class DynastioApi
    {
        private readonly TimeSpan _timeout;
        private HttpClient _client;
        private HttpClientHandler _clientHandler;
        private string tokenKey, tokenValue;
        private string _baseAddress = "https://auth.dynast.cloud";
        private string MediaTypeJson = "application/json";
        private Random random = new Random();
        public DynastioApi(string token)
        {
            tokenKey = token.Split(':')[0];
            tokenValue = token.Split(":")[1];
            _timeout = TimeSpan.FromSeconds(20);


            _players = new Cacheable<List<Player>>(TimeSpan.FromSeconds(30), GetPlayersAsync);
            _servers = new Cacheable<List<Server>>(TimeSpan.FromSeconds(30), GetServersAsync);
            _version = new Cacheable<Version>(TimeSpan.FromSeconds(500), GetVersionAsync);
            _changelog = new Cacheable<string>(TimeSpan.FromSeconds(500), GetChangeLogAsync);
            _leaderboardcoin = new Cacheable<List<Leaderboardcoin>>(TimeSpan.FromSeconds(250), GetLeaderboardcoinsAsync);
            _leaderboardscore = new Cacheable<List<Leaderboardscore>>(TimeSpan.FromSeconds(250), GetLeaderboardscoresAsync);
            _featuredVideos = new Cacheable<List<FeaturedVideos>>(TimeSpan.FromMinutes(29), GetFeaturedVideosAsync);

            CreateHttpClient();
        }

        private readonly Cacheable<List<Player>> _players;
        private readonly Cacheable<List<Server>> _servers;
        private readonly Cacheable<Version> _version;
        private readonly Cacheable<string> _changelog;
        private readonly Cacheable<List<Leaderboardcoin>> _leaderboardcoin;
        private readonly Cacheable<List<Leaderboardscore>> _leaderboardscore;
        private readonly Cacheable<List<FeaturedVideos>> _featuredVideos;

        public Version Version { get => _version.Value; }
        public List<Player> OnlinePlayers { get => _players.Value; }
        public List<Server> OnlineServers { get => _servers.Value; }
        public List<Leaderboardcoin> Leaderboardcoins { get => _leaderboardcoin.Value; }
        public List<Leaderboardscore> Leaderboardscore { get => _leaderboardscore.Value; }
        public string Changelog { get => _changelog.Value; }
        public List<FeaturedVideos> FeaturedVideos { get => _featuredVideos.Value; }

        public async Task<List<Server>> GetServersAsync() => await GetServersAsync(ServerType.AllServersWithAllPlayers);
        public async Task<List<Server>> GetServersAsync(ServerType serverType = default)
        {
            string url = serverType switch
            {
                ServerType.AllServersWithAllPlayers => "all?full=true",
                ServerType.PublicServersWithAllPlayers => "?full=true",
                ServerType.AllServersWithTopPlayers => "all",
                ServerType.PublicServersWithTopPlayers => "#",
                _ => ""
            };

            var result = await GetAsync("https://announcement-amsterdam-0-alpaca.dynast.cloud/" + url + "&random=" + random.Next());
            var data = JsonConvert.DeserializeObject<DataType<List<Server>>>(result);
            return data.Servers;
        }
        public async Task<List<Player>> GetPlayersAsync()
            => await Task.FromResult(_servers.Value.SelectMany(a => a.GetPlayers() ?? null).ToList());

        public async Task<Version> GetVersionAsync()
        {
            var result = await GetAsync("https://dynast.cloud/version.json");
            return JsonConvert.DeserializeObject<Version>(result);
        }
        public async Task<string> GetChangeLogAsync()
        {
            var result = await GetAsync("https://dynast.cloud/changelog.txt");
            return JsonConvert.DeserializeObject<string>(result);
        }
        public async Task<DiscordRank> UpdateDiscordRank(string accountId, int rank)
        {
            var result = await PostAsync<DataType<DiscordRank>>(_baseAddress + $"/write_api/set_user_discord_rank?uid={accountId}", new DiscordRank(rank));
            return result.data;
        }
        public async Task<List<Leaderboardcoin>> GetLeaderboardcoinsAsync()
        {
            var result = await GetAsync(_baseAddress + "/api/get_top_by_coins");
            var data = JsonConvert.DeserializeObject<DataType<Leaderboardcoin[]>>(result);
            return data.data.ToList();
        }
        public async Task<bool> GetUserPincodeStatusAsync(string Id, string pincode)
        {
            var result = await GetAsync(_baseAddress + $"/api/check_pincode?uid={Id}&pin={pincode}");
            var data = JsonConvert.DeserializeObject<DataType<bool>>(result);
            return data.data;
        }
        public async Task<List<Leaderboardscore>> GetLeaderboardscoresAsync()
        {
            var result = await GetAsync(_baseAddress + "/leaderboard/list_all");
            var data = JsonConvert.DeserializeObject<DataType<List<Leaderboardscore>>>(result);
            return data.data;
        }
        public async Task<UserRank> GetUserRankAsync(string playerId)
        {
            var result = await GetAsync(_baseAddress + "/api/get_user_rank?uid=" + playerId);
            var data = JsonConvert.DeserializeObject<DataType<List<int>>>(result);
            return new UserRank(data.data);
        }
        public async Task<PlayerStat> GetUserStatAsync(string playerId)
        {
            var result = await GetAsync(_baseAddress + "/api/get_user_stat?uid=" + playerId);
            var data = JsonConvert.DeserializeObject<DataType<string>>(result);
            var cleardata = JsonConvert.DeserializeObject(data.data).ToString();
            var final = JsonConvert.DeserializeObject<PlayerStat>(cleardata);
            return final;
        }
        public async Task<ProfileCard> GetUserProfileCardAsync(string playerId)
        {
            var result = await GetAsync(_baseAddress + "/api/get_user_card?uid=" + playerId);
            var data = JsonConvert.DeserializeObject<DataType<ProfileCardEntitiy>>(result);

            var clearStat = JsonConvert.DeserializeObject(data.data.Stat).ToString();
            var Stat = JsonConvert.DeserializeObject<PlayerStat>(clearStat);

            var clearPchest = JsonConvert.DeserializeObject(data.data.Chest).ToString();
            var pchest = ParseToChest(clearPchest);

            return new ProfileCard()
            {
                Profile = data.data.Profile,
                Chest = pchest,
                Stat = Stat,
            };
        }
        public async Task<Profile> GetUserProfileAsync(string playerId)
        {
            var result = await GetAsync(_baseAddress + "/api/get_user_profile?uid=" + playerId);
            var data = JsonConvert.DeserializeObject<DataType<Profile>>(result);
            return data.data;
        }
        public async Task<Personalchest> GetUserPersonalchestAsync(string playerId)
        {
            var result = await GetAsync(_baseAddress + "/api/get_user_chest?uid=" + playerId);
            var data = JsonConvert.DeserializeObject<DataType<string>>(result);
            return ParseToChest(data.data);
        }
        internal Personalchest ParseToChest(string data)
        {
            var cleardata = JsonConvert.DeserializeObject(data).ToString();
            var final = JsonConvert.DeserializeObject<JObject>(cleardata).SelectToken("items").ToArray(); ;

            var chestItems = new List<PersonalChestItem>();
            foreach (var item in final)
            {
                var item_ = new PersonalChestItem()
                {
                    index = int.Parse(item[0].ToString()),
                    ItemType = (ItemType)int.Parse(item[1].ToString()),
                    Count = int.Parse(item[2].ToString()),
                    Durablity = int.Parse(item[3].ToString()),
                    Details = item[4].ToString(),
                    OwnerID = item[5].ToString(),
                    Token = item[6].ToString()
                };
                chestItems.Add(item_);
            }
            return new Personalchest(chestItems);
        }
        public async Task<UserSurroundingRank> GetUserSurroundingRankAsync(string playerId)
        {
            var result = await GetAsync(_baseAddress + "/leaderboard/surrounding?uid=" + playerId);
            var data = JsonConvert.DeserializeObject<DataType<List<UserSurroundingRankRow[]>>>(result);
            return new UserSurroundingRank(playerId)
            {
                UsersRankDaily = data.data[0].ToList(),
                UsersRankWeekly = data.data[1].ToList(),
                UsersRankMontly = data.data[2].ToList()
            };
        }
        public async Task<List<FeaturedVideos>> GetFeaturedVideosAsync()
        {
            var result = await GetAsync(_baseAddress + "/api/get_featured_videos");
            var data = JsonConvert.DeserializeObject<DataType<List<FeaturedVideos>>>(result);
            return data.data;
        }

        private void CreateHttpClient()
        {
            _clientHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                UseDefaultCredentials = true,
            };

            _client = new HttpClient(_clientHandler, false)
            {
                Timeout = _timeout
            };

            _client.DefaultRequestHeaders.UserAgent.ParseAdd("dynastio.net");

            //_client.BaseAddress = new Uri("https://auth.dynast.io/");

            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add(tokenKey, tokenValue);
        }

        private void EnsureHttpClientCreated()
        {
            if (_client == null)
            {
                CreateHttpClient();
            }
        }

        private static string ConvertToJsonString(object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        private static string NormalizeBaseUrl(string url)
        {
            return url.EndsWith("/") ? url : url + "/";
        }
        private async Task<string> PostAsync(string url, object input)
        {


            using (var requestContent = new StringContent(ConvertToJsonString(input), Encoding.UTF8, MediaTypeJson))
            {
                using (var response = await _client.PostAsync(url, requestContent))
                {
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        private async Task<TResult> PostAsync<TResult>(string url, object input) where TResult : class, new()
        {
            var strResponse = await PostAsync(url, input);

            return JsonConvert.DeserializeObject<TResult>(strResponse, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        private async Task<TResult> GetAsync<TResult>(string url) where TResult : class, new()
        {
            var strResponse = await GetAsync(url);

            return JsonConvert.DeserializeObject<TResult>(strResponse, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        private async Task<string> GetAsync(string url)
        {
            using (var response = await _client.GetAsync(url))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        private async Task<string> PutAsync(string url, object input)
        {
            return await PutAsync(url, new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, MediaTypeJson));
        }

        private async Task<string> PutAsync(string url, HttpContent content)
        {


            using (var response = await _client.PutAsync(url, content))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        private async Task<string> DeleteAsync(string url)
        {


            using (var response = await _client.DeleteAsync(url))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        public void Dispose()
        {
            _clientHandler?.Dispose();
            _client?.Dispose();
        }
    }
}