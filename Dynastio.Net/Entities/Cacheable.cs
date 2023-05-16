using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Net.Entities
{
    public struct Cacheable<T>
    {
        private Func<Task<T>> _updateFunc;
        private int _time = 1000;
        public Cacheable(int millisecondsUpdateTime, Func<Task<T>> updateFunc)
        {
            _updateFunc = updateFunc;
            _time = millisecondsUpdateTime;
        }

        private DateTime _valueTime = DateTime.MinValue;

        private T _value = default;
        public T Value
        {
            get
            {
                if ((DateTime.UtcNow - _valueTime).TotalMilliseconds > _time || _value is null)
                {
                    _value = _updateFunc().GetAwaiter().GetResult();
                    _valueTime = DateTime.UtcNow;
                }
                return _value;
            }
        }


    }
}
