namespace MinimalLock.Tester.MutexSetCases
{
    public class MutexSetSimpleCases : TesterBase
    {
        public MutexSetSimpleCases(ITestOutputHelper c)
            : base(c)
        {
        }

        [Fact]
        public async Task Acquire()
        {
            MutexSet<string> a = new();
            (string stringID1, string stringID2, string stringID3) = GetIDs();
            a.PollingIntervalMs = 1;
            a.DefaultWaitTimeoutMs = 10;

            Assert.True(a.TryAcquire(stringID1));
            Assert.False(a.TryAcquire(stringID1));

            // null
            Assert.Throws<ArgumentNullException>(() => a.TryAcquire(null));

            // with waiting
            Assert.False(await a.TryAcquireAfterWait(stringID1));
            Assert.False(await a.TryAcquireAfterWait(stringID1));

            Assert.True(await a.TryAcquireAfterWait(stringID2));
            Assert.False(a.TryAcquire(stringID2));

            // with busy waiting
            Assert.False(a.TryAcquireAfterShortWait(stringID1));
            Assert.False(a.TryAcquireAfterShortWait(stringID1));

            Assert.True(a.TryAcquireAfterShortWait(stringID3));
            Assert.False(a.TryAcquire(stringID3));


            // changing data type

            MutexSet<int> b = new();

            Assert.True(b.TryAcquire(1));
            Assert.True(b.TryAcquire(2));
            Assert.False(b.TryAcquire(1));
            Assert.False(b.TryAcquire(2));
        }

        [Fact]
        public void IsLocked()
        {
            MutexSet<string> a = new();
            (string stringID1, string stringID2, string stringID3) = GetIDs();
            a.PollingIntervalMs = 1;

            a.TryAcquire(stringID1);
            a.TryAcquire(stringID2);

            Assert.True(a.IsLocked(stringID1));
            Assert.True(a.IsLocked(stringID2));
            Assert.False(a.IsLocked(stringID3));
        }

        [Fact]
        public void Release()
        {
            MutexSet<string> a = new();
            (string stringID1, _, _) = GetIDs();
            a.PollingIntervalMs = 1;

            Assert.True(a.TryAcquire(stringID1));
            Assert.False(a.TryAcquire(stringID1));

            Assert.True(a.TryRelease(stringID1));

            Assert.False(a.IsLocked(stringID1));
            Assert.True(a.TryAcquire(stringID1));

            Assert.True(a.TryRelease(stringID1));

            Assert.False(a.IsLocked(stringID1));
            Assert.True(a.TryAcquire(stringID1));

            Assert.True(a.TryRelease(stringID1));

            Assert.False(a.IsLocked(stringID1));
        }

        [Fact]
        public async Task AcquireMakesWaitersWait()
        {
            await AcquireMakesWaitersWaitBase(false);
        }

        [Fact]
        public async Task AcquireMakesWaitersBusyWait()
        {
            await AcquireMakesWaitersWaitBase(true);
        }

        [Fact]
        public async Task AcquireTimeout()
        {
            await AcquireTimeoutBase(false);
        }

        [Fact]
        public async Task AcquireTimeoutWithBusyWait()
        {
            await AcquireTimeoutBase(true);
        }

        [Fact]
        public async Task SerialAcquireRelease()
        {
            MutexSet<string> a = new();
            (string stringID1, _, _) = GetIDs();
            a.PollingIntervalMs = 1;

            for (int i = 0; i < 10; ++i)
            {
                Assert.True(a.TryAcquire(stringID1));
                Assert.True(a.IsLocked(stringID1));
                Assert.True(a.TryRelease(stringID1));
                Assert.False(a.IsLocked(stringID1));
            }

            for (int i = 0; i < 10; ++i)
            {
                Assert.True(await a.TryAcquireAfterWait(stringID1));
                Assert.True(a.IsLocked(stringID1));
                Assert.True(a.TryRelease(stringID1));
                Assert.False(a.IsLocked(stringID1));
            }
        }

        [Fact]
        public void GetUsedResources()
        {
            MutexSet<string> a = new();
            (string stringID1, string stringID2, string stringID3) = GetIDs();

            Assert.True(a.TryAcquire(stringID1));
            Assert.True(a.TryAcquire(stringID2));

            var ur = a.GetUsedResources();

            Assert.True(ur.Length == 2);
            Assert.Contains(stringID1, ur);
            Assert.Contains(stringID2, ur);
            Assert.DoesNotContain(stringID3, ur);
        }

