using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public static class YoutuberExtenstions
    {
        public static string ToYoutubeChannel(this string Id)
        {
            return $"https://www.youtube.com/channel/{Id}";
        }
    }
}
