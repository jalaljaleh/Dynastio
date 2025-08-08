using Dynastio.Bot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    /// <summary>
    /// Provides factory methods for creating <see cref="User"/> instances.
    /// </summary>
    internal static class UserFactory
    {
        /// <summary>
        /// Creates a default <see cref="User"/> instance with empty collections.
        /// </summary>
        /// <param name="id">The unique identifier for the user.</param>
        /// <returns>A new <see cref="User"/> object with default values.</returns>
        public static User CreateDefault(ulong id)
        {
            return new User
            {
                Id = id,
                Accounts = new(),
                GuildProfiles = new()
            };
        }
    }

}
