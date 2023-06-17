using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody.Signature.Patterns
{
    public static class GenericPatternFactory
    {
        public static ITypePatterns New(IReadOnlyCollection<ITypePattern>? patterns, string? genericPrefix)
        {
            if (patterns == null) return new NoneTypePatterns();
            if (patterns.Count == 0) return new AnyTypePatterns();

            var array = patterns.ToArray();
            if (genericPrefix != null)
            {
                if (array.Length == 1 && array[0] is GenericParameterTypePattern genericPattern)
                {
                    genericPattern.VirtualName = genericPrefix;
                }
                else if (array.Length > 1)
                {
                    for (var i = 0; i < array.Length; i++)
                    {
                        if (array[i] is GenericParameterTypePattern gptp)
                        {
                            gptp.VirtualName = genericPrefix + (i + 1);
                        }
                    }
                }
            }
            return new TypePatterns(array);
        }
    }
}
