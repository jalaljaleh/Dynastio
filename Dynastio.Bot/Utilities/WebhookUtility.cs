using Discord.Webhook;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    internal class WebhookUtility 
    {
        public WebhookUtility(IServiceProvider services) 
        {

        }
        public static string GetWebhookUrl(IWebhook webhook) => $"https://discord.com/api/webhooks/{webhook.Id}/{webhook.Token}";
        public static async Task<IWebhook> GetWebhookAsync(ITextChannel channel)
        {
            var webhooks = await channel.GetWebhooksAsync();
            if (webhooks == null || webhooks.Count == 0 || webhooks.Any(a => a.Name == "Dynast.io") is false)
            {
                var webhook = await channel.CreateWebhookAsync("Dynast.io");
                webhooks = await channel.GetWebhooksAsync();
            }
            return webhooks.First(a => a.Name == "Dynast.io");
        }
    }
}
