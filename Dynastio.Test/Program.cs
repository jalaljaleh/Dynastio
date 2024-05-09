using Dynastio.Net;
using System.Net.Http.Headers;

namespace Dynastio.Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var dynastio = new DynastioApi("X");

            var result = dynastio.GetUserProfileAsync("discord:805534924622004274").GetAwaiter().GetResult();


        }

    }

}
