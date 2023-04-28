using System.Linq;

namespace Rougamo.Fody.Signature
{
    public class MethodSignature
    {
        public MethodSignature(Modifier modifiers, TypeSignature returnType, TypeSignature declareType, string methodName, string[] methodGenericParameters, TypeSignature[] methodParameters)
        {
            Modifiers = modifiers;
            ReturnType = returnType;
            DeclareType = declareType;
            MethodName = methodName;
            MethodGenericParameters = methodGenericParameters;
            MethodParameters = methodParameters;
        }

        public Modifier Modifiers { get; set; }

        public TypeSignature ReturnType { get; set; }

        public TypeSignature DeclareType { get; set; }

        public string MethodName { get; set; }

        public string[] MethodGenericParameters { get; set; }

        public TypeSignature[] MethodParameters { get; set; }

        public override string ToString()
        {
            var modifiers = Modifiers.ToDefinitionString();
            var methodGenericParameter = MethodGenericParameters.Length == 0 ? string.Empty : $"<{string.Join(", ", MethodGenericParameters)}>";
            var methodParameters = MethodParameters.Length == 0 ? string.Empty : string.Join(", ", MethodParameters.AsEnumerable());

            return $"{modifiers} {ReturnType} {DeclareType}.{MethodName}{methodGenericParameter}({methodParameters})";
        }
    }
}
