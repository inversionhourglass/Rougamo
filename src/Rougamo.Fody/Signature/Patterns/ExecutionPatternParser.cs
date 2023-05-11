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

            throw new NotImplementedException();
        }
    }
}
