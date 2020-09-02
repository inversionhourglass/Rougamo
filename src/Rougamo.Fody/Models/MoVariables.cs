using Mono.Cecil.Cil;
using Rougamo.Fody.Models;

namespace Rougamo.Fody
{
    internal sealed class MoVariables
    {
        public VariableWithInitialize[] Mos { get; set; }
        
        public VariableDefinition EntryContext { get; set; }
        
        public VariableDefinition SuccessContext { get; set; }
        
        public VariableDefinition ExceptionContext { get; set; }

        public VariableDefinition ExitContext { get; set; }

        public VariableDefinition Arguments { get; set; }
    }
}
