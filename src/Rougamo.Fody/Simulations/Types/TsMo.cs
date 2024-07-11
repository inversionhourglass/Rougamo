using Mono.Cecil;
using Mono.Cecil.Cil;
using Rougamo.Fody.Simulations.Parameters;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody.Simulations.Types
{
    internal class TsMo(TypeReference typeRef, IHost? host, ModuleDefinition moduldeDef) : TypeSimulation(typeRef, host, moduldeDef)
    {
        public PropertySimulation P_IsCompleted => PropertySimulate(Constants.PROP_IsCompleted, true);

        public MethodSimulation M_OnEntry => MethodSimulate(Constants.METHOD_OnEntry);

        public MethodSimulation M_OnException => MethodSimulate(Constants.METHOD_OnException);

        public MethodSimulation M_OnSuccess => MethodSimulate(Constants.METHOD_OnSuccess);

        public MethodSimulation M_OnExit => MethodSimulate(Constants.METHOD_OnExit);

        public MethodSimulation<TsAsyncable> M_OnEntryAsync => MethodSimulate<TsAsyncable>(Constants.METHOD_OnEntryAsync);

        public MethodSimulation<TsAsyncable> M_OnExceptionAsync => MethodSimulate<TsAsyncable>(Constants.METHOD_OnExceptionAsync);

        public MethodSimulation<TsAsyncable> M_OnSuccessAsync => MethodSimulate<TsAsyncable>(Constants.METHOD_OnSuccessAsync);

        public MethodSimulation<TsAsyncable> M_OnExitAsync => MethodSimulate<TsAsyncable>(Constants.METHOD_OnExitAsync);

        public IList<Instruction> New(Mo mo, MethodSimulation executingMethod)
        {
            if (mo.Attribute != null)
            {
                var ctor = mo.Attribute.Constructor.Simulate(this);
                var arguments = mo.Attribute.ConstructorArguments.Select(x => x.Simulate(Module)).ToArray();
                var instructions = new List<Instruction>
                {
                    ctor.Call(null, arguments)
                };
                if (mo.Attribute.HasProperties)
                {
                    foreach (var property in mo.Attribute.Properties)
                    {
                        var propSimulation = OptionalPropertySimulate(property.Name, true);
                        if (propSimulation?.Setter != null)
                        {
                            var propValue = new ObjectValue(property.Argument.Value, property.Argument.Type, Module);
                            instructions.Add(propSimulation.Setter.DupCall(null, propValue));
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

            return New();
        }
    }
}
