using Rougamo.Fody.Signature.Tokens;
using System.Collections.Generic;

namespace Rougamo.Fody.Signature.Patterns.Parsers
{
    public class SetterPatternParser : Parser
    {
        public static SetterPattern Parse(string pattern)
        {
            var tokens = TokenSourceBuilder.Build(pattern);

            var modifier = ParseModifier(tokens);
            var propertyType = ParseType(tokens);
            var declaringTypeProperty = ParseType(tokens).ToDeclaringTypeMethod("set_");

            var genericParameters = new List<GenericParameterTypePattern>();
            declaringTypeProperty.DeclaringType.Compile(genericParameters, false);
            propertyType.Compile(genericParameters, true);

            return new SetterPattern(modifier, propertyType, declaringTypeProperty);
        }
    }
}
