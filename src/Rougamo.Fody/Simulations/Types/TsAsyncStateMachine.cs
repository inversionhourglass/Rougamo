using Mono.Cecil;
using Rougamo.Fody.Enhances.Async;
using System.Linq;

namespace Rougamo.Fody.Simulations.Types
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal class TsAsyncStateMachine(TypeReference typeRef, IHost? host, ModuleWeaver moduleWeaver) : TypeSimulation(typeRef, host, moduleWeaver)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        public FieldSimulation? F_MoArray { get; private set; }

        public FieldSimulation<TsMo>[]? F_Mos { get; private set; }

        public FieldSimulation<TsMethodContext> F_MethodContext { get; private set; }

        public FieldSimulation F_State { get; private set; }

        public FieldSimulation<TsAsyncBuilder> F_Builder { get; private set; }

        public FieldSimulation[] F_Parameters { get; private set; }

        public FieldSimulation<TsWeavingTarget>? F_DeclaringThis { get; private set; }

        public FieldSimulation<TsAwaiter> F_Awaiter { get; private set; }

        public FieldSimulation<TsAwaiter> F_MoAwaiter { get; private set; }

        public FieldSimulation? F_Result { get; private set; }

        public MethodSimulation M_MoveNext => MethodSimulate(Constants.METHOD_MoveNext, false);

        public TsAsyncStateMachine SetFields(AsyncFields fields)
        {
            F_MoArray = fields.MoArray?.Resolve().Simulate(this);
            F_Mos = fields.Mos.Select(x => x.Resolve().Simulate<TsMo>(this)).ToArray();
            F_MethodContext = fields.MethodContext.Resolve().Simulate<TsMethodContext>(this);
            F_State = fields.State.Resolve().Simulate(this);
            F_Builder = fields.Builder.Resolve().Simulate<TsAsyncBuilder>(this);
            F_Parameters = fields.Parameters.Select(x => x!.Resolve().Simulate(this)).ToArray();
            F_DeclaringThis = fields.DeclaringThis?.Resolve().Simulate<TsWeavingTarget>(this);
            F_Awaiter = fields.Awaiter.Resolve().Simulate<TsAwaiter>(this);
            F_MoAwaiter = fields.MoAwaiter.Resolve().Simulate<TsAwaiter>(this);
            F_Result = fields.Result?.Resolve().Simulate(this);

            return this;
        }
    }
}
