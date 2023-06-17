using System.Linq;

namespace Rougamo.Fody.Signature
{
    public class MethodSignature
    {
        public MethodSignature(Modifier modifiers, TypeSignature returnType, TypeSignature declareType, GenericSignature method, TypeSignature[] methodParameters)
        {
            Modifiers = modifiers;
            ReturnType = returnType;
            DeclareType = declareType;
            Method = method;
            MethodParameters = methodParameters;
        }

        public Modifier Modifiers { get; set; }

        public TypeSignature ReturnType { get; set; }

        public TypeSignature DeclareType { get; set; }

        public GenericSignature Method { get; set; }

        public TypeSignature[] MethodParameters { get; set; }

        public override string ToString()
        {
            var modifiers = Modifiers.ToDefinitionString();
            var methodGenericParameter = Method.Generics.Length == 0 ? string.Empty : $"<{string.Join(",", Method.Generics.AsEnumerable())}>";
            var methodParameters = MethodParameters.Length == 0 ? string.Empty : string.Join(",", MethodParameters.AsEnumerable());

            return $"{modifiers} {ReturnType} {DeclareType}.{Method.Name}{methodGenericParameter}({methodParameters})";
        }
    }
}
