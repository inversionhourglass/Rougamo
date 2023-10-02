namespace PatternUsage.Attributes.Methods
{
    public class IntArrayReturnAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "method(int[,][][,,] *(..))";
    }
}
