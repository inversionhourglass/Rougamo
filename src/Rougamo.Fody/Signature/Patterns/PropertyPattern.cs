namespace Rougamo.Fody.Signature.Patterns
{
    public class PropertyPattern
    {
        public PropertyPattern(ModifierPattern modifier, ITypePattern propertyType, DeclaringTypeMethodPattern declaringTypeProperty)
        {
            Modifier = modifier;
            PropertyType = propertyType;
            DeclaringTypeProperty = declaringTypeProperty;
        }

        public ModifierPattern Modifier { get; }

        public ITypePattern PropertyType { get; }

        public DeclaringTypeMethodPattern DeclaringTypeProperty { get; }

        public virtual bool IsMatch(MethodSignature signature)
        {
            if (!IsPropertyMethod(signature, out var isGetter)) return false;

            if (!Modifier.IsMatch(signature.Modifiers)) return false;

            var propertyTypeSignature = isGetter ? signature.ReturnType : signature.MethodParameters[0];
            if (PropertyType.AssignableMatch)
            {
                return DeclaringTypeProperty.IsMatch(signature.DeclareType, signature.Method) && PropertyType.IsMatch(propertyTypeSignature);
            }
            else
            {
                return PropertyType.IsMatch(propertyTypeSignature) && DeclaringTypeProperty.IsMatch(signature.DeclareType, signature.Method);
            }
        }

        protected virtual bool IsPropertyMethod(MethodSignature signature, out bool isGetter)
        {
            if (signature.Definition.IsGetter)
            {
                isGetter = true;
                return true;
            }
            isGetter = false;
            return signature.Definition.IsSetter;
        }
    }
}
