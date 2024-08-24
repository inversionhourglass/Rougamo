using Cecil.AspectN.Tokens;
using System.Collections.Generic;

namespace Cecil.AspectN.Patterns.Parsers
{
    public class GetterPatternParser : Parser
    {
        public static GetterPattern Parse(string pattern)
        {
            var tokens = TokenSourceBuilder.Build(pattern);

            var modifier = ParseModifier(tokens);
            var propertyType = ParseType(tokens);
            var declaringTypeProperty = ParseType(tokens).ToDeclaringTypeMethod("get_");

            var genericParameters = new List<GenericParameterTypePattern>();
            declaringTypeProperty.DeclaringType.Compile(genericParameters, false);
            propertyType.Compile(genericParameters, true);

            return new GetterPattern(modifier, propertyType, declaringTypeProperty);
        }
    }
}
