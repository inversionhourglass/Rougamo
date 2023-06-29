using Mono.Cecil;
using System.Linq;

namespace Rougamo.Fody.Signature
{
    public class MethodSignature
    {
        public MethodSignature(MethodDefinition definition, Modifier modifiers, TypeSignature returnType, TypeSignature declareType, GenericSignature method, TypeSignature[] methodParameters)
        {
            Definition = definition;
            Modifiers = modifiers;
            ReturnType = returnType;
            DeclareType = declareType;
            Method = method;
            MethodParameters = methodParameters;
        }

        public MethodDefinition Definition { get; }

        public Modifier Modifiers { get; }

        public TypeSignature ReturnType { get; }

        public TypeSignature DeclareType { get; }

        public GenericSignature Method { get; }

        public TypeSignature[] MethodParameters { get; }

        public override string ToString()
        {
            var modifiers = Modifiers.ToDefinitionString();
            var methodGenericParameter = Method.Generics.Length == 0 ? string.Empty : $"<{string.Join(",", Method.Generics.AsEnumerable())}>";
            var methodParameters = MethodParameters.Length == 0 ? string.Empty : string.Join(",", MethodParameters.AsEnumerable());

            return $"{modifiers} {ReturnType} {DeclareType}.{Method.Name}{methodGenericParameter}({methodParameters})";
        }
    }
}
