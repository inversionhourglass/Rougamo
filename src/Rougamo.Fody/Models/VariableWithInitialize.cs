using Mono.Cecil.Cil;

namespace Rougamo.Fody.Models
{
    internal sealed class VariableWithInitialize
    {
        public VariableDefinition Variable { get; set; }

        public Instruction[] Instructions { get; set; }
    }
}
