namespace MinimalLock
{
    /// <summary>
    /// MutexSet wrapper for only 1 resource
    /// </summary>
    public class MutexSingle
    {
        /// <inheritdoc cref="MutexSet{TResourceID}.PollingIntervalMs"/>
        public int PollingIntervalMs
        {
            get => _singleMutexSet.PollingIntervalMs;
            set => _singleMutexSet.PollingIntervalMs = value;
        }

        /// <inheritdoc cref="MutexSet{TResourceID}.DefaultWaitTimeoutMs"/>
        public int DefaultWaitTimeoutMs
        {
            get => _singleMutexSet.DefaultWaitTimeoutMs;
            set => _singleMutexSet.DefaultWaitTimeoutMs = value;
        }


        private const char KEY = '0';
        private MutexSet<char> _singleMutexSet { get; } = new();


        /// <summary>
        /// <inheritdoc cref="MutexSet{TResourceID}.Wait(TResourceID, CancellationTokenSource?)"/>
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns>time waited in ms</returns>
        public async Task<double> Wait(CancellationTokenSource? cancellationTokenSource = null)
        {
            return await _singleMutexSet.Wait(KEY, cancellationTokenSource);
        }

        /// <summary>
        /// <inheritdoc cref="MutexSet{TResourceID}.WaitShortly(TResourceID, CancellationTokenSource?)"/>
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public double WaitShortly(
            CancellationTokenSource? cancellationTokenSource = null)
        {
            return _singleMutexSet.WaitShortly(KEY, cancellationTokenSource);
        }

        /// <summary>
        /// <inheritdoc cref="MutexSet{TResourceID}.IsLocked(TResourceID)"/>
        /// </summary>
        /// <returns></returns>
        public bool IsLocked()
        {
            return _singleMutexSet.IsLocked(KEY);
        }

        /// <summary>
        /// <inheritdoc cref="MutexSet{TResourceID}.TryAcquire(TResourceID)"/>
        /// </summary>
        /// <returns></returns>
        public bool TryAcquire()
        {
            return _singleMutexSet.TryAcquire(KEY);
        }

        /// <summary>
        /// <inheritdoc cref="MutexSet{TResourceID}.TryAcquireAfterWait(TResourceID, CancellationTokenSource?)"/>
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<bool> TryAcquireAfterWait(
            CancellationTokenSource? cancellationTokenSource = null)
        {
            return await _singleMutexSet.TryAcquireAfterWait(KEY, cancellationTokenSource);
        }

        /// <summary>
        /// <inheritdoc cref="MutexSet{TResourceID}.TryAcquireAfterShortWait(TResourceID, CancellationTokenSource?)"/>
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public bool TryAcquireAfterShortWait(
            CancellationTokenSource? cancellationTokenSource = null)
        {
            return _singleMutexSet.TryAcquireAfterShortWait(KEY, cancellationTokenSource);
        }

        /// <summary>
        /// <inheritdoc cref="MutexSet{TResourceID}.Release(TResourceID)"/>
        /// </summary>
        public void Release()
        {
            _singleMutexSet.Release(KEY);
        }
    }
}
