namespace Cecil.AspectN.Matchers
{
    public interface IMatcher
    {
        bool IsMatch(MethodSignature signature);
    }
}
