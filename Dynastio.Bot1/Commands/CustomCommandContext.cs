using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Data;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{

    //
    // Summary:
    //     Represents a WebSocket-based context of a command. This may include the client,
    //     guild, channel, user, and message.


    public interface ICustomCommandContext : ICommandContext
    {
        Guild BotGuild { get; }
        User BotUser { get; }
        Locale Locale { get; }
    }


    public class CustomCommandContext : ICustomCommandContext
    {
        /// <inheritdoc/>
        public IDiscordClient Client { get; }
        /// <inheritdoc/>
        public IGuild Guild { get; }
        /// <inheritdoc/>
        public IMessageChannel Channel { get; }
        /// <inheritdoc/>
        public IUser User { get; }
        /// <inheritdoc/>
        public IUserMessage Message { get; }

        /// <summary> Indicates whether the channel that the command is executed in is a private channel. </summary>
        public bool IsPrivate => Channel is IPrivateChannel;

        public Guild BotGuild { get; }

        public User BotUser { get; }


        public Locale Locale { get; }

        /// <summary>
        ///     Initializes a new <see cref="CommandContext" /> class with the provided client and message.
        /// </summary>
        /// <param name="client">The underlying client.</param>
        /// <param name="msg">The underlying message.</param>
        public CustomCommandContext(IDiscordClient client, IUserMessage msg, Guild guild, User user,  Locale locale)
        {
            Client = client;
            Guild = (msg.Channel as IGuildChannel)?.Guild;
            Channel = msg.Channel;
            User = msg.Author;
            Message = msg;
            this.BotGuild = guild;
            this.BotUser = user;
            this.Locale = locale;
        }
    }
    public class CustomSocketCommandContext : ICustomCommandContext
    {
        //
        // Summary:
        //     Gets the Discord.WebSocket.DiscordSocketClient that the command is executed with.
        public DiscordSocketClient Client { get; }

        //
        // Summary:
        //     Gets the Discord.WebSocket.SocketGuild that the command is executed in.
        public SocketGuild Guild { get; }

        //
        // Summary:
        //     Gets the Discord.WebSocket.ISocketMessageChannel that the command is executed
        //     in.
        public ISocketMessageChannel Channel { get; }

        //
        // Summary:
        //     Gets the Discord.WebSocket.SocketUser who executed the command.
        public SocketUser User { get; }

        //
        // Summary:
        //     Gets the Discord.WebSocket.SocketUserMessage that the command is interpreted
        //     from.
        public SocketUserMessage Message { get; }

        //
        // Summary:
        //     Indicates whether the channel that the command is executed in is a private channel.
        public bool IsPrivate => Channel is IPrivateChannel;

        IDiscordClient ICommandContext.Client => Client;

        IGuild ICommandContext.Guild => Guild;

        IMessageChannel ICommandContext.Channel => Channel;

        IUser ICommandContext.User => User;

        IUserMessage ICommandContext.Message => Message;

        public Guild BotGuild { get; }

        public User BotUser { get; }

        public Locale Locale { get; }

        //
        // Summary:
        //     Initializes a new Discord.Commands.SocketCommandContext class with the provided
        //     client and message.
        //
        // Parameters:
        //   client:
        //     The underlying client.
        //
        //   msg:
        //     The underlying message.
        public CustomSocketCommandContext(DiscordSocketClient client, SocketUserMessage msg, Guild guild, User user, Locale locale)
        {
            Client = client;
            Guild = (msg.Channel as SocketGuildChannel)?.Guild;
            Channel = msg.Channel;
            User = msg.Author;
            Message = msg;
            this.BotGuild = guild;
            this.BotUser = user;
            this.Locale = locale;
        }
    }
}