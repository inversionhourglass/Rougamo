namespace Rougamo.Fody.Signature.Patterns
{
    public class NotDeclaringTypeMethodPattern : DeclaringTypeMethodPattern
    {
        public NotDeclaringTypeMethodPattern(IIntermediateTypePattern declaringType, GenericNamePattern method) : base(declaringType, method)
        {
        }

        public override bool IsMatch(TypeSignature declaringType, GenericSignature method)
        {
            if (Method.IsAny) return DeclaringType.IsAny || !DeclaringType.IsMatch(declaringType);

            if (DeclaringType.IsAny) return !Method.IsMatch(method);

            return !DeclaringType.IsMatch(declaringType) || !Method.IsMatch(method);
        }
    }
}
