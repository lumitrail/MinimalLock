namespace MinimalLock
{
    /// <summary>
    /// 1. 읽기는 읽기를 막지 않음<br></br>
    /// 2. 읽기는 쓰기를 막음<br></br>
    /// 3. 쓰기는 읽기를 막음(세부 내용 필요, 이미 있는 읽기는 종료를 기다리며 추가 읽기는 막음-2때문)<br></br>
    /// 4. 쓰기는 쓰기를 막음<br></br>
    /// </summary>
    /// <typeparam name="TResourceID">resource id</typeparam>
    public class RwLockSet<TResourceID> : Internals.PollingCommons
        where TResourceID : notnull
    {
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
                    _writes.Release(id);
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
    }
}
