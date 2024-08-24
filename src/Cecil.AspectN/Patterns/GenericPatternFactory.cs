namespace Cecil.AspectN.Patterns
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
    }
}
