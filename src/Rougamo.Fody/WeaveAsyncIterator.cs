using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void AsyncIteratorMethodWeave(RouMethod rouMethod)
        {
            // todo: IteratorMethodWeave
            //rouMethod.MethodDef.Body.OptimizePlus();
        }
    }
}
