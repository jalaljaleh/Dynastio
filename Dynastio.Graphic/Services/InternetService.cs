using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Graphic
{
    public class InternetService
    {
        HttpClient _client;
        public InternetService()
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
            {
                return true;
            };

            _client = new HttpClient(clientHandler);
            _client.DefaultRequestHeaders.Add("application", "Dynastio.Graphic");
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        public async Task<Image> GetImageAsync(string url)
        {
            Image img = null;
            using (WebClient webClient = new WebClient())
            {
                byte[] data = webClient.DownloadData(url);

                using (MemoryStream mem = new MemoryStream(data))
                {
                    img = Image.Load(mem);

                    mem.Close();
                }
            }
            return img;
        }
        public async Task<string> GetAsync(string api)
        {
            string result;
            using (var request = new HttpRequestMessage(HttpMethod.Get, api))
            {
                var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                result = await response.Content.ReadAsStringAsync();
            }
            return result;
        }
    }
}
