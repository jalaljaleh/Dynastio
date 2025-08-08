using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    namespace Dynastio.Bot
    {
        /// <summary>
        /// Provides safe execution wrappers for asynchronous tasks.
        /// </summary>
        public static class TaskExtensions
        {
            /// <summary>
            /// Executes a <see cref="Task"/> and returns whether it completed without throwing.
            /// </summary>
            /// <param name="task">The task to execute.</param>
            /// <returns>
            /// <c>true</c> if the task completed successfully; otherwise <c>false</c>.
            /// </returns>
            public static async Task<bool> TryAsync(this Task task)
            {
                if (task == null)
                    throw new ArgumentNullException(nameof(task));

                try
                {
                    await task.ConfigureAwait(false);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            /// <summary>
            /// Executes a <see cref="Task{TResult}"/> and returns a tuple indicating success and its result.
            /// </summary>
            /// <typeparam name="T">The type returned by the task.</typeparam>
            /// <param name="task">The task to execute.</param>
            /// <returns>
            /// A tuple where <c>isSuccessful</c> is <c>true</c> if no exception was thrown, 
            /// and <c>result</c> is the returned value or <c>default</c> on failure.
            /// </returns>
            public static async Task<(bool isSuccessful, T result)> TryAsync<T>(this Task<T> task)
            {
                if (task == null)
                    throw new ArgumentNullException(nameof(task));

                try
                {
                    var result = await task.ConfigureAwait(false);
                    return (true, result);
                }
                catch
                {
                    return (false, default!);
                }
            }
        }
    }
