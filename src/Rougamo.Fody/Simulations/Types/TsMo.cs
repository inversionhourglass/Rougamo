using Fody;
using Fody.Simulations;
using Fody.Simulations.PlainValues;
using Fody.Simulations.Types;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody.Simulations.Types
{
    internal class TsMo(TypeReference typeRef, IHost? host, SimulationModuleWeaver moduldeWeaver) : TypeSimulation(typeRef, host, moduldeWeaver)
    {
        public PropertySimulation P_IsCompleted => PropertySimulate(Constants.PROP_IsCompleted, true);

        public PropertySimulation P_Features => PropertySimulate(Constants.PROP_Features, true);

        public MethodSimulation M_OnEntry => MethodSimulate(Constants.METHOD_OnEntry, true);

        public MethodSimulation M_OnException => MethodSimulate(Constants.METHOD_OnException, true);

        public MethodSimulation M_OnSuccess => MethodSimulate(Constants.METHOD_OnSuccess, true);

        public MethodSimulation M_OnExit => MethodSimulate(Constants.METHOD_OnExit, true);

        public MethodSimulation<TsAwaitable> M_OnEntryAsync => MethodSimulate<TsAwaitable>(Constants.METHOD_OnEntryAsync, true);

        public MethodSimulation<TsAwaitable> M_OnExceptionAsync => MethodSimulate<TsAwaitable>(Constants.METHOD_OnExceptionAsync, true);

        public MethodSimulation<TsAwaitable> M_OnSuccessAsync => MethodSimulate<TsAwaitable>(Constants.METHOD_OnSuccessAsync, true);

        public MethodSimulation<TsAwaitable> M_OnExitAsync => MethodSimulate<TsAwaitable>(Constants.METHOD_OnExitAsync, true);

        public IList<Instruction> New(MethodSimulation host, Mo mo)
        {
            if (mo.Attribute != null)
            {
                var ctor = mo.Attribute.Constructor.Simulate(this);
                var arguments = mo.Attribute.ConstructorArguments.Select(x => x.Simulate(ModuleWeaver)).ToArray();
                var instructions = new List<Instruction>
                {
                    ctor.Call(host, arguments)
                };
                if (mo.Attribute.HasProperties)
                {
                    foreach (var property in mo.Attribute.Properties)
                    {
                        var propSimulation = OptionalPropertySimulate(property.Name, true);
                        if (propSimulation?.Setter != null)
                        {
                            var propValue = new ObjectValue(property.Argument.Value, property.Argument.Type, ModuleWeaver);
                            instructions.Add(propSimulation.Setter.DupCall(host, propValue));
                        }
                    }
                }

                return instructions;
            }

            if (mo.IsStruct)
            {
                var vThis = host.CreateVariable<TsMo>(this);
                return [.. vThis.AssignNew(), .. vThis.Load()];
            }

            return New(host);
        }
    }
}
