using Rougamo.Fody.Signature.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rougamo.Fody.Signature.Patterns
{
    public class ExecutionPatternParser : Parser
    {
        public static ExecutionPattern Parse(string pattern)
        {
            var tokens = TokenSourceBuilder.Build(pattern);
            
            var modifier = ParseModifier(tokens);
            var returnTypePattern = ParseType(tokens);
            var declareTypePattern = ParseType(tokens);
            var methodNamePattern = declareTypePattern.ExtractNamePattern();
            var parameterTypePattern = ParseParameters(tokens);
            return default;
        }
    }
}
