namespace MinimalLock.Tester.RwLockSetCases
{
    public class RwLockSetSimpleCases : TesterBase
    {
        public RwLockSetSimpleCases(ITestOutputHelper c)
            : base(c)
        {
        }

        [Fact]
        public async Task ReadBlocksNoReads()
        {
            RwLockSet<string> a = new();
            a.MaxReads = 2;
            a.DefaultWaitTimeoutMs = 1;
            a.PollingIntervalMs = 1;
            (string id1, string id2, _) = GetIDs();

            Assert.True(a.TryAcquireReading(id1));
            Assert.True(a.TryAcquireReading(id1));
            Assert.False(a.TryAcquireReading(id1));

            Assert.True(a.TryReleaseReading(id1));
            Assert.True(a.TryAcquireReading(id1));

            Assert.True(a.TryAcquireReading(id2));

            a.ForceReleaseReads();
            a.ForceReleaseWrites();

            Assert.True(await a.TryAcquireReadingAfterWait(id1));
            Assert.True(await a.TryAcquireReadingAfterWait(id1));
            Assert.False(await a.TryAcquireReadingAfterWait(id1));

            Assert.True(a.TryReleaseReading(id1));
            Assert.True(await a.TryAcquireReadingAfterWait(id1));

            Assert.True(await a.TryAcquireReadingAfterWait(id2));
        }

        [Fact]
        public async void ReadBlocksWrites()
        {
            RwLockSet<string> a = new();
            a.MaxReads = 2;
            a.DefaultWaitTimeoutMs = 1;
            a.PollingIntervalMs = 1;
            (string id1, string id2, _) = GetIDs();

            Assert.True(a.TryAcquireReading(id1));
            Assert.False(a.TryAcquireWriting(id1));
            Assert.True(a.TryReleaseReading(id1));
            Assert.True(a.TryAcquireWriting(id1));

            Assert.True(a.TryAcquireWriting(id2));

            a.ForceReleaseReads();
            a.ForceReleaseWrites();

            Assert.True(await a.TryAcquireReadingAfterWait(id1));
            Assert.False(await a.TryAcquireWritingAfterWait(id1));
            Assert.True(a.TryReleaseReading(id1));
            Assert.True(await a.TryAcquireWritingAfterWait(id1));

            Assert.True(await a.TryAcquireWritingAfterWait(id2));
        }

        [Fact]
        public async Task WriteBlocksReads()
        {
            RwLockSet<string> a = new();
            a.MaxReads = 2;
            a.DefaultWaitTimeoutMs = 1;
            a.PollingIntervalMs = 1;
            (string id1, string id2, _) = GetIDs();

            Assert.True(a.TryAcquireWriting(id1));
            Assert.False(a.TryAcquireReading(id1));
            Assert.True(a.TryAcquireReading(id2));
            Assert.True(a.TryReleaseWriting(id1));
            Assert.True(a.TryAcquireReading(id1));

            a.ForceReleaseReads();
            a.ForceReleaseWrites();

            Assert.True(await a.TryAcquireWritingAfterWait(id1));
            Assert.False(await a.TryAcquireReadingAfterWait(id1));
            Assert.True(await a.TryAcquireReadingAfterWait(id2));
            Assert.True(a.TryReleaseWriting(id1));
            Assert.True(await a.TryAcquireReadingAfterWait(id1));
        }

        [Fact]
        public async Task WriteBlocksWrites()
        {
            RwLockSet<string> a = new();
            a.MaxReads = 2;
            a.DefaultWaitTimeoutMs = 1;
            a.PollingIntervalMs = 1;
            (string id1, string id2, _) = GetIDs();

            Assert.True(a.TryAcquireWriting(id1));
            Assert.False(a.TryAcquireWriting(id1));
            Assert.True(a.TryAcquireWriting(id2));
            Assert.True(a.TryReleaseWriting(id1));
            Assert.True(a.TryAcquireWriting(id1));

            a.ForceReleaseReads();
            a.ForceReleaseWrites();

            Assert.True(await a.TryAcquireWritingAfterWait(id1));
            Assert.False(await a.TryAcquireWritingAfterWait(id1));
            Assert.True(await a.TryAcquireWritingAfterWait(id2));
            Assert.True(a.TryReleaseWriting(id1));
            Assert.True(await a.TryAcquireWritingAfterWait(id1));
        }
    }
}
