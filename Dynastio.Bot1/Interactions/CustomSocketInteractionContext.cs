using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot;
using Dynastio.Data;
using Dynastio.Net;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.Interactions
{

    //
    // Summary:
    //     Represents a Web-Socket based context of an Discord.IDiscordInteraction
    public class CustomSocketInteractionContext : CustomSocketInteractionContext<SocketInteraction>
    {
        //
        // Summary:
        //     Initializes a new Discord.Interactions.SocketInteractionContext
        //
        // Parameters:
        //   client:
        //     The underlying client
        //
        //   interaction:
        //     The underlying interaction
        public CustomSocketInteractionContext(DiscordSocketClient client, SocketInteraction interaction, User user, Locale Locale, Guild BotGuild)
            : base(client, interaction, user, Locale, BotGuild)
        {
        }
    }
    //
    // Summary:
    //     Represents the context of an Interaction.
    public interface ICustomInteractionContext : IInteractionContext
    {
        Guild BotGuild { get; }
        User BotUser { get; }
        Locale Locale { get; }
        SocketInteraction OverridenInteraction { get; set; }
    }
    //
    // Summary:
    //     Represents a Web-Socket based context of an Discord.IDiscordInteraction.
    public class CustomSocketInteractionContext<TInteraction> : ICustomInteractionContext, IRouteMatchContainer where TInteraction : SocketInteraction
    {
        public Guild BotGuild { get; }

        public User BotUser { get; }
        //
        // Summary:
        //     Gets the Discord.WebSocket.DiscordSocketClient that the command will be executed
        //     with.
        public DiscordSocketClient Client { get; }
        public SocketInteraction OverridenInteraction { get; set; }
        //
        // Summary:
        //     Gets the Discord.WebSocket.SocketGuild the command originated from.
        //
        // Remarks:
        //     Will be null if the command is from a DM Channel.
        public SocketGuild Guild { get; }

        //
        // Summary:
        //     Gets the Discord.WebSocket.ISocketMessageChannel the command originated from.
        public ISocketMessageChannel Channel { get; }

        //
        // Summary:
        //     Gets the Discord.WebSocket.SocketUser who executed the command.
        public SocketUser User { get; }

        //
        // Summary:
        //     Gets the Discord.WebSocket.SocketInteraction the command was recieved with.
        public TInteraction Interaction { get; }

        public IReadOnlyCollection<IRouteSegmentMatch> SegmentMatches { get; private set; }

        IEnumerable<IRouteSegmentMatch> IRouteMatchContainer.SegmentMatches => SegmentMatches;

        IDiscordClient IInteractionContext.Client => Client;

        IGuild IInteractionContext.Guild => Guild;

        IMessageChannel IInteractionContext.Channel => Channel;

        IUser IInteractionContext.User => User;

        IDiscordInteraction IInteractionContext.Interaction => Interaction;

        public Locale Locale { get; }

        //
        // Summary:
        //     Initializes a new Discord.Interactions.SocketInteractionContext`1.
        //
        // Parameters:
        //   client:
        //     The underlying client.
        //
        //   interaction:
        //     The underlying interaction.
        public CustomSocketInteractionContext(DiscordSocketClient client, TInteraction interaction, User botUser, Locale locale, Guild botGuild)
        {
            Client = client;
            Channel = interaction.Channel;
            Guild = (interaction.User as SocketGuildUser)?.Guild;
            User = interaction.User;
            Interaction = interaction;
            BotUser = botUser;
            Locale = locale;
            BotGuild = botGuild;
        }

        public void SetSegmentMatches(IEnumerable<IRouteSegmentMatch> segmentMatches)
        {
            SegmentMatches = segmentMatches.ToImmutableArray();
        }
    }

}