using Dynastio.Net;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Dynastio.Data;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Discord.WebSocket;
using Discord;

namespace Dynastio.Bot
{
    public class UserService
    {
        private readonly DynastioClient _dynastioClient;
        private readonly GuildService _guildService;
        private readonly WebhookService _webhook;
        private readonly DiscordSocketClient _discord;
        private readonly DynastioData _dynastioData;
        private readonly IServiceProvider _services;
        public UserService(IServiceProvider services)
        {
            Global.Main.Log("User Service", "Start Async");

            this._dynastioClient = services.GetRequiredService<DynastioClient>();
            this._dynastioData = services.GetRequiredService<DynastioData>();
            this._services = services;
            this._guildService = services.GetRequiredService<GuildService>();
            this._webhook = services.GetRequiredService<WebhookService>();
            this._discord = services.GetRequiredService<DiscordSocketClient>();

            this._discord.UserJoined += _discord_UserJoined;
        }
        private async Task _discord_UserJoined(SocketGuildUser discordUser)
        {
            var botUser = await _dynastioData.GetUserAsync(discordUser.Id, false);
            if (botUser is not null)
            {
                var newRoles = await SyncRankedRoles(botUser.activiy_level, botUser.Id, discordUser);
            }
        }
        public async Task<IUserMessage> SendMessageAsync(ulong userId, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed[] embeds = null)
        {
            var user = await GetUserAsync(userId);
            return await user.SendMessageAsync(text, isTTS, embed, options, allowedMentions, components, embeds);
        }
        public async Task<IUser> GetUserAsync(ulong id)
        {
            return await _discord.GetUserAsync(id);
        }
        public IRole GetHighestRankedRoleUser(IGuildUser user = null)
        {
            return user.Guild.Roles
                .Where(a => a.Name.StartsWith("rank: "))
                .OrderBy(a=>a.Position)
                .FirstOrDefault();
        }
        public async Task<IGuildUser> GetGuildUserAsync(ulong id)
        {
            return await Task.FromResult(_discord.Guilds.FirstOrDefault().GetUser(id));
        }
        public async Task UserRankedUpAsync(User user, IGuildUser discordUser, ITextChannel channel)
        {
            var newRoles = await SyncRankedRoles(user.activiy_level, user.Id, discordUser);
            if (newRoles is null || newRoles.Count == 0)
                return;

            var latestDiscordRole = discordUser.Guild.Roles.FirstOrDefault(a => a.Id == newRoles.LastOrDefault());


            await channel.SendMessageAsync(user.Id.ToUserMention(),
                   embed: new EmbedBuilder()
                   {
                       Title = " You just got new rank 🎉",
                       Description = $"🎉 You just got new rank **{user.activiy_level}** exp: **{user.activiy_score}**",
                       Color = latestDiscordRole?.Color ?? Color.Orange,
                       Fields = new List<EmbedFieldBuilder>()
                                {
                                         new EmbedFieldBuilder()
                                         .WithName("Unlocked Roles")
                                         .WithValue(string.Join(", ", newRoles.Select(a=> $"<@&{a}>")))
                                         .WithIsInline(true)
                                },
                       ThumbnailUrl = latestDiscordRole.GetIconUrl() ?? ""
                   }.Build());
        }
        public async Task<List<ulong>> SyncRankedRoles(int currentLevel, ulong userId, IGuildUser discordUser = null)
        {
            if (discordUser is null)
            {
                discordUser = await GetGuildUserAsync(userId);

                if (discordUser is null)
                    return null;
            }

            var roles = discordUser.Guild.Roles
                .Where(x => x.Name.StartsWith("rank: "))
                .OrderBy(a => a.Position)
                .Select(a => a.Id)
                .ToList();

            var reached = discordUser.RoleIds.Where(a => roles.Contains(a));
            var toAdd = roles.GetRange(0, currentLevel);
            toAdd.RemoveRange(0, reached.Count());

            await discordUser.AddRolesAsync(toAdd);
            return toAdd;
        }



    }
}
