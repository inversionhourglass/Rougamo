using Mono.Cecil;
using Mono.Cecil.Cil;
using Rougamo.Fody.Enhances;
using Rougamo.Fody.Enhances.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void StrictAsyncTaskMethodWeave(RouMethod rouMethod, TypeDefinition stateMachineTypeDef)
        {
            var typeName = $"$Rougamo_{stateMachineTypeDef.Name}";
            if (stateMachineTypeDef.Module.Types.Any(x => x.Namespace == stateMachineTypeDef.Namespace && x.Name == typeName)) return;

            var actualTypeDef = stateMachineTypeDef.Clone(typeName);
            actualTypeDef.DeclaringType.NestedTypes.Add(actualTypeDef);
        }
    }
}
