using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Extenstions
{
    public static class TaskExtenstions
    {
        public static async Task<bool> TryAsync(this Task task)
        {
            try
            {
                await task;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static async Task<(bool isSuccesful, T result)> TryAsync<T>(this Task<T> task)
        {
            try
            {
                var res = await task;
                return (true, res);
            }
            catch
            {
                return (false, default(T));
            }
        }
    }
}
