using System.Collections.Generic;

namespace Cecil.AspectN.Patterns
{
    public class IntermediateTypePatterns : TypePatterns
    {
        public IntermediateTypePatterns(IIntermediateTypePattern[] patterns) : base(patterns)
        {
        }

        public void Compile(List<GenericParameterTypePattern> genericParameters, bool genericIn)
        {
            foreach (var pattern in Patterns)
            {
                if (pattern is IIntermediateTypePattern intermediatePattern)
                {
                    intermediatePattern.Compile(genericParameters, genericIn);
                }
            }
        }
    }
}
