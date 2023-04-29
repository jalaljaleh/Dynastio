using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynastio.Data;
namespace Dynastio.Bot
{
    internal class AutocompleteUtilities
    {
        public static List<AutocompleteResult> Parse(List<UserAccount> accounts)
        {
            var result = new List<AutocompleteResult>();
            foreach (var account in accounts)
            {
                result.Add(new AutocompleteResult()
                {
                    Name = account.Nickname,
                    Value = account.GetHashCode().ToString()
                });
            }
            return result;
        }
    }
}
