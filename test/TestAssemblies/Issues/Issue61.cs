using Issues.Attributes;
using Rougamo;
using System;

namespace Issues
{
    [SkipRefStruct]
    public class Issue61
    {
        public delegate string In(ReadOnlySpan<char> span);

        public delegate ReadOnlySpan<char> Out(string str);

        [_61_]
        public string ReadOnlySpanIn(ReadOnlySpan<char> span)
        {
            return span.ToString();
        }

        [_61_]
        public ReadOnlySpan<char> ReadOnlySpanOut(string str)
        {
            return new ReadOnlySpan<char>(str.ToCharArray());
        }
    }
}
