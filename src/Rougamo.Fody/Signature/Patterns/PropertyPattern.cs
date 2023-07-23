namespace Rougamo.Fody.Signature.Patterns
{
    public class PropertyPattern
    {
        public PropertyPattern(ModifierPattern modifier, ITypePattern returnType, DeclaringTypeMethodPattern declaringTypeProperty)
        {
            Modifier = modifier;
            ReturnType = returnType;
            DeclaringTypeProperty = declaringTypeProperty;
        }

        public ModifierPattern Modifier { get; }

        public ITypePattern ReturnType { get; }

        public DeclaringTypeMethodPattern DeclaringTypeProperty { get; }

        public virtual bool IsMatch(MethodSignature signature)
        {
            if (!signature.Definition.IsGetter && !signature.Definition.IsSetter) return false;

            if (!Modifier.IsMatch(signature.Modifiers)) return false;

            if (ReturnType.AssignableMatch)
            {
                return DeclaringTypeProperty.IsMatch(signature.DeclareType, signature.Method) && ReturnType.IsMatch(signature.ReturnType);
            }
            else
            {
                return ReturnType.IsMatch(signature.ReturnType) && DeclaringTypeProperty.IsMatch(signature.DeclareType, signature.Method);
            }
        }
    }
}
