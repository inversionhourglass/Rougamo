using Cecil.AspectN.Tokens;
using System.Collections.Generic;

namespace Cecil.AspectN.Patterns.Parsers
{
    public class ExecutionPatternParser : Parser
    {
        public static ExecutionPattern Parse(string pattern)
        {
            var tokens = TokenSourceBuilder.Build(pattern);

            var modifier = ParseModifier(tokens);
            var async = ParseAsync(tokens);
            var returnType = ParseType(tokens);
            var declaringTypeMethod = ParseType(tokens).ToDeclaringTypeMethod();
            var parameters = ParseParameters(tokens);
            
            var genericParameters = new List<GenericParameterTypePattern>();
            declaringTypeMethod.DeclaringType.Compile(genericParameters, false);
            if (declaringTypeMethod.Method.GenericPatterns is TypePatterns tps)
            {
                foreach (var p in tps.Patterns)
                {
                    if (p is GenericParameterTypePattern gptp)
                    {
                        genericParameters.Add(gptp);
                    }
                }
            }
            returnType.Compile(genericParameters, true);
            if (parameters is IntermediateTypePatterns itps)
            {
                foreach (var p in itps.Patterns)
                {
                    if (p is IIntermediateTypePattern itp)
                    {
                        itp.Compile(genericParameters, true);
                    }
                }
            }

            var returnTypeX = async ? new AsyncTypePattern(returnType) : (ITypePattern)returnType;
            return new ExecutionPattern(modifier, returnTypeX, declaringTypeMethod, parameters);
        }
    }
}
