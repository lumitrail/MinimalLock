namespace MinimalLock
{
    /// <summary>
    /// Controlls multiple semaphores with resource ID.
    /// </summary>
    /// <typeparam name="TResourceID"></typeparam>
    public class SemaphoreSet<TResourceID> : Internals.PollingCommons
        where TResourceID : notnull
    {
        /// <summary>
        /// </summary>
        public int MaxAllowed
        {
            get => _maxAllowed;
            set => _maxAllowed = Math.Max(1, value);
        }
        private int _maxAllowed;


        private System.Collections.Concurrent
            .ConcurrentDictionary<TResourceID, SemaphoreElem> _insiders { get; } = new();


        /// <summary>
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
                while (_insiders.TryGetValue(id, out SemaphoreElem? v))
                {
                    if (v.Count < MaxAllowed
                        || IsTimeout(startTime))
                    {
                        break;
                    }
                    await Task.Delay(PollingIntervalMs);
                }
            }
            else
            {
                while (_insiders.TryGetValue(id, out SemaphoreElem? v))
                {
                    if (v.Count < MaxAllowed
                        || cancellationTokenSource.IsCancellationRequested
                        || IsTimeout(startTime))
                    {
                        break;
                    }
                    await Task.Delay(PollingIntervalMs);
                }
            }

            return GetElapsedTimeMs(startTime);
        }

        /// <summary>
        /// ignoring polling interval, spin wait.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public double WaitShortly(
            TResourceID id,
            CancellationTokenSource? cancellationTokenSource = null)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));

            var s = new SpinWait();

            DateTime startTime = DateTime.Now;

            if (cancellationTokenSource == null)
            {
                while (_insiders.TryGetValue(id, out SemaphoreElem? v))
                {
                    if (v.Count < MaxAllowed
                        || IsTimeout(startTime))
                    {
                        break;
                    }
                    s.SpinOnce();
                }
            }
            else
            {
                while (_insiders.TryGetValue(id, out SemaphoreElem? v))
                {
                    if (v.Count < MaxAllowed
                        || cancellationTokenSource.IsCancellationRequested
                        || IsTimeout(startTime))
                    {
                        break;
                    }
                    s.SpinOnce();
                }
            }

            return GetElapsedTimeMs(startTime);
        }

        /// <summary>
        /// Someone is accessing (1 or more).
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool IsUsed(TResourceID id)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));

            if (_insiders.TryGetValue(id, out SemaphoreElem? e)
                && e.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// MaxAllowed are accessing.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool IsFull(TResourceID id)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));
            if (_insiders.TryGetValue(id, out SemaphoreElem? v))
            {
                return v.Count >= MaxAllowed;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool TryAcquire(TResourceID id)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));

            if (_insiders.TryGetValue(id, out SemaphoreElem? v))
            {
                return v.TryUp(MaxAllowed);
            }
            else
            {
                return _insiders.TryAdd(id, new SemaphoreElem(1));
            }
        }

        /// <summary>
        /// When you are going to wait tens of milliseconds
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

            var e = new SemaphoreElem(1);

            if (cancellationTokenSource == null)
            {
                while (true)
                {
                    if (_insiders.TryAdd(id, e))
                    {
                        return true;
                    }
                    else if (_insiders.TryGetValue(id, out SemaphoreElem? v)
                        && v.TryUp(MaxAllowed))
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
                    if (_insiders.TryAdd(id, e))
                    {
                        return true;
                    }
                    else if (_insiders.TryGetValue(id, out SemaphoreElem? v)
                        && v.TryUp(MaxAllowed))
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
        }

        /// <summary>
        /// When you are going to wait few milliseconds
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool TryAcquireAfterShortWait(
            TResourceID id,
            CancellationTokenSource? cancellationTokenSource = null)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));

            var s = new SpinWait();
            var e = new SemaphoreElem(1);

            DateTime startTime = DateTime.Now;

            if (cancellationTokenSource == null)
            {
                while (true)
                {
                    if (_insiders.TryAdd(id, e))
                    {
                        return true;
                    }
                    else if (_insiders.TryGetValue(id, out SemaphoreElem? v)
                        && v.TryUp(MaxAllowed))
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
                    if (_insiders.TryAdd(id, e))
                    {
                        return true;
                    }
                    else if (_insiders.TryGetValue(id, out SemaphoreElem? v)
                        && v.TryUp(MaxAllowed))
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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public bool TryRelease(TResourceID id)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));
            if (_insiders.TryGetValue(id, out SemaphoreElem? v))
            {
                bool downOk = v.TryDown();

                if (v.Count == 0
                    && _insiders.TryRemove(id, out _))
                {
                    return true;
                }
                else if (downOk)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public SemaphoreValues[] GetUsedResources()
        {
            var resultList = new List<SemaphoreValues>(_insiders.Count);

            foreach (var kv in _insiders)
            {
                var e = new SemaphoreValues(kv.Key, kv.Value.Count);
                if (e.Count > 0)
                {
                    resultList.Add(e);
                }
            }

            return resultList.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ForceReleaseAll()
        {
            _insiders.Clear();
        }


        public readonly struct SemaphoreValues
        {
            public readonly TResourceID ID;
            public readonly int Count;

            public SemaphoreValues(TResourceID id, int count)
            {
                ID = id;
                Count = count;
            }
        }

        private class SemaphoreElem
        {
            public object LockObj { get; } = new();

            public int Count { get; private set; }


            public SemaphoreElem()
            {
                Count = 0;
            }

            public SemaphoreElem(int initCount)
            {
                Count = initCount;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="max"></param>
            /// <returns></returns>
            public bool TryUp(int max)
            {
                lock (LockObj)
                {
                    if (Count >= max)
                    {
                        return false;
                    }
                    else
                    {
                        ++Count;
                        return true;
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public bool TryDown()
            {
                lock (LockObj)
                {
                    if (Count <= 0)
                    {
                        return false;
                    }
                    else
                    {
                        --Count;
                        return true;
                    }
                }
            }
        }
    }
}
