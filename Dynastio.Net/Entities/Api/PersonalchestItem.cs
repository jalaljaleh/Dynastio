using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Net
{
    public class PersonalChestItem
    {
        public int index { get; set; }
        public ItemType ItemType { get; set; }
        public int Count { get; set; }
        public int Durablity { get; set; }
        public string OwnerID { get; set; }

        public string Token { get; set; }
        public string Details { get; set; }

    }
}
