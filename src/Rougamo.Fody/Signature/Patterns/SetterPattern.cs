namespace Rougamo.Fody.Signature.Patterns
{
    public class SetterPattern : PropertyPattern
    {
        public SetterPattern(ModifierPattern modifier, ITypePattern returnType, DeclaringTypeMethodPattern declaringTypeProperty) : base(modifier, returnType, declaringTypeProperty)
        {
        }

        protected override bool IsPropertyMethod(MethodSignature signature, out bool isGetter)
        {
            isGetter = false;
            return signature.Definition.IsSetter;
        }
    }
}
