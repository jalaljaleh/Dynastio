using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dynastio.Net.Entities
{
    internal class Cacheable<T>
    {
        private Func<Task<T>> _updateFunc;
        private TimeSpan _time;
        public Cacheable(TimeSpan millisecondsUpdateTime, Func<Task<T>> updateFunc)
        {
            _updateFunc = updateFunc;
            _time = millisecondsUpdateTime;
            _valueTime = DateTime.MinValue;
        }
        private DateTime _valueTime;
        private T _value;
        public T Value
        {
            get
            {
                if ((DateTime.UtcNow - _valueTime).TotalMilliseconds > _time.TotalMilliseconds || _value is null)
                {
                    _value = _updateFunc().GetAwaiter().GetResult();
                    _valueTime = DateTime.UtcNow;
                }
                return _value;
            }
        }


    }

}




