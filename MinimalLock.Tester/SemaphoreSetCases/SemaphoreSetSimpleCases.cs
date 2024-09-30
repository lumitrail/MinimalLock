namespace MinimalLock.Tester.SemaphoreSetCases
{
    public class SemaphoreSetSimpleCases : TesterBase
    {
        public SemaphoreSetSimpleCases(ITestOutputHelper c)
            : base(c)
        {
        }

        [Fact]
        public async Task Acquire()
        {
            SemaphoreSet<string> a = new();
            (string stringID1, string stringID2, string stringID3) = GetIDs();
            a.PollingIntervalMs = 1;
            a.DefaultWaitTimeoutMs = 10;
            a.MaxAllowed = 4;

            // acquire to MaxAllowed
            Assert.True(a.TryAcquire(stringID1));
            Assert.True(a.TryAcquire(stringID1));
            Assert.True(a.TryAcquire(stringID1));
            Assert.True(a.TryAcquire(stringID1));

            // cannot acquire anymore
            Assert.False(a.TryAcquire(stringID1));
            Assert.False(a.TryAcquire(stringID1));

            // null
            Assert.Throws<ArgumentNullException>(() => a.TryAcquire(null));

            // with waiting
            Assert.False(await a.TryAcquireAfterWait(stringID1));
            Assert.False(await a.TryAcquireAfterWait(stringID1));

            Assert.True(a.TryAcquire(stringID2));
            Assert.True(await a.TryAcquireAfterWait(stringID2));
            Assert.True(a.TryAcquire(stringID2));
            Assert.True(await a.TryAcquireAfterWait(stringID2));

            Assert.False(await a.TryAcquireAfterWait(stringID2));
            Assert.False(a.TryAcquire(stringID2));

            // with busy waiting
            Assert.False(a.TryAcquireAfterShortWait(stringID1));
            Assert.False(a.TryAcquireAfterShortWait(stringID1));

            Assert.True(a.TryAcquire(stringID3));
            Assert.True(a.TryAcquireAfterShortWait(stringID3));
            Assert.True(a.TryAcquire(stringID3));
            Assert.True(a.TryAcquireAfterShortWait(stringID3));

            Assert.False(a.TryAcquireAfterShortWait(stringID3));
            Assert.False(a.TryAcquire(stringID3));

            // changing data type

            SemaphoreSet<int> b = new();
            b.MaxAllowed = 2;

            Assert.True(b.TryAcquire(1));
            Assert.True(b.TryAcquire(1));
            Assert.False(b.TryAcquire(1));

            Assert.True(b.TryAcquire(5));
            Assert.True(b.TryAcquire(5));
            Assert.False(b.TryAcquire(5));
        }

        [Fact]
        public void IsUsed()
        {
            SemaphoreSet<string> a = new();
            (string stringID1, string stringID2, string stringID3) = GetIDs();
            a.PollingIntervalMs = 1;
            a.DefaultWaitTimeoutMs = 10;
            a.MaxAllowed = 2;

            Assert.True(a.TryAcquire(stringID1));
            Assert.True(a.TryAcquire(stringID1));
            Assert.True(a.IsUsed(stringID1));

            Assert.True(a.TryAcquire(stringID2));
            Assert.True(a.IsUsed(stringID2));

            Assert.False(a.IsUsed(stringID3));
        }

        [Fact]
        public void IsFull()
        {
            SemaphoreSet<string> a = new();
            (string stringID1, string stringID2, string stringID3) = GetIDs();
            a.PollingIntervalMs = 1;
            a.DefaultWaitTimeoutMs = 10;
            a.MaxAllowed = 2;

            Assert.True(a.TryAcquire(stringID1));
            Assert.True(a.TryAcquire(stringID1));
            Assert.True(a.IsFull(stringID1));

            Assert.True(a.TryAcquire(stringID2));
            Assert.False(a.IsFull(stringID2));

            Assert.False(a.IsFull(stringID3));
        }

        [Fact]
        public void GetUsedResources()
        {
            SemaphoreSet<string> a = new();
            (string stringID1, string stringID2, string stringID3) = GetIDs();
            a.PollingIntervalMs = 1;
            a.DefaultWaitTimeoutMs = 10;
            a.MaxAllowed = 2;

            Assert.True(a.TryAcquire(stringID1));
            Assert.True(a.TryAcquire(stringID1));

            Assert.True(a.TryAcquire(stringID2));

            var ur = a.GetUsedResources();
            Assert.True(ur.Length == 2);

            var urIDs = ur.Select(r => r.ID);
            Assert.Contains(stringID1, urIDs);
            Assert.Contains(stringID2, urIDs);
            Assert.DoesNotContain(stringID3, urIDs);

            var r1 = ur.Single(r => r.ID == stringID1);
            Assert.True(r1.Count == 2);

            var r2 = ur.Single(r => r.ID == stringID2);
            Assert.True(r2.Count == 1);
        }

        [Fact]
        public void Release()
        {
            SemaphoreSet<string> a = new();
            (string stringID1, string stringID2, string stringID3) = GetIDs();
            a.PollingIntervalMs = 1;
            a.DefaultWaitTimeoutMs = 10;
            a.MaxAllowed = 3;

            // acquire
            // to MaxAllowed
            Assert.True(a.TryAcquire(stringID1));
            Assert.True(a.TryAcquire(stringID1));
            Assert.True(a.TryAcquire(stringID1));

            // cannot acquire
            Assert.False(a.TryAcquire(stringID1));

            // release
            Assert.True(a.TryRelease(stringID1));

            // can acquire
            Assert.True(a.TryAcquire(stringID1));

            // release to 0
            Assert.True(a.TryRelease(stringID1));
            Assert.True(a.TryRelease(stringID1));
            Assert.True(a.TryRelease(stringID1));

            // GetUsedResources.Length = 0

            var ur = a.GetUsedResources();

            Assert.True(ur.Length == 0);
            Assert.Empty(ur);
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
        public void ReleaseAll()
        {
            SemaphoreSet<string> a = new();
            (string stringID1, string stringID2, string stringID3) = GetIDs();
            a.MaxAllowed = 2;

            Assert.True(a.TryAcquire(stringID1));
            Assert.True(a.TryAcquire(stringID2));
            Assert.True(a.TryAcquire(stringID3));

            Assert.True(a.TryAcquire(stringID1));
            Assert.True(a.TryAcquire(stringID2));
            Assert.True(a.TryAcquire(stringID3));

            var ur = a.GetUsedResources();

            var urIDs = ur.Select(r => r.ID);

            Assert.Contains(stringID1, urIDs);
            Assert.Contains(stringID2, urIDs);
            Assert.Contains(stringID3, urIDs);

            var urCounts = ur.Select(r => r.Count);
            Assert.True(urCounts.Max() == 2);
            Assert.True(urCounts.Min() == 2);

            a.ForceReleaseAll();

            var ur2 = a.GetUsedResources();

            Assert.True(!ur2.Any());

            Assert.True(a.TryAcquire(stringID1));
            Assert.True(a.TryAcquire(stringID2));
            Assert.True(a.TryAcquire(stringID3));

            Assert.True(a.TryAcquire(stringID1));
            Assert.True(a.TryAcquire(stringID2));
            Assert.True(a.TryAcquire(stringID3));
        }

        private async Task AcquireMakesWaitersWaitBase(bool useBusyWait)
        {
            SemaphoreSet<string> a = new();
            (string stringID1, _, _) = GetIDs();
            a.PollingIntervalMs = 1;
            a.MaxAllowed = 2;
            console.WriteLine($"Wait polling {a.PollingIntervalMs}ms");

            int delayMs = Convert.ToInt32(RNG.NextDouble() * 1000);
            a.DefaultWaitTimeoutMs = 2 * delayMs;

            Assert.True(a.TryAcquire(stringID1));
            Assert.True(a.TryAcquire(stringID1));
            Assert.True(a.IsFull(stringID1));

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
            Assert.True(a.IsFull(stringID1));

            double elapsedTimeMs = (endTime - startTime).TotalMilliseconds;

            console.WriteLine($"Delay: {delayMs}, Timeout: {a.DefaultWaitTimeoutMs}, Elapsed: {elapsedTimeMs}");
            console.WriteLine($"Performance loss: {elapsedTimeMs - delayMs}ms");

            Assert.True(elapsedTimeMs >= delayMs);
            Assert.True(elapsedTimeMs < a.DefaultWaitTimeoutMs);

            await t;
            Assert.True(a.IsFull(stringID1));
        }

        private async Task AcquireTimeoutBase(bool useBusyWait)
        {
            SemaphoreSet<string> a = new();
            (string stringID1, _, _) = GetIDs();
            a.PollingIntervalMs = 1;
            a.MaxAllowed = 2;
            console.WriteLine($"Wait polling {a.PollingIntervalMs}ms");

            int delayMs = Convert.ToInt32(RNG.NextDouble() * 1000);
            a.DefaultWaitTimeoutMs = delayMs / 2;

            Assert.True(a.TryAcquire(stringID1));
            Assert.True(a.TryAcquire(stringID1));
            Assert.True(a.IsFull(stringID1));

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
            Assert.True(a.IsFull(stringID1));

            double elapsedTimeMs = (endTime - startTime).TotalMilliseconds;

            console.WriteLine($"Delay: {delayMs}, Timeout: {a.DefaultWaitTimeoutMs}, Elapsed: {elapsedTimeMs}.");
            console.WriteLine($"Performance loss: {elapsedTimeMs - a.DefaultWaitTimeoutMs}ms");

            Assert.True(elapsedTimeMs < delayMs);
            Assert.True(elapsedTimeMs >= a.DefaultWaitTimeoutMs);

            await t;
            // released now
            Assert.False(a.IsFull(stringID1));
        }
    }
}
