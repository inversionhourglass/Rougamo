namespace Cecil.AspectN.Patterns
{
    public class DeclaringTypeMethodPattern
    {
        public DeclaringTypeMethodPattern(IIntermediateTypePattern declaringType, GenericNamePattern method)
        {
            DeclaringType = declaringType;
            Method = method;
        }

        public IIntermediateTypePattern DeclaringType { get; }

        public GenericNamePattern Method { get; }

        public virtual bool IsMatch(TypeSignature declaringType, GenericSignature method)
        {
            return DeclaringType.IsMatch(declaringType) && Method.IsMatch(method);
        }
    }
}
