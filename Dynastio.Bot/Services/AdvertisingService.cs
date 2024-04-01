using Discord;
using Dynastio.Bot.Database;
using Dynastio.Bot.Extenstions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
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
        private readonly RepeaterService repeaterService;

        private List<Advertise> _remainedAdvertising;
        private readonly ConcurrentStack<Advertise> _toUpdate = new();
        public AdvertisingService(IServiceProvider services) : base(services)
        {
            repeaterService = services.GetRequiredService<RepeaterService>();
            _remainedAdvertising = new();
        }
        public async Task InitializeAsync()
        {
            _remainedAdvertising = await _db.GetAvailableAdsAsync();
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
            _remainedAdvertising.Add(advertise);
            return true;
        }

        public async Task DeleteAdvertise(Advertise advertise)
        {
            _remainedAdvertising.Remove(advertise);
            await _db.DeleteAsync(advertise);
        }
        public List<Advertise> GetRemainingAdvertises()
        {
            return _remainedAdvertising;
        }
        public ComponentBuilder ExploitationAdvertisingButtons(ComponentBuilder cBuilder,int row = 0, int count= 4)
        {
            var advertises = ExploitationAdvertising(Database.AdsType.Buttons, count);
            advertises.ForEach(
                a => 
                cBuilder.WithButton(a.ToButtonBuilder(), row)
            );
            return cBuilder;
        }
        public List<Advertise> ExploitationAdvertising(AdsType type, int take)
        {
            var res = _remainedAdvertising.Where(a => a.Type == type).OrderBy(a => a.DisplayCount).Take(take);
            foreach (var advertise in res)
            {
                advertise.DisplayCount++;

                if (advertise.DisplayCount >= advertise.Count)
                {
                    advertise.FinishedAt = DateTime.UtcNow;

                    _remainedAdvertising.Remove(advertise);

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
        public string GetInlineEmbedDescription(int size = 4)
        {
            var embedBottomAdvertises = ExploitationAdvertising(Database.AdsType.InlineEmbedDescription, size);

            string text = string.Join(" ‌ ‌ ‌ ‌", embedBottomAdvertises?.Select(a => a.GetEmbedLink()));
            return " " + text + " ";
        }


    }
}
