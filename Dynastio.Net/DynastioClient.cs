using Dynastio.Bot.Global;
using Dynastio.Net.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Dynastio.Net
{
    public class DynastioClient
    {
        internal HttpClient _client;
        public DynastioClient(string token)
        {
            HttpClientHandler clientHandler = new HttpClientHandler()
            {
                AllowAutoRedirect = true,
            };
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
            {
                return true;
            };

            _client = new HttpClient(clientHandler);
            //_client.BaseAddress = new Uri("https://auth.dynast.io/");
            _client.DefaultRequestHeaders.Add(token.Split(':')[0], token.Split(':')[1]);
            _client.DefaultRequestHeaders.Add("application", "dynastio.net");
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            _players = new Cacheable<List<Player>>(TimeSpan.FromSeconds(30), GetPlayersAsync);
            _servers = new Cacheable<List<Server>>(TimeSpan.FromSeconds(30), GetServersAsync);
            _version = new Cacheable<Version>(TimeSpan.FromSeconds(500), GetVersionAsync);
            _changelog = new Cacheable<string>(TimeSpan.FromSeconds(500), GetChangeLogAsync);
            _leaderboardcoin = new Cacheable<List<Leaderboardcoin>>(TimeSpan.FromSeconds(250), GetLeaderboardcoinsAsync);
            _leaderboardscore = new Cacheable<Leaderboardscore[][]>(TimeSpan.FromSeconds(250), GetLeaderboardscoresAsync);
            _featuredVideos = new Cacheable<List<FeaturedVideos>>(TimeSpan.FromMinutes(29), GetFeaturedVideosAsync);
        }

        private readonly Cacheable<List<Player>> _players;
        private readonly Cacheable<List<Server>> _servers;
        private readonly Cacheable<Version> _version;
        private readonly Cacheable<string> _changelog;
        private readonly Cacheable<List<Leaderboardcoin>> _leaderboardcoin;
        private readonly Cacheable<Leaderboardscore[][]> _leaderboardscore;
        private readonly Cacheable<List<FeaturedVideos>> _featuredVideos;

        public Version Version { get => _version.Value; }
        public List<Player> OnlinePlayers { get => _players.Value; }
        public List<Server> OnlineServers { get => _servers.Value; }
        public List<Leaderboardcoin> Leaderboardcoins { get => _leaderboardcoin.Value; }
        public List<Leaderboardscore> LeaderboardscoresDaily { get => _leaderboardscore.Value.ToArray()[0].ToList(); }
        public List<Leaderboardscore> LeaderboardscoresWeekly { get => _leaderboardscore.Value.ToArray()[1].ToList(); }
        public List<Leaderboardscore> LeaderboardscoresMonthly { get => _leaderboardscore.Value.ToArray()[2].ToList(); }
        public string Changelog { get => _changelog.Value; }
        public List<FeaturedVideos> FeaturedVideos { get => _featuredVideos.Value; }

        internal async Task<string> GetAsync(string api)
        {
            string result;
            using (var request = new HttpRequestMessage(HttpMethod.Get, api))
            {
                var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                result = await response.Content.ReadAsStringAsync();
            }
            return result;
        }

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

            var result = await GetAsync("https://announcement-amsterdam-0-alpaca.dynast.cloud/" + url + "&random=" + Main.Random.Next());
            var data = JsonConvert.DeserializeObject<DataType<List<Server>>>(result);
            return data.Servers;
        }
        public async Task<List<Player>> GetPlayersAsync()
            => await Task.FromResult(_servers.Value.SelectMany(a => a.GetPlayers() ?? null).ToList());

        public async Task<Version> GetVersionAsync()
        {
            var result = await GetAsync("https://dynast.io/version.json");
            return JsonConvert.DeserializeObject<Version>(result);
        }
        public async Task<string> GetChangeLogAsync()
        {
            var result = await GetAsync("https://dynast.io/changelog.txt");
            return JsonConvert.DeserializeObject<string>(result);
        }
        public async Task<List<Leaderboardcoin>> GetLeaderboardcoinsAsync()
        {
            var result = await GetAsync("https://auth.dynast.io/api/get_top_by_coins");
            var data = JsonConvert.DeserializeObject<DataType<Leaderboardcoin[]>>(result);
            return data.data.ToList();
        }
        public async Task<bool> GetUserPincodeStatusAsync(string Id, string pincode)
        {
            var result = await GetAsync($"https://auth.dynast.io/api/check_pincode?uid={Id}&pin={pincode}");
            var data = JsonConvert.DeserializeObject<DataType<bool>>(result);
            return data.data;
        }
        public async Task<Leaderboardscore[][]> GetLeaderboardscoresAsync()
        {
            var result = await GetAsync("https://auth.dynast.io/leaderboard/list_all");
            var data = JsonConvert.DeserializeObject<DataType<Leaderboardscore[][]>>(result);
            return data.data;
        }
        public async Task<UserRank> GetUserRankAsync(string playerId)
        {
            var result = await GetAsync("https://auth.dynast.io/api/get_user_rank?uid=" + playerId);
            var data = JsonConvert.DeserializeObject<DataType<List<int>>>(result);
            return new UserRank(data.data);
        }
        public async Task<PlayerStat> GetUserStatAsync(string playerId)
        {
            var result = await GetAsync("https://auth.dynast.io/api/get_user_stat?uid=" + playerId);
            var data = JsonConvert.DeserializeObject<DataType<string>>(result);
            var cleardata = JsonConvert.DeserializeObject(data.data).ToString();
            var final = JsonConvert.DeserializeObject<PlayerStat>(cleardata);
            return final;
        }
        public async Task<ProfileCard> GetUserProfileCardAsync(string playerId)
        {
            var result = await GetAsync("https://auth.dynast.cloud/api/get_user_card?uid=" + playerId);
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
            var result = await GetAsync("https://auth.dynast.io/api/get_user_profile?uid=" + playerId);
            var data = JsonConvert.DeserializeObject<DataType<Profile>>(result);
            return data.data;
        }
        public async Task<Personalchest> GetUserPersonalchestAsync(string playerId)
        {
            var result = await GetAsync("https://auth.dynast.io/api/get_user_chest?uid=" + playerId);
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
            var result = await GetAsync("https://auth.dynast.io/leaderboard/surrounding?uid=" + playerId);
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
            var result = await GetAsync("https://auth.dynast.io/api/get_featured_videos");
            var data = JsonConvert.DeserializeObject<DataType<List<FeaturedVideos>>>(result);
            return data.data;
        }
    }
}