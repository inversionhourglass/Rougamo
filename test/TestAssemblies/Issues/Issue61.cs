using Issues.Attributes;
using Rougamo.Context;
using System;

namespace Issues
{
    public class Issue61
    {
        public delegate string In(ReadOnlySpan<char> span);

        public delegate ReadOnlySpan<char> Out(string str);

        [_61_(MethodContextOmits = Omit.Arguments)]
        public string ReadOnlySpanIn(ReadOnlySpan<char> span)
        {
            return span.ToString();
        }

        [_61_(MethodContextOmits = Omit.ReturnValue)]
        public ReadOnlySpan<char> ReadOnlySpanOut(string str)
        {
            return new ReadOnlySpan<char>(str.ToCharArray());
        }
    }
}
