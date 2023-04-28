
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Net
{
    internal class DataType<T>
    {
        [JsonProperty("error")] public bool Error { get; set; } = false;
        [JsonProperty("errorMessage")] public string ErrorMessage { get; set; } = null;
        [JsonProperty("code")] public int Code { get; set; } = 0;
        [JsonProperty("data")] public T data;
        [JsonProperty("status")] public string status { get; set; } = null;
        [JsonProperty("servers")] public virtual List<Server> Servers { get; set; } = null;


    }
}
