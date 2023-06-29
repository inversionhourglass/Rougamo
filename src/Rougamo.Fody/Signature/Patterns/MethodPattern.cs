namespace Rougamo.Fody.Signature.Patterns
{
    public class MethodPattern : ExecutionPattern
    {
        public MethodPattern(ModifierPattern modifier, ITypePattern returnType, DeclaringTypeMethodPattern declaringTypeMethod, ITypePatterns methodParameters) : base(modifier, returnType, declaringTypeMethod, methodParameters)
        {
        }

        public override bool IsMatch(MethodSignature signature)
        {
            return signature.Definition.IsPlainMethod() && base.IsMatch(signature);
        }
    }
}
