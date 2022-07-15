using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using static Mono.Cecil.Cil.Instruction;

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
            FieldReference? returnsFieldRef = returnsFieldDef;
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
                returnsFieldRef = returnsFieldRef == null ? null : new FieldReference(returnsFieldRef.Name, returnsFieldRef.FieldType, stateTypeRef);
                stateFieldRef = new FieldReference(stateFieldDef.Name, stateFieldDef.FieldType, stateTypeRef);
                currentFieldRef = new FieldReference(currentFieldRef.Name, currentFieldRef.FieldType, stateTypeRef);
            }
            IteratorOnEntry(rouMethod, moveNextMethodDef, mosFieldRef, contextFieldRef, stateFieldRef);
            AsyncOnException(rouMethod, moveNextMethodDef, mosFieldRef, contextFieldRef, out var setExceptionFirst, out _);
            ExecuteMoMethod(Constants.METHOD_OnExit, moveNextMethodDef, rouMethod.Mos.Count, null, mosFieldRef, contextFieldRef, setExceptionFirst, this.ConfigReverseCallEnding());
            AsyncIteratorOnSuccessWithExit(rouMethod, moveNextMethodDef, mosFieldRef, contextFieldRef, returnsFieldRef, currentFieldRef);

            rouMethod.MethodDef.Body.OptimizePlus();
            moveNextMethodDef.Body.OptimizePlus();
        }

        private void AsyncIteratorOnSuccessWithExit(RouMethod rouMethod, MethodDefinition moveNextMethodDef, FieldReference mosFieldRef, FieldReference contextFieldRef, FieldReference? returnsFieldRef, FieldReference currentFieldRef)
        {
            var instructions = moveNextMethodDef.Body.Instructions;
            var setResults = instructions.Where(x => (x.OpCode.Code == Code.Call || x.OpCode.Code == Code.Callvirt) && x.Operand is MethodReference methodRef && methodRef.DeclaringType.FullName == "System.Threading.Tasks.Sources.ManualResetValueTaskSourceCore`1<System.Boolean>" && methodRef.Name == Constants.METHOD_SetResult).ToArray();
            foreach (var setResult in setResults)
            {
                var setResultLdarg0 = setResult.Previous.Previous.Previous;
                if (setResultLdarg0.OpCode.Code != Code.Ldarg_0) throw new RougamoException($"Offset {setResultLdarg0.Offset} of {rouMethod.MethodDef.FullName}'s MoveNext is {setResultLdarg0.OpCode.Code}, it should be Ldarg0 of SetResult which offset is {setResult.Offset}");

                switch (setResult.Previous.OpCode.Code)
                {
                    case Code.Ldc_I4_0: // finished
                        if(returnsFieldRef != null)
                        {
                            var listToArrayMethodRef = returnsFieldRef.FieldType.GenericTypeMethodReference(_methodListToArrayRef, ModuleDefinition);
                            instructions.InsertBefore(setResultLdarg0, new[]
                            {
                                Create(OpCodes.Ldarg_0),
                                Create(OpCodes.Ldfld, contextFieldRef),
                                Create(OpCodes.Ldarg_0),
                                Create(OpCodes.Ldfld, returnsFieldRef),
                                Create(OpCodes.Callvirt, listToArrayMethodRef),
                                Create(OpCodes.Callvirt, _methodMethodContextSetReturnValueRef)
                            });
                        }
                        var afterOnSuccessNop = instructions.InsertBefore(setResultLdarg0, Create(OpCodes.Nop));
                        ExecuteMoMethod(Constants.METHOD_OnSuccess, moveNextMethodDef, rouMethod.Mos.Count, null, mosFieldRef, contextFieldRef, afterOnSuccessNop, this.ConfigReverseCallEnding());
                        ExecuteMoMethod(Constants.METHOD_OnExit, moveNextMethodDef, rouMethod.Mos.Count, null, mosFieldRef, contextFieldRef, setResultLdarg0, this.ConfigReverseCallEnding());
                        break;
                    case Code.Ldc_I4_1: // yield
                        if(returnsFieldRef != null)
                        {
                            var listAddMethodRef = returnsFieldRef.FieldType.GenericTypeMethodReference(_methodListAddRef, ModuleDefinition);
                            instructions.InsertAfter(setResultLdarg0, new[]
                            {
                                Create(OpCodes.Ldfld, returnsFieldRef),
                                Create(OpCodes.Ldarg_0),
                                Create(OpCodes.Ldfld, currentFieldRef),
                                Create(OpCodes.Callvirt, listAddMethodRef),
                                Create(OpCodes.Ldarg_0)
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
