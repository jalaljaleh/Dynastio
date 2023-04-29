using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Net
{
    public class Version
    {
        [JsonProperty("version")]
        public string CurrentVersion { get; set; }
        [JsonProperty("url")]
        public string DownloadUrl { get; set; }
    }
}
