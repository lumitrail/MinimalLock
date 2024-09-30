namespace MinimalLock.Tester
{
    public abstract class TesterBase
    {
        public static Random RNG = new(DateTime.Now.Millisecond);

        protected readonly ITestOutputHelper console;

        public TesterBase(ITestOutputHelper c)
        {
            console = c;
        }

        protected (string id1, string id2, string id3) GetIDs()
        {
            string stringID1 = Guid.NewGuid().ToString();
            string stringID2 = Guid.NewGuid().ToString();
            string stringID3 = Guid.NewGuid().ToString();

            return (stringID1, stringID2, stringID3);
        }
    }
}
