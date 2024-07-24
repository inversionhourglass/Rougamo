using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Rougamo.Fody.Simulations.PlainValues;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody.Simulations.Types
{
    internal class TsMo(TypeReference typeRef, IHost? host, BaseModuleWeaver moduldeWeaver) : TypeSimulation(typeRef, host, moduldeWeaver)
    {
        public PropertySimulation P_IsCompleted => PropertySimulate(Constants.PROP_IsCompleted, true);

        public MethodSimulation M_OnEntry => MethodSimulate(Constants.METHOD_OnEntry, true);

        public MethodSimulation M_OnException => MethodSimulate(Constants.METHOD_OnException, true);

        public MethodSimulation M_OnSuccess => MethodSimulate(Constants.METHOD_OnSuccess, true);

        public MethodSimulation M_OnExit => MethodSimulate(Constants.METHOD_OnExit, true);

        public MethodSimulation<TsAsyncable> M_OnEntryAsync => MethodSimulate<TsAsyncable>(Constants.METHOD_OnEntryAsync, true);

        public MethodSimulation<TsAsyncable> M_OnExceptionAsync => MethodSimulate<TsAsyncable>(Constants.METHOD_OnExceptionAsync, true);

        public MethodSimulation<TsAsyncable> M_OnSuccessAsync => MethodSimulate<TsAsyncable>(Constants.METHOD_OnSuccessAsync, true);

        public MethodSimulation<TsAsyncable> M_OnExitAsync => MethodSimulate<TsAsyncable>(Constants.METHOD_OnExitAsync, true);

        public IList<Instruction> New(MethodSimulation host, Mo mo, MethodSimulation executingMethod)
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
                var vThis = executingMethod.CreateVariable<TsMo>(this);
                return [.. vThis.AssignNew(), .. vThis.Load()];
            }

            return New(host);
        }
    }
}
