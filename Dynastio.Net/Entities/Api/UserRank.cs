using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Net
{
    public class UserRank
    {
        public UserRank(List<int> data)
        {
            _data = data;
        }
        private readonly List<int> _data;
        public int Daily { get => CheckValue(_data.FirstOrDefault()); }
        public int Weekly { get => CheckValue(_data.Skip(1).FirstOrDefault()); }
        public int Monthly { get => CheckValue(_data.LastOrDefault()); }
        int CheckValue(int value) => value < 0 ? 99999 : value;
    }
}
