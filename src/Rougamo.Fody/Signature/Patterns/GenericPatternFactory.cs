using System;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody.Signature.Patterns
{
    public static class GenericPatternFactory
    {
        public static void FormatVirtualName(ITypePatterns patterns, string? genericPrefix)
        {
            if (patterns is TypePatterns typePatterns && genericPrefix != null)
            {
                var pts = typePatterns.Patterns;
                if (pts.Length == 1 && pts[0] is GenericParameterTypePattern genericPattern)
                {
                    genericPattern.VirtualName = genericPrefix;
                }
                else if (pts.Length > 1)
                {
                    for (var i = 0; i < pts.Length; i++)
                    {
                        if (pts[i] is GenericParameterTypePattern gptp)
                        {
                            gptp.VirtualName = genericPrefix + (i + 1);
                        }
                    }
                }
            }
        }

        //public static ITypePatterns New(IReadOnlyCollection<ITypePattern>? patterns, string? genericPrefix)
        //{
        //    if (patterns == null) return new NoneTypePatterns();
        //    if (patterns.Count == 0) return new AnyTypePatterns();

        //    var array = patterns.ToArray();
        //    if (genericPrefix != null)
        //    {
        //        if (array.Length == 1 && array[0] is GenericParameterTypePattern genericPattern)
        //        {
        //            genericPattern.VirtualName = genericPrefix;
        //        }
        //        else if (array.Length > 1)
        //        {
        //            for (var i = 0; i < array.Length; i++)
        //            {
        //                if (array[i] is GenericParameterTypePattern gptp)
        //                {
        //                    gptp.VirtualName = genericPrefix + (i + 1);
        //                }
        //            }
        //        }
        //    }
        //    return new TypePatterns(array);
        //}
    }
}
