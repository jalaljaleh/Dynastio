using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using Dynastio.Data;
namespace Dynastio.Bot
{
    public class ChannelUtilities
    {
        public static async Task<IWebhook> GetWebhookAsync(ITextChannel channel, string avatar = null)
        {
            try
            {
                var webhooks = await channel.GetWebhooksAsync();
                if (webhooks == null || webhooks.Count == 0)
                {
                    using (var stream = new FileStream(path: "Dynastio.jpg".ResourcesPath(), FileMode.Open))
                    {
                        var webhook = await channel.CreateWebhookAsync("Dynastio", stream);
                        webhooks.Append(webhook);
                    }
                }
                return webhooks.First();
            }
            catch
            {
                return null;
            }
        }
        public static async Task<bool> ExecuteImageOnlyModule(Guild guild, Locale locale, ulong channelId, IMessage message)
        {
            if (guild.OnlyImageChannels.Contains(channelId))
            {
                if (!message.HasAnyImage())
                {

                    try
                    {
                        await message.Author.SendMessageAsync(message.Author.Id.ToUserMention() + locale["image_only_channel"]);
                        await Task.Delay(TimeSpan.FromSeconds(Program.Random.Next(5)));
                    }
                    catch
                    {
                        var msg = await message.Channel.SendMessageAsync(message.Author.Id.ToUserMention() + locale["image_only_channel"]);
                        await Task.Delay(TimeSpan.FromSeconds(Program.Random.Next(10)));
                        await msg.DeleteAsync();
                    }
                    try
                    {
                        await message.DeleteAsync();
                    }
                    catch
                    {

                    }
                    return true;
                }

            }
            return false;
        }
        public static async Task CheckImageOnlyChannels(IDatabaseContext _db, DiscordSocketClient _client, LocaleService _localeService)
        {
            var guilds = await _db.GetGuildsByMessageOnlyChannelsAsync();
            foreach (var g in guilds)
            {
                bool updateGuild = false;
                try
                {
                    if (!g.IsModerationEnabled) continue;

                    var guild = _client.GetGuild(g.Id);
                    var locale = _localeService[guild.PreferredLocale] ?? _localeService["en-US"];

                    foreach (var c in g.OnlyImageChannels)
                    {

                        try
                        {
                            var channel = guild.GetTextChannel(c);
                            if (channel == null)
                            {
                                g.OnlyImageChannels.Remove(c);
                                updateGuild = true;
                                continue;
                            };
                            if (channel.SlowModeInterval < Program.ImageOnlyChannelsSlowMode)
                            {
                                g.OnlyImageChannels.Remove(c);
                                updateGuild = true;
                                continue;
                            }

                            var messages = await channel.GetMessagesAsync(100).FlattenAsync();

                            do
                            {
                                foreach (var msg in messages)
                                {
                                    if (!msg.HasAnyImage())
                                    {
                                        await msg.DeleteAsync().Try();
                                        await Task.Delay(TimeSpan.FromMilliseconds(104));
                                    }
                                }
                                var latestMessage = messages.LastOrDefault();

                                if (latestMessage != null)
                                {
                                    if (latestMessage.Timestamp > DateTime.UtcNow.AddDays(-6))
                                        messages = await channel.GetMessagesAsync(latestMessage, Direction.Before, 100).FlattenAsync();
                                    else messages = null;
                                }
                            } while (messages.Any());

                        }
                        catch { }

                    }
                }
                catch { }

                if (updateGuild) await _db.UpdateAsync(g);
            }

        }
    }
}
