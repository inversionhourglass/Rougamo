namespace PatternUsage.Attributes.Methods
{
    public class DoubleAtTheSecondOfThreeParameterAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "method(* *(,double,))";
    }
}
