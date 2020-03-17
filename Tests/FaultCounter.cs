namespace Tests
{
    public class FaultCounter
    {
        private int faults = 0;

        public int Faults { get; }

        public void Increment()
        {
            faults++;
        }
    }
}
