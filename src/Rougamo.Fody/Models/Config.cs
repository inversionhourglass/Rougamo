namespace Rougamo.Fody.Models
{
    internal class Config
    {
        public Config(bool enabled, int moArrayThreshold,  bool recordingIteratorReturns, bool reverseCallNonEntry)
        {
            Enabled = enabled;
            MoArrayThreshold = moArrayThreshold;
            RecordingIteratorReturns = recordingIteratorReturns;
            ReverseCallNonEntry = reverseCallNonEntry;
        }

        public bool Enabled { get; }

        public int MoArrayThreshold { get; }

        public bool RecordingIteratorReturns { get; }

        public bool ReverseCallNonEntry { get; }
    }
}
