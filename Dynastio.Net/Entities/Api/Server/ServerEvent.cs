using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Net
{
    public class ServerEvent
    {
        public string id { get; set; }
        public string header { get; set; }
        public string description { get; set; }
        public DateTime start_time { get; set; }
        public DateTime finish_time { get; set; }
        public Kind kind { get; set; }
    }
    public class Kind
    {
        public string type { get; set; }
        public uint coef { get; set; }
    }
}
