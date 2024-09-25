namespace MinimalLock
{
    /// <summary>
    /// Controlls multiple mutexes with resource ID.
    /// </summary>
    /// <typeparam name="TResourceID"></typeparam>
    public class MutexSet<TResourceID> : Internals.PollingCommons
        where TResourceID : notnull
    {
        private const char V = '\n';
        private System.Collections.Concurrent
            .ConcurrentDictionary<TResourceID, char> _tasksOngoing { get; } = new();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns>time waited in ms</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<double> Wait(
            TResourceID id,
            CancellationTokenSource? cancellationTokenSource = null)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));

            DateTime startTime = DateTime.Now;

            if (cancellationTokenSource == null)
            {
                while (_tasksOngoing.ContainsKey(id)
                    && !IsTimeout(startTime))
                {
                    await Task.Delay(PollingIntervalMs);
                }
            }
            else
            {
                while (_tasksOngoing.ContainsKey(id)
                    && !cancellationTokenSource.IsCancellationRequested
                    && !IsTimeout(startTime))
                {
                    await Task.Delay(PollingIntervalMs);
                }
            }

            return GetElapsedTimeMs(startTime);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool IsLocked(TResourceID id)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));
            return _tasksOngoing.ContainsKey(id);
        }

        /// <summary>
        /// 한번 얻어 보기
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool TryAcquire(TResourceID id)
        {
            ArgumentNullException.ThrowIfNull(id);
            return _tasksOngoing.TryAdd(id, V);
        }

        /// <summary>
        /// 기다려서 얻기
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<bool> TryAcquireAfterWait(
            TResourceID id,
            CancellationTokenSource? cancellationTokenSource = null)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));

            DateTime startTime = DateTime.Now;

            if (cancellationTokenSource == null)
            {
                while (true)
                {
                    if (_tasksOngoing.TryAdd(id, V))
                    {
                        return true;
                    }
                    else if (IsTimeout(startTime))
                    {
                        return false;
                    }
                    await Task.Delay(PollingIntervalMs);
                }
            }
            else
            {
                while (true)
                {
                    if (_tasksOngoing.TryAdd(id, V))
                    {
                        return true;
                    }
                    else if (cancellationTokenSource.IsCancellationRequested)
                    {
                        return false;
                    }
                    else if (IsTimeout(startTime))
                    {
                        return false;
                    }
                    await Task.Delay(PollingIntervalMs);
                }
            }
        }

        /// <summary>
        /// ignoring polling interval, spin wait.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool TryAcquireAfterBusyWait(
            TResourceID id,
            CancellationTokenSource? cancellationTokenSource = null)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));

            DateTime startTime = DateTime.Now;

            var s = new SpinWait();

            if (cancellationTokenSource == null)
            {
                while (true)
                {
                    if (_tasksOngoing.TryAdd(id, V))
                    {
                        return true;
                    }
                    else if (IsTimeout(startTime))
                    {
                        return false;
                    }
                    s.SpinOnce();
                }
            }
            else
            {
                while (true)
                {
                    if (_tasksOngoing.TryAdd(id, V))
                    {
                        return true;
                    }
                    else if (cancellationTokenSource.IsCancellationRequested)
                    {
                        return false;
                    }
                    else if (IsTimeout(startTime))
                    {
                        return false;
                    }
                    s.SpinOnce();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public void Release(TResourceID id)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));
            _tasksOngoing.Remove(id, out _);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TResourceID[] GetLockedResources()
        {
            return _tasksOngoing.Keys.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ForceReleaseAll()
        {
            _tasksOngoing.Clear();
        }
    }
}
