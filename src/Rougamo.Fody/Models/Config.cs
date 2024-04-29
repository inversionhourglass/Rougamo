using System.Linq;
using System.Text.RegularExpressions;

namespace Rougamo.Fody.Models
{
    internal class Config
    {
        public Config(bool enabled, bool compositeAccessibility, int moArrayThreshold,  bool recordingIteratorReturns, bool reverseCallNonEntry, bool proxyCalling, string[] exceptTypePatterns)
        {
            Enabled = enabled;
            CompositeAccessibility = compositeAccessibility;
            MoArrayThreshold = moArrayThreshold;
            RecordingIteratorReturns = recordingIteratorReturns;
            ReverseCallNonEntry = reverseCallNonEntry;
            ProxyCalling = proxyCalling;
            ExceptTypePatterns = exceptTypePatterns.Select(x => new Regex(x)).ToArray();
        }

        public bool Enabled { get; }

        public bool CompositeAccessibility { get; }

        public int MoArrayThreshold { get; }

        public bool RecordingIteratorReturns { get; }

        public bool ReverseCallNonEntry { get; }

        public bool ProxyCalling { get; }

        public Regex[] ExceptTypePatterns { get; }
    }
}
