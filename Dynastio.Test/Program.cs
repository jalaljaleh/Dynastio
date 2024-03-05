using Dynastio.Net;
using System.Net.Http.Headers;

namespace Dynastio.Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var dynastio = new DynastioApi("");

            var p = dynastio.GetUserProfileCardAsync("google:109997366771820676430").GetAwaiter().GetResult();


        }

    }
   
}
