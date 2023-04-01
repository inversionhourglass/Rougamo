namespace Rougamo.Fody.Models
{
    internal class Config
    {
        public Config(bool enabled, bool recordingIteratorReturns, bool reverseCallNonEntry)
        {
            Enabled = enabled;
            RecordingIteratorReturns = recordingIteratorReturns;
            ReverseCallNonEntry = reverseCallNonEntry;
        }

        public bool Enabled { get; }

        public bool RecordingIteratorReturns { get; }

        public bool ReverseCallNonEntry { get; }
    }
}
