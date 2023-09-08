using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.DependencyInjection;
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

        public YoutubeService(IServiceProvider services)
        {
            var config = services.GetRequiredService<Configuration>();
            if (config.YoutubeApi == null)
            {
                Global.Main.Log("Youtube Service", "Api key not found.", ConsoleColor.Red);
                return;
            }

            youtube = new YouTubeService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                ApiKey = config.YoutubeApi
            });
        }
        public static string GetUrlFromVideoId(string videoId)
        {
            return "https://www.youtube.com/watch?v=" + videoId;
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
                searchListRequest.Order = SearchResource.ListRequest.OrderEnum.Date;
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
        public async Task<bool> IsChannelExistAsync(string channelId)
        {
            var search = youtube.Channels.List("snippet");
            search.Id = channelId;
            var result = await search.ExecuteAsync();

            return result.Items.Any(a => a.Id == channelId);
        }
        public async Task<VideoListResponse> GetVideoAsync(string videoId)
        {
            VideosResource.ListRequest listRequest = youtube.Videos.List("snippet");
            listRequest.Id = videoId;
            VideoListResponse response = await listRequest.ExecuteAsync();
            return response;
        }
    }
}
