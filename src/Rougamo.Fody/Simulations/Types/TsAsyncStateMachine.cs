using Mono.Cecil;
using Rougamo.Fody.Enhances.Async;
using System.Linq;

namespace Rougamo.Fody.Simulations.Types
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal class TsAsyncStateMachine(TypeReference typeRef, IHost? host, ModuleWeaver moduleWeaver) : TsStateMachine(typeRef, host, moduleWeaver), IAsyncStateMachine
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        private FieldSimulation<TsAsyncBuilder> _fBuilder;

        public FieldSimulation<TsAsyncBuilder> F_Builder => _fBuilder ??= FieldSimulate<TsAsyncBuilder>(Constants.FIELD_Builder);

        public FieldSimulation<TsAwaiter> F_Awaiter { get; private set; }

        public FieldSimulation<TsAwaiter> F_MoAwaiter { get; private set; }

        public FieldSimulation? F_Result { get; private set; }

        public IAsyncBuilder FV_Builder => F_Builder.Value;

        public MethodSimulation M_SetStateMachine => MethodSimulate(Constants.METHOD_SetStateMachine, false);

        public TsAsyncStateMachine SetFields(AsyncFields fields)
        {
            F_MoArray = fields.MoArray?.Simulate<TsArray<TsMo>>(this);
            F_Mos = fields.Mos.Select(x => x.Simulate<TsMo>(this)).ToArray();
            F_MethodContext = fields.MethodContext.Simulate<TsMethodContext>(this);
            F_State = fields.State.Simulate(this);
            _fBuilder = fields.Builder.Simulate<TsAsyncBuilder>(this);
            F_Parameters = fields.Parameters.Select(x => x!.Simulate(this)).ToArray();
            F_DeclaringThis = fields.DeclaringThis?.Simulate<TsWeavingTarget>(this);
            F_Awaiter = fields.Awaiter.Simulate<TsAwaiter>(this);
            F_MoAwaiter = fields.MoAwaiter.Simulate<TsAwaiter>(this);
            F_Result = fields.Result?.Simulate(this);

            return this;
        }
    }
}
