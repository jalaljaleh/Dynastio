using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Dynastio.Net
{
    public class Leaderboardscore
    {
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Nickname { get; set; }
        public long Score { get; set; }
    }
}
