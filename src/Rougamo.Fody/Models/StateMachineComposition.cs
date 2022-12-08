using Mono.Cecil;

namespace Rougamo.Fody.Models
{
    internal class StateMachineComposition
    {
        public RouMethod RouMethod { get; set; }

        public MethodDefinition MoveNextMethodDef { get; set; }

        public FieldReference BuilderFieldRef { get; set; }

        public FieldReference StateFieldRef { get; set; }

        public FieldReference MosFieldRef { get; set; }

        public FieldReference ContextFieldRef { get; set; }
    }
}
