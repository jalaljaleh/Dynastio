using Dynastio.Bot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    internal class GuildFactory
    {
        public static Guild CreateDefault(ulong Id)
        {
            return new Guild()
            {
                Id = Id,
                XpSystemSettings = new XpSystemSettings()
                {
                    IsEnabled = false,
                    AllowedXpChannels = new()
                },
                BadgeBridgeSettings = new BadgeBridgeSettings()
                {
                    IsEnabled = false
                }
            };
        }
    }
}
