using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public static class YouTubeExtenstions
    {
        public static string ToYoutubeVideoUrl(this ResourceId id)
        {
            return "https://youtu.be/" + id.VideoId;
        }
        public static string ToYoutubeChannelSubConfirmation(this string channelId)
        {
            return $"https://www.youtube.com/channel/{channelId}/?sub_confirmation=1";
        }
    }
}
