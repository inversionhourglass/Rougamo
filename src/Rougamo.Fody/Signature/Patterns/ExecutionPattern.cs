namespace Rougamo.Fody.Signature.Patterns
{
    public class ExecutionPattern
    {
        public ExecutionPattern(ModifierPattern modifier, ITypePattern returnType, DeclaringTypeMethodPattern declaringTypeMethod, ITypePatterns methodParameters)
        {
            Modifier = modifier;
            ReturnType = returnType;
            //DeclareType = declareType;
            //Method = method;
            DeclaringTypeMethod = declaringTypeMethod;
            MethodParameters = methodParameters;
        }

        public ModifierPattern Modifier { get; }

        public ITypePattern ReturnType { get; }

        //public ITypePattern DeclareType { get; }

        //public GenericNamePattern Method { get; }

        public DeclaringTypeMethodPattern DeclaringTypeMethod { get; }

        public ITypePatterns MethodParameters { get; }

        public bool IsMatch(MethodSignature signature)
        {
            if (MethodParameters.Count != signature.Method.Generics.Length) return false;

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

            //if (!Method.IsMatch(signature.Method)) return false;

            //if (ReturnType.AssignableMatch)
            //{
            //    if (!DeclareType.IsMatch(signature.DeclareType)) return false;
            //    if (!ReturnType.IsMatch(signature.ReturnType)) return false;
            //}
            //else
            //{
            //    if (!ReturnType.IsMatch(signature.ReturnType)) return false;
            //    if (!DeclareType.IsMatch(signature.DeclareType)) return false;
            //}

            return MethodParameters.IsMatch(signature.MethodParameters);
        }
    }
}
