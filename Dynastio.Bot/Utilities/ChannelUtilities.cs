using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Utilities
{
    public class ChannelUtilities
    {
        public static async Task<IWebhook> GetWebhookAsync(ITextChannel channel)
        {
            try
            {
                var webhooks = await channel.GetWebhooksAsync();
                if (webhooks == null || webhooks.Count == 0)
                {
                    var webhook = await channel.CreateWebhookAsync("Dynastio");
                    webhooks.Append(webhook);
                }
                return webhooks.First();
            }
            catch
            {
                return null;
            }
        }
    }
}
