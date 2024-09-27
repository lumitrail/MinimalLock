namespace MinimalLock.Internals
{
    public abstract class PollingCommons
    {
        /// <summary>Wait loop granularity</summary>
        /// <remarks>Typical effective range: [15.625ms, 30sec] due to system clock interrupt rate</remarks>
        public int PollingIntervalMs
        {
            get => _pollingIntervalMs;
            set => _pollingIntervalMs = Math.Min(Math.Max(1, value), 30000);
        }
        private int _pollingIntervalMs = 10;

        /// <summary>Wait timeout</summary>
        /// <remarks>Effective range: [16ms, 10min]</remarks>
        public int DefaultWaitTimeoutMs
        {
            get => _defaultWaitTimeoutMs;
            set => _defaultWaitTimeoutMs = Math.Min(Math.Max(10, value), 600000);
        }
        private int _defaultWaitTimeoutMs = 10000;

        /// <summary>
        /// Checks whether it's timeout, from startTime.
        /// </summary>
        /// <param name="startTime"></param>
        /// <returns></returns>
        protected bool IsTimeout(DateTime startTime)
        {
            return GetElapsedTimeMs(startTime) >= DefaultWaitTimeoutMs;
        }

        /// <summary>
        /// Gets elapsed time from startTime in milliseconds.
        /// </summary>
        /// <param name="startTime"></param>
        /// <returns></returns>
        protected double GetElapsedTimeMs(DateTime startTime)
        {
            return (DateTime.Now - startTime).TotalMilliseconds;
        }
    }
}
