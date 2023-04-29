using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Net
{
    public class Personalchest
    {
        public Personalchest(List<PersonalChestItem> items)
        {
            Items = items;
        }
        public List<PersonalChestItem> Items { get; set; } = new List<PersonalChestItem>();
        public Dictionary<int, PersonalChestItem> GetAsDictionary()
        {
            var chestItems = new Dictionary<int, PersonalChestItem>();
            foreach (var item in Items)
            {
                chestItems.Add(item.index, item);
            }
            return chestItems;
        }
    }
}
