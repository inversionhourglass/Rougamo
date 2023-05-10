namespace Rougamo.Fody.Signature.Matchers
{
    public interface IMatcher
    {
        bool IsMatch(MethodSignature signature);
    }
}
