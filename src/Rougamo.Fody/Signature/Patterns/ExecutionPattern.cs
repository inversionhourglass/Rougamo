namespace Rougamo.Fody.Signature.Patterns
{
    public class ExecutionPattern
    {
        public ModifierPattern Modifiers { get; set; }

        public TypePattern ReturnType { get; set; }

        public TypePattern DeclareType { get; set; }

        public NamePattern MethodName { get; set; }

        public TypePattern[] MethodGenericParameters { get; set; }

        public TypePattern[] MethodParameters { get; set; }
    }
}
