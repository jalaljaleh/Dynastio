/*!
 * Discord Template By (https://jalaljaleh.github.io/)
 * Copyright 2021-2022 Jalal Jaleh
 * Licensed under MIT (https://github.com/jalaljaleh/Template.Discord.Bot/blob/master/LICENSE.txt)
 * Project Url (https://github.com/jalaljaleh/Template.Discord.Bot/)
 */
namespace Dynastio.Bot.Interactions
{
    using Discord;
    using Discord.Interactions;

    public class CustomInteractionModuleBase<T> : InteractionModuleBase<T> where T : class, IInteractionContext
    {
       
        public CustomInteractionModuleBase() : base()
        {
            
        }

    }
}
