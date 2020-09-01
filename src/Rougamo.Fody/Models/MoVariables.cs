using Mono.Cecil.Cil;

namespace Rougamo.Fody
{
    internal sealed class MoVariables
    {
        public VariableDefinition[] Mos { get; set; }
        
        public VariableDefinition EntryContext { get; set; }
        
        public VariableDefinition SuccessContext { get; set; }
        
        public VariableDefinition ExceptionContext { get; set; }

        public VariableDefinition ExitContext { get; set; }

        public VariableDefinition Arguments { get; set; }
    }
}
