using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void AsyncIteratorMethodWeave(RouMethod rouMethod)
        {
            IteratorInit(rouMethod, Constants.TYPE_AsyncIteratorStateMachineAttribute, true, out var stateTypeDef, out var mosFieldDef, out var contextFieldDef, out var returnsFieldDef);
            var moveNextMethodDef = stateTypeDef.Methods.Single(m => m.Name == Constants.METHOD_MoveNext);
            var stateFieldDef = stateTypeDef.Fields.Single(x => x.Name == "<>1__state");
            FieldReference mosFieldRef = mosFieldDef;
            FieldReference contextFieldRef = contextFieldDef;
            FieldReference returnsFieldRef = returnsFieldDef;
            FieldReference stateFieldRef = stateFieldDef;
            FieldReference currentFieldRef = stateTypeDef.Fields.Single(m => m.Name.EndsWith("current"));
            if (stateTypeDef.HasGenericParameters)
            {
                // generic return type will get in
                // public IEnumerable<MyClass<T>> Mt<T>()
                var stateTypeRef = new GenericInstanceType(stateTypeDef);
                foreach (var parameter in stateTypeDef.GenericParameters)
                {
                    stateTypeRef.GenericArguments.Add(parameter);
                }
                mosFieldRef = new FieldReference(mosFieldDef.Name, mosFieldDef.FieldType, stateTypeRef);
                contextFieldRef = new FieldReference(contextFieldDef.Name, contextFieldDef.FieldType, stateTypeRef);
                returnsFieldRef = returnsFieldRef == null ? null : new FieldReference(returnsFieldDef.Name, returnsFieldDef.FieldType, stateTypeRef);
                stateFieldRef = new FieldReference(stateFieldDef.Name, stateFieldDef.FieldType, stateTypeRef);
                currentFieldRef = new FieldReference(currentFieldRef.Name, currentFieldRef.FieldType, stateTypeRef);
            }
            IteratorOnEntry(rouMethod, moveNextMethodDef, mosFieldRef, contextFieldRef, stateFieldRef);
            AsyncOnExceptionWithExit(rouMethod, moveNextMethodDef, mosFieldRef, contextFieldRef, null, _typeVoidRef, null, false);
            AsyncIteratorOnSuccessWithExit(rouMethod, moveNextMethodDef, mosFieldRef, contextFieldRef, returnsFieldRef, currentFieldRef);

            rouMethod.MethodDef.Body.OptimizePlus();
            moveNextMethodDef.Body.OptimizePlus();
        }

        private void AsyncIteratorOnSuccessWithExit(RouMethod rouMethod, MethodDefinition moveNextMethodDef, FieldReference mosFieldRef, FieldReference contextFieldRef, FieldReference returnsFieldRef, FieldReference currentFieldRef)
        {
            var instructions = moveNextMethodDef.Body.Instructions;
            var setResults = instructions.Where(x => (x.OpCode.Code == Code.Call || x.OpCode.Code == Code.Callvirt) && x.Operand is MethodReference methodRef && methodRef.DeclaringType.FullName == "System.Threading.Tasks.Sources.ManualResetValueTaskSourceCore`1<System.Boolean>" && methodRef.Name == Constants.METHOD_SetResult).ToArray();
            foreach (var setResult in setResults)
            {
                switch (setResult.Previous.OpCode.Code)
                {
                    case Code.Ldc_I4_0: // finished
                        ExecuteMoMethod(Constants.METHOD_OnExit, moveNextMethodDef, rouMethod.Mos.Count, null, mosFieldRef, contextFieldRef, setResult.Next, this.ConfigReverseCallEnding());
                        ExecuteMoMethod(Constants.METHOD_OnSuccess, moveNextMethodDef, rouMethod.Mos.Count, null, mosFieldRef, contextFieldRef, setResult.Next, this.ConfigReverseCallEnding());
                        if(returnsFieldRef != null)
                        {
                            var listToArrayMethodRef = returnsFieldRef.FieldType.GenericTypeMethodReference(_methodListToArrayRef, ModuleDefinition);
                            instructions.InsertAfter(setResult, new[]
                            {
                                Instruction.Create(OpCodes.Ldarg_0),
                                Instruction.Create(OpCodes.Ldfld, contextFieldRef),
                                Instruction.Create(OpCodes.Ldarg_0),
                                Instruction.Create(OpCodes.Ldfld, returnsFieldRef),
                                Instruction.Create(OpCodes.Callvirt, listToArrayMethodRef),
                                Instruction.Create(OpCodes.Callvirt, _methodMethodContextSetReturnValueRef)
                            });
                        }
                        break;
                    case Code.Ldc_I4_1: // yield
                        if(returnsFieldRef != null)
                        {
                            var listAddMethodRef = returnsFieldRef.FieldType.GenericTypeMethodReference(_methodListAddRef, ModuleDefinition);
                            instructions.InsertAfter(setResult, new[]
                            {
                                Instruction.Create(OpCodes.Ldarg_0),
                                Instruction.Create(OpCodes.Ldfld, returnsFieldRef),
                                Instruction.Create(OpCodes.Ldarg_0),
                                Instruction.Create(OpCodes.Ldfld, currentFieldRef),
                                Instruction.Create(OpCodes.Callvirt, listAddMethodRef)
                            });
                        }
                        break;
                    default:
                        throw new RougamoException($"before SetResult is not ldc.i4.0 or ldc.i4.1, it is {setResult.OpCode.Code}");
                }
            }
        }
    }
}
