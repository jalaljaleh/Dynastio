/*!
 * Task Extension v1 (https://jalaljaleh.github.io/)
 * Copyright 2021-2022 Jalal Jaleh
 * Licensed under Apache (https://github.com/jalaljaleh/Dynastio.Discord/blob/master/LICENSE.txt)
 * Original (https://github.com/jalaljaleh/Dynastio.Discord/blob/master/Dynastio.Bot/Extensions/TaskExtensions.cs)
 */



namespace Dynastio.Bot
{
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    public static class TaskExtensions
    {
        public static void RunInBackground(this Task task, bool tryCatch = false)
        {
            _ = Task.Run(async () =>
            {
                if (tryCatch)
                    await task.Try();
                else await task;
            });
        }
        public static async Task<T> TryGet<T>(this Task<T> task)
        {
            try
            {
                return await task;
            }
            catch
            {
                return default(T);
            }
        }
        public static async Task<bool> Try(this Task task)
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
    }
}
