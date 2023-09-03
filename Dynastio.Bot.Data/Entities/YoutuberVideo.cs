using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Data
{
    public class YoutuberVideo
    {
        public string url { get; set; }
        public ulong user { get; set; }
        public DateTime createdAt { get; set; }
    }
}
