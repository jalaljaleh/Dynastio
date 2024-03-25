
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Net
{
    public class Server
    {
        public bool IsMatched(string nickname)
        {
            return Label.ToLower().Trim().Contains(nickname.ToLower().Trim());
        }

        public string GetDirectLink()
        {
            return $"https://dynast.io/?direct={Ip}:{Port}";
        }

        [JsonProperty("peer_key")]
        public long? PeerKey { get; set; }


        [JsonProperty("ssl_port")]
        public int SslPort { get; set; }

        [JsonProperty("ssl_ping_port")]
        public int SslPingPort { get; set; }

        [JsonProperty("ssl_host")]
        public string SslHost { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("ping_port")]
        public int PingPort { get; set; }

        [JsonProperty("client_count")]
        public int PlayersCount { get; set; }

        [JsonProperty("connections_limit")]
        public int ConnectionsLimit { get; set; }

        [JsonProperty("map")]
        public string Map { get; set; }

        [JsonProperty("map_hash")]
        public string MapHash { get; set; }

        [JsonProperty("game_mode")]
        public string GameMode { get; set; }

        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("top_player_name")]
        public string TopPlayerName { get; set; }

        [JsonProperty("top_player_score")]
        public long TopPlayerScore { get; set; }

        [JsonProperty("top_player_level")]
        public int TopPlayerLevel { get; set; }

        [JsonProperty("load_avg")]
        public int LoadAvg { get; set; }

        [JsonProperty("load_max")]
        public int LoadMax { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("custom_mode")]
        public bool CustomMode { get; set; }

        [JsonProperty("private")]
        public bool IsPrivate { get; set; }

        [JsonProperty("frame_drop")]
        public int FrameDrop { get; set; }

        [JsonProperty("secret")]
        public string Secret { get; set; }

        [JsonProperty("lifetime")]
        public int Lifetime { get; set; }

        [JsonProperty("server_time")]
        public double? ServerTime { get; set; }

        [JsonProperty("players")]
        public List<Player> Players { get; set; }

        [JsonProperty("events")]
        public List<ServerEvent> Events { get; set; }
        [JsonProperty("backend")]
        public string Backend { get; set; }
        [JsonProperty("new_io")]
        public bool NewIo { get; set; }
        public List<Player> GetPlayers()
        {
            return Players.Select(a => a.Update(this)).ToList() ?? new();
        }

    }

}