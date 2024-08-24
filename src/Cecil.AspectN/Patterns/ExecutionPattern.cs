namespace Cecil.AspectN.Patterns
{
    public class ExecutionPattern
    {
        public ExecutionPattern(ModifierPattern modifier, ITypePattern returnType, DeclaringTypeMethodPattern declaringTypeMethod, ITypePatterns methodParameters)
        {
            Modifier = modifier;
            ReturnType = returnType;
            DeclaringTypeMethod = declaringTypeMethod;
            MethodParameters = methodParameters;
        }

        public ModifierPattern Modifier { get; }

        public ITypePattern ReturnType { get; }

        public DeclaringTypeMethodPattern DeclaringTypeMethod { get; }

        public ITypePatterns MethodParameters { get; }

        public virtual bool IsMatch(MethodSignature signature)
        {
            if (MethodParameters.Count != signature.MethodParameters.Length) return false;

            if (!Modifier.IsMatch(signature.Modifiers)) return false;

            if (ReturnType.AssignableMatch)
            {
                if (!DeclaringTypeMethod.IsMatch(signature.DeclareType, signature.Method)) return false;
                if (!ReturnType.IsMatch(signature.ReturnType)) return false;
            }
            else
            {
                if (!ReturnType.IsMatch(signature.ReturnType)) return false;
                if (!DeclaringTypeMethod.IsMatch(signature.DeclareType, signature.Method)) return false;
            }

            return MethodParameters.IsMatch(signature.MethodParameters);
        }
    }
}
