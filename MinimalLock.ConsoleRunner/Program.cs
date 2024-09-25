using System.Diagnostics;

namespace MinimalLock.ConsoleRunner
{
    internal class Program
    {
        static Random RNG = new(DateTime.Now.Microsecond);

        static async Task Main(string[] args)
        {
            await AcquireMakesWaiters(true);
            await AcquireMakesWaiters(true);
        }

        private static async Task AcquireMakesWaiters(bool useBusyWait)
        {
            MutexSet<string> a = new();
            (string stringID1, _, _) = GetIDs();
            a.PollingIntervalMs = 1;
            Console.WriteLine($"Wait polling {a.PollingIntervalMs}ms");

            int delayMs = Convert.ToInt32(RNG.NextDouble() * 1000);

            if (delayMs < 50)
            {
                delayMs += 50;
            }

            a.DefaultWaitTimeoutMs = 2 * delayMs;

            Debug.Assert(a.TryAcquire(stringID1));

            // Release after delayMs
            Task t = Task.Run(async () => {
                await Task.Delay(delayMs);
                a.Release(stringID1);
                //Assert.False(a.IsLocked(stringID1));
            });

            // Acquire after delayMs
            bool isAcquired;
            List<DateTime> loopTimestamps;

            if (useBusyWait)
            {
                (isAcquired, loopTimestamps) = a.TryAcquireAfterBusyWait(stringID1);
            }
            else
            {
                (isAcquired, loopTimestamps) = await a.TryAcquireAfterWait(stringID1);
            }


            Debug.Assert(isAcquired);
            Debug.Assert(a.IsLocked(stringID1));

            double elapsedTimeMs = (loopTimestamps.Last() - loopTimestamps.First()).TotalMilliseconds;

            Console.WriteLine($"Delay: {delayMs}, Timeout: {a.DefaultWaitTimeoutMs}, Elapsed: {elapsedTimeMs}");

            var last100Stamps = loopTimestamps.TakeLast(100).ToList();
            PrintIntervals(last100Stamps);
            //PrintIntervals(loopTimestamps);

            await t;
            Debug.Assert(a.IsLocked(stringID1));

            string? d = Console.ReadLine();
        }

        private static void PrintIntervals(IReadOnlyList<DateTime> loopTimestamps)
        {
            if (loopTimestamps == null)
            {
                return;
            }

            var loopElapsedTimes = new List<double>(loopTimestamps.Count);

            DateTime previousCursor = loopTimestamps.First();

            for (int i = 1; i < loopTimestamps.Count; ++i)
            {
                loopElapsedTimes.Add((loopTimestamps[i] - previousCursor).TotalMilliseconds);
                previousCursor = loopTimestamps[i];
            }

            foreach (var et in loopElapsedTimes)
            {
                Console.WriteLine(et);
            }
        }

        private static (string id1, string id2, string id3) GetIDs()
        {
            string stringID1 = Guid.NewGuid().ToString();
            string stringID2 = Guid.NewGuid().ToString();
            string stringID3 = Guid.NewGuid().ToString();

            return (stringID1, stringID2, stringID3);
        }
    }
}
