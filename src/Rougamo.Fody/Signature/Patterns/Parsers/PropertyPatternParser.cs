using Rougamo.Fody.Signature.Tokens;
using System.Collections.Generic;

namespace Rougamo.Fody.Signature.Patterns.Parsers
{
    public class PropertyPatternParser : Parser
    {
        public static PropertyPattern Parse(string pattern)
        {
            var tokens = TokenSourceBuilder.Build(pattern);

            var modifier = ParseModifier(tokens);
            var propertyType = ParseType(tokens);
            var declaringTypeProperty = ParseType(tokens).ToDeclaringTypeMethod("get_", "set_");

            var genericParameters = new List<GenericParameterTypePattern>();
            declaringTypeProperty.DeclaringType.Compile(genericParameters, false);
            propertyType.Compile(genericParameters, true);

            return new PropertyPattern(modifier, propertyType, declaringTypeProperty);
        }
    }
}
