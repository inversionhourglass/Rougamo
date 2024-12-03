using Fody;
using Fody.Simulations;
using Fody.Simulations.Types;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations.Types
{
    internal class TsMo(TypeReference typeRef, IHost? host, SimulationModuleWeaver moduldeWeaver) : TypeSimulation(typeRef, host, moduldeWeaver)
    {
        private readonly Dictionary<string, MethodSimulation> _aspectMethodSimulations = [];

        public PropertySimulation P_IsCompleted => PropertySimulate(Constants.PROP_IsCompleted, true);

        public PropertySimulation P_Features => PropertySimulate(Constants.PROP_Features, true);

        public MethodSimulation M_OnEntry => AspectMethodSimulate(Constants.METHOD_OnEntry);

        public MethodSimulation M_OnException => AspectMethodSimulate(Constants.METHOD_OnException);

        public MethodSimulation M_OnSuccess => AspectMethodSimulate(Constants.METHOD_OnSuccess);

        public MethodSimulation M_OnExit => AspectMethodSimulate(Constants.METHOD_OnExit);

        public MethodSimulation<TsAwaitable> M_OnEntryAsync => AsyncAspectMethodSimulate<TsAwaitable>(Constants.METHOD_OnEntryAsync);

        public MethodSimulation<TsAwaitable> M_OnExceptionAsync => AsyncAspectMethodSimulate<TsAwaitable>(Constants.METHOD_OnExceptionAsync);

        public MethodSimulation<TsAwaitable> M_OnSuccessAsync => AsyncAspectMethodSimulate<TsAwaitable>(Constants.METHOD_OnSuccessAsync);

        public MethodSimulation<TsAwaitable> M_OnExitAsync => AsyncAspectMethodSimulate<TsAwaitable>(Constants.METHOD_OnExitAsync);

        public MethodSimulation M_Singleton => MethodSimulate(Constants.METHOD__Singleton, false, x => x.Name == Constants.METHOD__Singleton && x.IsStatic);

        public IList<Instruction> New(MethodSimulation host, Mo mo) => mo.New(this, host);

        private MethodSimulation AspectMethodSimulate(string name)
        {
            if (!_aspectMethodSimulations.TryGetValue(name, out var simulation))
            {
                var tdIAsyncMo = Def.GetInterfaceDefinition(Constants.TYPE_IAsyncMo);
                var methodDef = tdIAsyncMo == null ? Def.GetMethod(name, true) : tdIAsyncMo.GetMethod(name, false);
                simulation = methodDef.Simulate(this, tdIAsyncMo != null);
                _aspectMethodSimulations[name] = simulation;
            }
            return simulation;
        }

        private MethodSimulation<TRet> AsyncAspectMethodSimulate<TRet>(string name) where TRet : TypeSimulation
        {
            if (!_aspectMethodSimulations.TryGetValue(name, out var simulation))
            {
                var tdISyncMo = Def.GetInterfaceDefinition(Constants.TYPE_ISyncMo);
                var methodDef = tdISyncMo == null ? Def.GetMethod(name, true) : tdISyncMo.GetMethod(name, false);
                simulation = methodDef.Simulate<TRet>(this, tdISyncMo != null);
                _aspectMethodSimulations[name] = simulation;
            }
            return (MethodSimulation<TRet>)simulation;
        }
    }
}
