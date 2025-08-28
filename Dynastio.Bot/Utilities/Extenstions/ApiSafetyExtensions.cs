using DnsClient;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    /// <summary>
    /// Safety and convenience extensions for calling DynastioApi without leaking exceptions
    /// into the calling code. Includes sync and async variants, retries, and small enumerable helpers.
    /// </summary>
    internal static class ApiSafetyExtensions
    {
        /// <summary>
        /// Executes the action with the api instance and returns default(T) if it throws.
        /// You can optionally log the exception via onError.
        /// </summary>
        public static T DefaultIfError<T>(
            this DynastioApi api,
            Func<DynastioApi, T> action,
            Action<Exception> onError = null)
        {
            try
            {
                return action(api);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                return default;
            }
        }

        /// <summary>
        /// Executes the action and, if it throws, returns the provided fallback instead of default(T).
        /// Use when you prefer an explicit fallback value over default(T).
        /// </summary>
        public static T DefaultIfError<T>(
            this DynastioApi api,
            Func<DynastioApi, T> action,
            T fallback,
            Action<Exception> onError = null)
        {
            try
            {
                return action(api);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                return fallback;
            }
        }

        /// <summary>
        /// Executes the action and returns null on error for value types by using a nullable return.
        /// Helpful when you need to distinguish "error" (null) vs "legitimate zero" values.
        /// </summary>
        public static T? DefaultIfErrorNullable<T>(
            this DynastioApi api,
            Func<DynastioApi, T> action,
            Action<Exception> onError = null)
            where T : struct
        {
            try
            {
                return action(api);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                return null;
            }
        }

        /// <summary>
        /// Async variant that returns default(T) on error. OperationCanceledException is not swallowed.
        /// </summary>
        public static async Task<T> DefaultIfErrorAsync<T>(
            this DynastioApi api,
            Func<DynastioApi, Task<T>> action,
            Action<Exception> onError = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                return await action(api).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw; // propagate cancellations
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                return default;
            }
        }

        /// <summary>
        /// Retries an async action on failure up to maxAttempts with optional backoff and per-attempt logging.
        /// Throws if the final attempt fails or the operation is canceled.
        /// </summary>
        public static async Task<T> RetryIfErrorAsync<T>(
            this DynastioApi api,
            Func<DynastioApi, Task<T>> action,
            int maxAttempts = 3,
            Func<int, TimeSpan> delayProvider = null,
            Action<Exception, int> onRetry = null,
            CancellationToken cancellationToken = default)
        {
            if (maxAttempts <= 0) throw new ArgumentOutOfRangeException(nameof(maxAttempts));

            delayProvider ??= attempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt - 1)); // 200ms, 400ms, 800ms...

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return await action(api).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex) when (attempt < maxAttempts)
                {
                    onRetry?.Invoke(ex, attempt);
                    var delay = delayProvider(attempt);
                    if (delay > TimeSpan.Zero)
                    {
                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            // Last attempt: let the exception bubble so callers can decide what to do.
            return await action(api).ConfigureAwait(false);
        }

        /// <summary>
        /// Applies a timeout to an async API call. Returns fallback on timeout and optionally signals onTimeout.
        /// </summary>
        public static async Task<T> WithTimeoutAsync<T>(
            this DynastioApi api,
            Func<DynastioApi, Task<T>> action,
            TimeSpan timeout,
            T fallback = default,
            Action onTimeout = null,
            CancellationToken cancellationToken = default)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var timeoutTask = Task.Delay(timeout, cts.Token);
            var workTask = action(api);

            var completed = await Task.WhenAny(workTask, timeoutTask).ConfigureAwait(false);
            if (completed == workTask)
            {
                cts.Cancel(); // cancel the timeout task
                return await workTask.ConfigureAwait(false);
            }

            onTimeout?.Invoke();
            return fallback;
        }

        /// <summary>
        /// Returns an empty sequence if the source is null. Useful to avoid null checks before LINQ.
        /// </summary>
        public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> source) => source ?? Enumerable.Empty<T>();

        /// <summary>
        /// Safely counts an enumerable, returning 0 if null or if Count() throws (e.g., deferred query failure).
        /// </summary>
        public static int SafeCount<T>(this IEnumerable<T> source)
        {
            try { return source?.Count() ?? 0; }
            catch { return 0; }
        }
    }
}

                    // ------------------ Examples -----------------------
                    // 
                    // Common sync cases
                    //           // Return default(int) = 0 when the call fails
                      //int playerCount = _api.DefaultIfError(a => a.OnlinePlayers.Count);

                                /// Return a specific fallback (e.g., -1) when the call fails
                      //int playerCountOrMinusOne = _api.DefaultIfError(a => a.OnlinePlayers.Count, fallback: -1);

                                /// Return null on error so you can distinguish "error" from "0 players"
                      //int? playerCountOrNull = _api.DefaultIfErrorNullable(a => a.OnlinePlayers.Count);

                                /// Get the list safely; if it fails, get an empty list
                      //var players = _api.DefaultIfError(a => a.OnlinePlayers, fallback: new List<Player>());

                                /// Combine with OrEmpty and SafeCount for clean pipelines
                      //int safeCount = _api.DefaultIfError(a => a.OnlinePlayers, fallback: null)
                      //                  .OrEmpty()
                      //                  .SafeCount();


                                // Logging on error
                      //int count = _api.DefaultIfError(a => a.OnlinePlayers.Count, onError: ex =>
                      //{
                      //    Console.WriteLine($"[Dynastio] Failed to read OnlinePlayers.Count: {ex.Message}");
                      //});

                          //Async usage
                          //csharp
                                    /// If you have an async API method
                          //var players = await _api.DefaultIfErrorAsync(
                          //    a => a.GetOnlinePlayersAsync(),
                          //    onError: ex => Console.WriteLine($"Async fetch failed: {ex.Message}")
                          //);

                                    /// With a fallback on error
                          //var playersOrEmpty = await _api.DefaultIfErrorAsync(
                          //    a => a.GetOnlinePlayersAsync(),
                          //    onError: null
                          //) ?? new List<Player>();
                          //Retry with exponential backoff
                          //csharp
                          //var players = await _api.RetryIfErrorAsync(
                          //    a => a.GetOnlinePlayersAsync(),
                          //    maxAttempts: 4,
                          //    delayProvider: attempt => TimeSpan.FromMilliseconds(250 * Math.Pow(2, attempt - 1)),
                          //    onRetry: (ex, attempt) => Console.WriteLine($"Attempt {attempt} failed: {ex.Message}")
                          //);
                          //Apply a timeout to an async call
                          //csharp
                          //var playersWithTimeout = await _api.WithTimeoutAsync(
                          //    a => a.GetOnlinePlayersAsync(),
                          //    timeout: TimeSpan.FromSeconds(2),
                          //    fallback: new List<Player>(),
                          //    onTimeout: () => Console.WriteLine("Players call timed out after 2s")
                          //);
                          //Safe enumeration and chaining
                          //csharp
                                    /// Safely enumerate even if _api.OnlinePlayers is null or throws during enumeration
                          //foreach (var p in _api.DefaultIfError(a => a.OnlinePlayers, fallback: null).OrEmpty())
                          //{
                          //    Console.WriteLine(p.Name);
                          //}