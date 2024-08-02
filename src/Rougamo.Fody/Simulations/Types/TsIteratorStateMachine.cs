using Mono.Cecil;
using Rougamo.Fody.Enhances.Iterator;
using System.Linq;

namespace Rougamo.Fody.Simulations.Types
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal class TsIteratorStateMachine(TypeReference typeRef, IHost? host, ModuleWeaver moduleWeaver) : TsStateMachine(typeRef, host, moduleWeaver), IIteratorStateMachine
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        public FieldSimulation F_Current { get; private set; }

        public FieldSimulation<TsList>? F_Items { get; private set; }

        public FieldSimulation<TsEnumerator> F_Iterator { get; private set; }

        public TsIteratorStateMachine SetFields(IteratorFields fields)
        {
            F_MoArray = fields.MoArray?.Resolve().Simulate<TsArray<TsMo>>(this);
            F_Mos = fields.Mos.Select(x => x.Resolve().Simulate<TsMo>(this)).ToArray();
            F_MethodContext = fields.MethodContext.Resolve().Simulate<TsMethodContext>(this);
            F_State = fields.State.Resolve().Simulate(this);
            F_Parameters = fields.Parameters.Select(x => x!.Resolve().Simulate(this)).ToArray();
            F_DeclaringThis = fields.DeclaringThis?.Resolve().Simulate<TsWeavingTarget>(this);
            F_Current = fields.Current.Resolve().Simulate(this);
            F_Items = fields.RecordedReturn?.Resolve().Simulate<TsList>(this);
            F_Iterator = fields.Iterator.Resolve().Simulate<TsEnumerator>(this);

            return this;
        }
    }
}
