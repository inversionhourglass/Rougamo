namespace Rougamo.Fody.Signature.Patterns
{
    public class GetterPattern : PropertyPattern
    {
        public GetterPattern(ModifierPattern modifier, ITypePattern returnType, DeclaringTypeMethodPattern declaringTypeProperty) : base(modifier, returnType, declaringTypeProperty)
        {
        }

        protected override bool IsPropertyMethod(MethodSignature signature, out bool isGetter)
        {
            isGetter = true;
            return signature.Definition.IsGetter;
        }
    }
}
