using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Database
{
    public interface IGuildRoleModuleBase
    {
        public bool IsEnabled { get; set; }
        public ulong HeaderId { get; set; }
    }

}
