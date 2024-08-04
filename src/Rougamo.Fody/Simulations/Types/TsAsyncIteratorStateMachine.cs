using Mono.Cecil;
using Rougamo.Fody.Enhances.AsyncIterator;
using System.Linq;

namespace Rougamo.Fody.Simulations.Types
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal class TsAsyncIteratorStateMachine(TypeReference typeRef, IHost? host, ModuleWeaver moduleWeaver) : TsStateMachine(typeRef, host, moduleWeaver), IIteratorStateMachine, IAsyncStateMachine
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        public FieldSimulation F_Disposed { get; private set; }

        public FieldSimulation F_Current { get; private set; }

        public FieldSimulation<TsList>? F_Items { get; private set; }

        public FieldSimulation<TsAsyncEnumerator> F_Iterator { get; private set; }

        public FieldSimulation<TsAsyncIteratorMethodBuilder> F_Builder { get; private set; }

        public FieldSimulation<TsManualResetValueTaskSourceCore> F_Promise { get; private set; }

        public FieldSimulation<TsAwaiter> F_Awaiter { get; private set; }

        public FieldSimulation<TsAwaiter> F_MoAwaiter { get; private set; }

        public IAsyncBuilder FV_Builder => F_Builder.Value;

        public TsAsyncIteratorStateMachine SetFields(AiteratorFields fields)
        {
            F_Disposed = fields.Disposed.Simulate(this);
            F_MoArray = fields.MoArray?.Simulate<TsArray<TsMo>>(this);
            F_Mos = fields.Mos.Select(x => x.Simulate<TsMo>(this)).ToArray();
            F_MethodContext = fields.MethodContext.Simulate<TsMethodContext>(this);
            F_State = fields.State.Simulate(this);
            F_Parameters = fields.Parameters.Select(x => x!.Simulate(this)).ToArray();
            F_DeclaringThis = fields.DeclaringThis?.Simulate<TsWeavingTarget>(this);
            F_Current = fields.Current.Simulate(this);
            F_Items = fields.RecordedReturn?.Simulate<TsList>(this);
            F_Iterator = fields.Iterator.Simulate<TsAsyncEnumerator>(this);
            F_Builder = fields.Builder.Simulate<TsAsyncIteratorMethodBuilder>(this);
            F_Promise = fields.Promise.Simulate<TsManualResetValueTaskSourceCore>(this);
            F_Awaiter = fields.Awaiter.Simulate<TsAwaiter>(this);
            F_MoAwaiter = fields.MoAwaiter.Simulate<TsAwaiter>(this);

            return this;
        }
    }
}
