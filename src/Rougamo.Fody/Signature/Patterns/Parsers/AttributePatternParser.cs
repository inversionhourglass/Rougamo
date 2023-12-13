using Rougamo.Fody.Signature.Tokens;

namespace Rougamo.Fody.Signature.Patterns.Parsers
{
    public class AttributePatternParser : Parser
    {
        public static AttributePattern Parse(string pattern)
        {
            var tokens = TokenSourceBuilder.Build(pattern);

            var position = ParsePosition(tokens);
            var index = position == AttributePositioon.Parameter ? ParseIndex(tokens) : -1;
            var attributeType = ParseType(tokens);
            attributeType.Compile([], false);

            return new AttributePattern(position, index, attributeType);
        }

        private static AttributePositioon ParsePosition(TokenSource tokens)
        {
            return tokens.Read()!.ToString().ToLower() switch
            {
                "type" => AttributePositioon.Type,
                "exec" => AttributePositioon.Execution,
                "para" => AttributePositioon.Parameter,
                "ret" => AttributePositioon.Return,
                "*" => AttributePositioon.Any,
                var s => throw new RougamoException($"Unrecognized attribute position [{s}], please check the attribute pattern ({tokens})")
            };
        }

        private static int ParseIndex(TokenSource tokens)
        {
            return tokens.Read()!.ToString() switch
            {
                "*" => -1,
                var s when int.TryParse(s, out var index) && index >= 0 => index,
                var s => throw new RougamoException($"Unrecognized parameter index [{s}], please check the attribute pattern ({tokens})")
            };
        }
    }
}