        [Fact]
        public void ReleaseAll()
        {
            MutexSet<string> a = new();
            (string stringID1, string stringID2, string stringID3) = GetIDs();

            Assert.True(a.TryAcquire(stringID1));
            Assert.True(a.TryAcquire(stringID2));
            Assert.True(a.TryAcquire(stringID3));

            var ur = a.GetUsedResources();

            Assert.True(ur.Length == 3);

            a.ForceReleaseAll();

            var ur2 = a.GetUsedResources();

            Assert.True(ur2.Length == 0);

            Assert.True(a.TryAcquire(stringID1));
            Assert.True(a.TryAcquire(stringID2));
            Assert.True(a.TryAcquire(stringID3));
        }

        private async Task AcquireMakesWaitersWaitBase(bool useBusyWait)
        {
            MutexSet<string> a = new();
            (string stringID1, _, _) = GetIDs();
            a.PollingIntervalMs = 1;
            console.WriteLine($"Wait polling {a.PollingIntervalMs}ms");

            int delayMs = Convert.ToInt32(RNG.NextDouble() * 1000);
            a.DefaultWaitTimeoutMs = 2 * delayMs;

            Assert.True(a.TryAcquire(stringID1));

            DateTime startTime = DateTime.Now;

            // Release after delayMs
            Task t = Task.Run(async () => {
                startTime = DateTime.Now;
                await Task.Delay(delayMs);
                Assert.True(a.TryRelease(stringID1));
            });

            // Acquire after delayMs
            bool isAcquired;

            if (useBusyWait)
            {
                isAcquired = a.TryAcquireAfterShortWait(stringID1);
            }
            else
            {
                isAcquired = await a.TryAcquireAfterWait(stringID1);
            }

            DateTime endTime = DateTime.Now;

            Assert.True(isAcquired);
            Assert.True(a.IsLocked(stringID1));

            double elapsedTimeMs = (endTime - startTime).TotalMilliseconds;

            console.WriteLine($"Delay: {delayMs}, Timeout: {a.DefaultWaitTimeoutMs}, Elapsed: {elapsedTimeMs}");
            console.WriteLine($"Performance loss: {elapsedTimeMs - delayMs}ms");

            Assert.True(elapsedTimeMs >= delayMs);
            Assert.True(elapsedTimeMs < a.DefaultWaitTimeoutMs);

            await t;
            Assert.True(a.IsLocked(stringID1));
        }

        private async Task AcquireTimeoutBase(bool useBusyWait)
        {
            MutexSet<string> a = new();
            (string stringID1, _, _) = GetIDs();
            a.PollingIntervalMs = 1;
            console.WriteLine($"Wait polling {a.PollingIntervalMs}ms");

            int delayMs = Convert.ToInt32(RNG.NextDouble() * 1000);
            a.DefaultWaitTimeoutMs = delayMs / 2;

            Assert.True(a.TryAcquire(stringID1));

            DateTime startTime = DateTime.Now;

            // Release after delayMs
            Task t = Task.Run(async () => {
                startTime = DateTime.Now;
                await Task.Delay(delayMs);
                Assert.True(a.TryRelease(stringID1));
            });

            // Can't acquire as its timeout < delayMs
            bool isAcquired;

            if (useBusyWait)
            {
                isAcquired = a.TryAcquireAfterShortWait(stringID1);
            }
            else
            {
                isAcquired = await a.TryAcquireAfterWait(stringID1);
            }

            DateTime endTime = DateTime.Now;

            Assert.False(isAcquired);
            Assert.True(a.IsLocked(stringID1));

            double elapsedTimeMs = (endTime - startTime).TotalMilliseconds;

            console.WriteLine($"Delay: {delayMs}, Timeout: {a.DefaultWaitTimeoutMs}, Elapsed: {elapsedTimeMs}.");
            console.WriteLine($"Performance loss: {elapsedTimeMs - a.DefaultWaitTimeoutMs}ms");

            Assert.True(elapsedTimeMs < delayMs);
            Assert.True(elapsedTimeMs >= a.DefaultWaitTimeoutMs);

            await t;
            // released now
            Assert.False(a.IsLocked(stringID1));
        }
    }
}
