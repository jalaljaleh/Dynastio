
namespace Dynastio.Bot
{
    using Discord.WebSocket;

    public interface IDiscordEvent
    {
        void Register(DiscordSocketClient client);
        void Unregister(DiscordSocketClient client);
    }
}