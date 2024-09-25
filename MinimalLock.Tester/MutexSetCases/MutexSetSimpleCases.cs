//namespace MinimalLock.Tester.MutexSetCases
//{
//    public class MutexSetSimpleCases
//    {
//        static Random RNG = new(DateTime.Now.Millisecond);

//        private readonly ITestOutputHelper console;

//        public MutexSetSimpleCases(ITestOutputHelper c)
//        {
//            console = c;
//        }

//        [Fact]
//        public async Task Acquire()
//        {
//            MutexSet<string> a = new();
//            (string stringID1, string stringID2, string stringID3) = GetIDs();
//            a.PollingIntervalMs = 1;

//            Assert.True(a.TryAcquire(stringID1));
//            Assert.True(a.TryAcquire(stringID2));

//            Assert.False(a.TryAcquire(stringID1));
//            Assert.False(a.TryAcquire(stringID2));

//            Assert.Throws<ArgumentNullException>(() => a.TryAcquire(null));

//            MutexSet<int> b = new();

//            Assert.True(b.TryAcquire(1));
//            Assert.True(b.TryAcquire(2));

//            a.DefaultWaitTimeoutMs = 10;

//            Assert.False((await a.TryAcquireAfterWait(stringID1)).acquired);
//            Assert.False((await a.TryAcquireAfterWait(stringID2)).acquired);
//            Assert.True((await a.TryAcquireAfterWait(stringID3)).acquired);
//        }

//        [Fact]
//        public void IsLocked()
//        {
//            MutexSet<string> a = new();
//            (string stringID1, string stringID2, string stringID3) = GetIDs();
//            a.PollingIntervalMs = 1;

//            a.TryAcquire(stringID1);
//            a.TryAcquire(stringID2);

//            Assert.True(a.IsLocked(stringID1));
//            Assert.True(a.IsLocked(stringID2));
//            Assert.False(a.IsLocked(stringID3));
//        }

//        [Fact]
//        public void Release()
//        {
//            MutexSet<string> a = new();
//            (string stringID1, _, _) = GetIDs();
//            a.PollingIntervalMs = 1;

//            Assert.True(a.TryAcquire(stringID1));
//            Assert.False(a.TryAcquire(stringID1));

//            a.Release(stringID1);

//            Assert.False(a.IsLocked(stringID1));
//            Assert.True(a.TryAcquire(stringID1));

//            a.Release(stringID1);

//            Assert.False(a.IsLocked(stringID1));
//            Assert.True(a.TryAcquire(stringID1));

//            a.Release(stringID1);

//            Assert.False(a.IsLocked(stringID1));
//        }

//        [Fact]
//        public async Task AcquireMakesWaiters()
//        {
//            await AcquireMakesWaitersBase(false);
//        }


//        [Fact]
//        public async Task AcquireMakesWaitersWithBusyWait()
//        {
//            await AcquireMakesWaitersBase(true);
//        }

//        [Fact]
//        public async Task AcquireWaitTimeout()
//        {
//            await AcquireWaitTimeoutBase(false);
//        }

//        [Fact]
//        public async Task AcquireWaitTimeoutWithBusyWait()
//        {
//            await AcquireWaitTimeoutBase(true);
//        }

//        private async Task AcquireWaitTimeoutBase(bool useBusyWait)
//        {
//            MutexSet<string> a = new();
//            (string stringID1, _, _) = GetIDs();
//            a.PollingIntervalMs = 1;
//            console.WriteLine($"Wait polling {a.PollingIntervalMs}ms");

//            int delayMs = Convert.ToInt32(RNG.NextDouble() * 1000);
//            a.DefaultWaitTimeoutMs = delayMs / 2;

//            Assert.True(a.TryAcquire(stringID1));

//             Release after delayMs
//            Task t = Task.Run(async () => {
//                await Task.Delay(delayMs);
//                a.Release(stringID1);
//                Assert.False(a.IsLocked(stringID1));
//            });

