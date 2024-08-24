namespace Cecil.AspectN.Patterns.Parsers
{
    public class MethodPatternParser : Parser
    {
        public static MethodPattern Parse(string pattern)
        {
            var executionPattern = ExecutionPatternParser.Parse(pattern);
            return new MethodPattern(executionPattern.Modifier, executionPattern.ReturnType, executionPattern.DeclaringTypeMethod, executionPattern.MethodParameters);
        }
    }
}
