namespace MinimalLock
{
    /// <summary>
    /// 1. Read doesn't block reads<br></br>
    /// 2. Read blocks writes<br></br>
    /// 3. Write blocks reads(while waiting ongoing reads)<br></br>
    /// 4. Write blocks writes<br></br>
    /// </summary>
    /// <typeparam name="TResourceID">resource id</typeparam>
    public class RwLockSet<TResourceID> : Internals.PollingCommons
        where TResourceID : notnull
    {
        /// <inheritdoc cref="SemaphoreSet{TResourceID}.MaxAllowed"/>
        public int MaxReads
        {
            get => _reads.MaxAllowed;
            set => _reads.MaxAllowed = value;
        }

        public override int PollingIntervalMs
        {
            get => _reads.PollingIntervalMs;
            set { _reads.PollingIntervalMs = value; _writes.PollingIntervalMs = value; }
        }

        public override int DefaultWaitTimeoutMs
        {
            get => _reads.DefaultWaitTimeoutMs;
            set { _reads.DefaultWaitTimeoutMs = value; _writes.DefaultWaitTimeoutMs = value; }
        }

        private SemaphoreSet<TResourceID> _reads { get; } = new();
        private MutexSet<TResourceID> _writes { get; } = new();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<bool> TryAcquireReadingAfterWait(
            TResourceID id,
            CancellationTokenSource? cancellationTokenSource = null)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));

            if (_writes.IsLocked(id))
            {
                return false;
            }
            return await _reads.TryAcquireAfterWait(id, cancellationTokenSource);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool TryAcquireReading(TResourceID id)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));

            if (_writes.IsLocked(id))
            {
                return false;
            }
            return _reads.TryAcquire(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool TryReleaseReading(TResourceID id)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));

            return _reads.TryRelease(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<bool> TryAcquireWritingAfterWait(
            TResourceID id,
            CancellationTokenSource? cancellationTokenSource = null)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));

            DateTime startTime = DateTime.Now;

            bool writingAcquired = await _writes.TryAcquireAfterWait(id, cancellationTokenSource);

            while (_reads.IsUsed(id))
            {
                if (IsTimeout(startTime))
                {
                    _writes.TryRelease(id);
                    return false;
                }
                await Task.Delay(PollingIntervalMs);
            }

            return writingAcquired;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool TryAcquireWriting(TResourceID id)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));
            return !_reads.IsUsed(id)
                    && _writes.TryAcquire(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public bool TryReleaseWriting(TResourceID id)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));
            return _writes.TryRelease(id);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ForceReleaseReads()
        {
            _reads.ForceReleaseAll();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ForceReleaseWrites()
        {
            _writes.ForceReleaseAll();
        }

        public readonly struct RwResources
        {
            public readonly TResourceID ID;
            public readonly int Readings;
            public readonly int Writings;
        }
    }
}
