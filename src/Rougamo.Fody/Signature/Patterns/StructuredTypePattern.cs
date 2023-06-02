using Rougamo.Fody.Signature.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rougamo.Fody.Signature.Patterns
{
    public class StructuredTypePattern
    {
        public TokenSource Namespace { get; set; }

        public NamePattern[] NestedTypeNames { get; set; }

        public bool AssignableMatch { get; set; }

        public bool WildcardMatch { get; set; }
    }
}
