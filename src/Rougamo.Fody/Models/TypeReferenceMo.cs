using Fody.Simulations;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Rougamo.Fody.Simulations.Types;
using System.Collections.Generic;

namespace Rougamo.Fody.Models
{
    internal class TypeReferenceMo(TypeReference moTypeRef, MoFrom from) : Mo(moTypeRef, from)
    {
        public override bool IsStruct => MoTypeDef.IsValueType;

        public override bool HasArguments => false;

        public override bool HasProperties => false;

        public override IList<Instruction> New(TsMo tMo, MethodSimulation host)
        {
            if (IsStruct)
            {
                var vThis = host.CreateVariable<TsMo>(tMo);
                return [.. vThis.AssignNew(), .. vThis.Load()];
            }

            return tMo.New(host);
        }
    }
}
