using System;
using System.Collections.Generic;
using System.Text;

namespace Rougamo.Fody.Signature.Patterns
{
    public abstract class NamePattern
    {
        public abstract bool IsMatch(string name);
    }
}
