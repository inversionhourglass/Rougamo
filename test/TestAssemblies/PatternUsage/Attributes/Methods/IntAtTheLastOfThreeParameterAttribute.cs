namespace PatternUsage.Attributes.Methods
{
    public class IntAtTheLastOfThreeParameterAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "method(* *(,,int))";
    }
}
