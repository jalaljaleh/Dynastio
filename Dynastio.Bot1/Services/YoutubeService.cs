using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public class YoutubeService
    {
        private YouTubeService youtube;
        private readonly string mainChannelId;
        private readonly string apiKey;
        public List<SearchResult> Videos { get; set; }
        public YoutubeService(string apiKey, string mainChannelId)
        {
            if (apiKey == null || mainChannelId == null)
            {
                Program.Log("YoutubeService", "api key not found.", ConsoleColor.Red);
                return;
            }
            this.apiKey = apiKey;
            this.mainChannelId = mainChannelId;
        }
        public async Task InitializeAsync()
        {
            if (Program.IsDebug())
            {
                Program.Log("YoutubeService", "Disabled in debug mode.", ConsoleColor.DarkYellow);
                return;
            }

            Program.Log("YoutubeService", "Initializing..");

            youtube = new YouTubeService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                ApiKey = apiKey
            });

            Program.Log("YoutubeService", "Get channel videos..");

            Videos = await GetAllChannelVideos(mainChannelId);
            Program.IsYoutubeServiceInitialized = true;

            Program.Log("YoutubeService", "Initialized.");
        }
        public Task<List<SearchResult>> GetAllChannelVideos(string channelId)
        {
            List<SearchResult> res = new List<SearchResult>();

            string nextpagetoken = " ";

            while (nextpagetoken != null)
            {
                var searchListRequest = youtube.Search.List("snippet");
                searchListRequest.MaxResults = 50;
                searchListRequest.ChannelId = channelId;
                searchListRequest.PageToken = nextpagetoken;
                searchListRequest.Type = "video";

                // Call the search.list method to retrieve results matching the specified query term.
                var searchListResponse = searchListRequest.Execute();

                // Process  the video responses 
                res.AddRange(searchListResponse.Items);

                nextpagetoken = searchListResponse.NextPageToken;

            }
            return Task.FromResult(res);
        }
        public Task<List<SearchResult>> SearchVideoByKeyword(string keyword)
        {
            var searchListRequest = youtube.Search.List("snippet");
            searchListRequest.MaxResults = 50;
            searchListRequest.Q = keyword;
            searchListRequest.Type = "video";

            var searchListResponse = searchListRequest.Execute();
            return Task.FromResult(searchListResponse.Items.ToList());
        }

    }
}
