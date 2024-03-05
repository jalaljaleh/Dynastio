using Discord;
using Dynastio.Bot.Database;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Services
{
    public class AdvertisingService : ServicesBase
    {
        public RepeaterService repeaterService { get; set; }

        private List<Advertise> _advertising;
        private ConcurrentStack<Advertise> _toUpdate = new();
        public AdvertisingService(IServiceProvider services) : base(services)
        {
            repeaterService = services.GetRequiredService<RepeaterService>();
            _advertising = new();
        }
        public async Task InitializeAsync()
        {
            _advertising = await _db.GetAvailableAdsAsync();
            repeaterService.AddAction(RefreshRecords, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }
        async Task RefreshRecords()
        {
            if (_toUpdate.Count == 0) return;

            var list = _toUpdate.ToList();
            _toUpdate.Clear();

            await _db.UpdateManyAsync(list);
        }
        public async Task<bool> InsertAndCache(Advertise advertise)
        {
            await _db.InsertAsync(advertise);
            _advertising.Add(advertise);
            return true;
        }

        public string GetInlineEmbedDescription(int size = 4)
        {
            var embedBottomAdvertises = ExploitationAdvertising(Database.AdsType.InlineEmbedDescription, size);

            string text = string.Join("  ", embedBottomAdvertises?.Select(a => $" [{a.Label}]({a.Url}) "));
            return text;
        }

        public List<Advertise> ExploitationAdvertising(AdsType type, int take)
        {
            var res = _advertising.Where(a => a.Type == type).OrderBy(a => a.DisplayCount).Take(take);
            foreach (var advertise in res)
            {
                advertise.DisplayCount++;

                if (advertise.DisplayCount >= advertise.Count)
                {
                    advertise.FinishedAt = DateTime.UtcNow;

                    _advertising.Remove(advertise);

                    _db.UpdateAsync(advertise).GetAwaiter().GetResult();

                    continue;
                }
                _toUpdate.Push(advertise);
            }
            return res.ToList();
        }
        public Advertise ExploitationAdvertise(AdsType type)
        {
            return ExploitationAdvertising(type, 1).FirstOrDefault();
        }
    }
}
