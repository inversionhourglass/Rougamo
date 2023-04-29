namespace Rougamo.Fody.Signature
{
    public class SimpleTypeSignature : TypeSignature
    {
        public SimpleTypeSignature(string name, TypeSignature? declaringType) : base(name, declaringType, TypeCategory.Simple)
        {
        }
    }
}