//             Can't acquire as its timeout < delayMs
//            bool isAcquired;
//            List<DateTime> elapsedTimeInner;
//            if (useBusyWait)
//            {
//                (isAcquired, elapsedTimeInner) = a.TryAcquireAfterBusyWait(stringID1);
//            }
//            else
//            {
//                (isAcquired, elapsedTimeInner) = await a.TryAcquireAfterWait(stringID1);
//            }

//            Assert.False(isAcquired);
//            Assert.True(a.IsLocked(stringID1));

//            console.WriteLine($"Delay: {delayMs}, Timeout: {a.DefaultWaitTimeoutMs}, Elapsed: {elapsedTimeInner}.");
//            console.WriteLine($"Performance loss: {elapsedTimeInner - a.DefaultWaitTimeoutMs}ms");

//            Assert.True(elapsedTimeInner < delayMs);
//            Assert.True(elapsedTimeInner >= a.DefaultWaitTimeoutMs);

//            await t;
//             released now
//            Assert.False(a.IsLocked(stringID1));
//        }

//        private async Task AcquireMakesWaitersBase(bool useBusyWait)
//        {
//            MutexSet<string> a = new();
//            (string stringID1, _, _) = GetIDs();
//            a.PollingIntervalMs = 1;
//            console.WriteLine($"Wait polling {a.PollingIntervalMs}ms");

//            int delayMs = Convert.ToInt32(RNG.NextDouble() * 1000);
//            a.DefaultWaitTimeoutMs = 2 * delayMs;

//            Assert.True(a.TryAcquire(stringID1));

//             Release after delayMs
//            Task t = Task.Run(async () => {
//                await Task.Delay(delayMs);
//                a.Release(stringID1);
//                Assert.False(a.IsLocked(stringID1));
//            });

//             Acquire after delayMs
//            bool isAcquired;
//            var elapsedTimeInner;
//            if (useBusyWait)
//            {
//                (isAcquired, elapsedTimeInner) = a.TryAcquireAfterBusyWait(stringID1);
//            }
//            else
//            {
//                (isAcquired, elapsedTimeInner) = await a.TryAcquireAfterWait(stringID1);
//            }

//            Assert.True(isAcquired);
//            Assert.True(a.IsLocked(stringID1));

//            console.WriteLine($"Delay: {delayMs}, Timeout: {a.DefaultWaitTimeoutMs}, Elapsed: {elapsedTimeInner}");
//            console.WriteLine($"Performance loss: {elapsedTimeInner - delayMs}ms");

//            Assert.True(elapsedTimeInner >= delayMs);
//            Assert.True(elapsedTimeInner < a.DefaultWaitTimeoutMs);

//            await t;
//            Assert.True(a.IsLocked(stringID1));
//        }

//        [Fact]
//        public async Task SerialAcquireRelease()
//        {
//            MutexSet<string> a = new();
//            (string stringID1, _, _) = GetIDs();
//            a.PollingIntervalMs = 1;

//            for (int i = 0; i < 10; ++i)
//            {
//                Assert.True(a.TryAcquire(stringID1));
//                Assert.True(a.IsLocked(stringID1));
//                a.Release(stringID1);
//                Assert.False(a.IsLocked(stringID1));
//            }

//            for (int i = 0; i < 10; ++i)
//            {
//                Assert.True((await a.TryAcquireAfterWait(stringID1)).acquired);
//                Assert.True(a.IsLocked(stringID1));
//                a.Release(stringID1);
//                Assert.False(a.IsLocked(stringID1));
//            }
//        }

//        private (string id1, string id2, string id3) GetIDs()
//        {
//            string stringID1 = Guid.NewGuid().ToString();
//            string stringID2 = Guid.NewGuid().ToString();
//            string stringID3 = Guid.NewGuid().ToString();

//            return (stringID1, stringID2, stringID3);
//        }
//    }
//}
