// Required namespaces for Discord.NET entities, interaction attributes, and async programming
using Discord;
using Discord.Interactions;
using Dynastio.Bot.Interactions;
using Dynastio.Bot.Interactions.Modules.Menu.Buttons;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    /// <summary>
    /// Defines the contract for any "button module" in the Dynastio bot.
    /// A button module knows:
    /// - what kind of button it represents
    /// - how to build the actual button object
    /// - how to handle the click/interaction event
    /// </summary>
    public interface IMenuComponentRule
    {


        /// <summary>
        /// A constant prefix used for all button interaction IDs in this bot.
        /// Helps group and namespace IDs to avoid collisions.
        /// </summary>
        public const string InteractionIdBase = "interactions.";

        /// <summary>
        /// Additional formatting for the ID (empty here by default,
        /// but can be replaced in an implementation).
        /// </summary>
        public const string IdParameterFormat = "";

        /// <summary>
        /// Builds the full CustomId string by joining the base and parameter format,
        /// and applying a "StarIfNullFormat()" extension to ensure a safe format string.
        /// This ID will be used to identify the button in the interaction handler.
        /// </summary>
        public static string BuildCustomId() => InteractionIdBase + IdParameterFormat.StarIfNullFormat();


    }
}
