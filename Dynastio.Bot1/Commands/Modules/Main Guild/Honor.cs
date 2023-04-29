using Discord;
using Discord.Commands;
using System.IO;
using System.Threading.Tasks;


namespace Dynastio.Bot.Commands.Main_Guild
{
    public class Honor : ModuleBase<CustomSocketCommandContext>
    {
        public UserService UserService { get; set; }

        [RequireHonorTime]
        [Command("honor")]
        public async Task PingAsync()
        {
            int gift = 0;
            var time = DateTime.UtcNow;

            gift = Program.Random.Next(0, 5);
            time = time.AddHours(1);

            Context.BotUser.LastHonorGift = time;
            Context.BotUser.Honor += gift;
            await UserService.UpdateAsync(Context.BotUser);

            await Context.Channel.SendMessageAsync(Context.User.Id.ToUserMention(), embed: $"You got {gift} honor, your honor is {Context.BotUser.Honor}.".ToSuccessfulEmbed());
        }
    }
}
