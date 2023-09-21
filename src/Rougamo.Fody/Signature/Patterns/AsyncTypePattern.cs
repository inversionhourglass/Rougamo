using System.Runtime.CompilerServices;

namespace Rougamo.Fody.Signature.Patterns
{
    public class AsyncTypePattern : ITypePattern
    {
        public AsyncTypePattern(ITypePattern pattern)
        {
            ElementPattern = pattern;
        }

        public ITypePattern ElementPattern { get; }

        public bool IsAny => false;

        public bool IsVoid => false;

        public bool AssignableMatch => ElementPattern.AssignableMatch;

        public bool IsMatch(TypeSignature signature)
        {
            if (signature.NestedTypes.Length != 1) return false;

            var typeSignature = signature.NestedTypes[0];
            if (ElementPattern.IsVoid)
            {
                return typeSignature.Generics.Length == 0 && signature.Namespace == "System" && typeSignature.Name == "Void";
            }

            if (ElementPattern is NullTypePattern) return typeSignature.Generics.Length == 0 && IsTask(signature, typeSignature);

            return typeSignature.Generics.Length == 1 && IsTask(signature, typeSignature) && ElementPattern.IsMatch(typeSignature.Generics[0]);
        }

        private bool IsTask(TypeSignature signature, GenericSignature typeSignature) => signature.Namespace == "System.Threading.Tasks" && (typeSignature.Name == "Task" || typeSignature.Name == "ValueTask");
    }
}
