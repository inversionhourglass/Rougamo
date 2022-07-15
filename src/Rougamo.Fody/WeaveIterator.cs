using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.Runtime.CompilerServices;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void IteratorMethodWeave(RouMethod rouMethod)
        {
            IteratorInit(rouMethod, Constants.TYPE_IteratorStateMachineAttribute, false, out var stateTypeDef, out var mosFieldDef, out var contextFieldDef, out var returnsFieldDef);
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
            // finallyEnd may be null
            GenerateTryCatchFinally(moveNextMethodDef, out var tryStart, out var catchStart, out var finallyStart, out var finallyEnd, out var rethrow, out var exceptionVariable, out var returnVariable);
            if (returnVariable == null) throw new RougamoException("iterator MoveNext should not return void");
            if (finallyEnd == null) throw new RougamoException("iterator MoveNext finally should not be null");
            SetTryCatchFinally(moveNextMethodDef, tryStart, catchStart, finallyStart, finallyEnd);
            IteratorOnEntry(rouMethod, moveNextMethodDef, mosFieldRef, contextFieldRef, stateFieldRef);
            OnExceptionFromField(moveNextMethodDef, rouMethod.Mos.Count, mosFieldRef, contextFieldRef, exceptionVariable, catchStart);
            IteratorOnExitOrMoveNext(moveNextMethodDef, rouMethod.Mos.Count, mosFieldRef, contextFieldRef, returnsFieldRef, currentFieldRef, returnVariable, finallyStart, finallyEnd);

            rouMethod.MethodDef.Body.OptimizePlus();
            moveNextMethodDef.Body.OptimizePlus();
        }

        private TypeReference IteratorInit(RouMethod rouMethod, string stateMachineName, bool isAsync, out TypeDefinition stateTypeDef, out FieldDefinition mosFieldDef, out FieldDefinition contextFieldDef, out FieldDefinition? returnsFieldDef)
        {
            IteratorFieldDefinition(rouMethod, stateMachineName, out stateTypeDef, out mosFieldDef, out contextFieldDef, out returnsFieldDef);
            var stateMachineTypeRef = IteratorGetEntryStateTypeReference(rouMethod.MethodDef.Body);
            var stateMachineVariable = rouMethod.MethodDef.Body.CreateVariable(stateMachineTypeRef);
            var retIns = rouMethod.MethodDef.Body.Instructions.Last();
            rouMethod.MethodDef.Body.Instructions.InsertBefore(retIns, Create(OpCodes.Stloc, stateMachineVariable));
            InitReturnsField(rouMethod, returnsFieldDef, stateMachineTypeRef, stateMachineVariable, retIns);
            StateMachineInitMosField(rouMethod, mosFieldDef, stateMachineVariable, retIns);
            StateMachineInitMethodContextField(rouMethod, contextFieldDef, stateMachineVariable, retIns, isAsync, true);
            rouMethod.MethodDef.Body.Instructions.InsertBefore(retIns, Create(OpCodes.Ldloc, stateMachineVariable));
            var returnType = rouMethod.MethodDef.ReturnType;
            return ((GenericInstanceType)returnType).GenericArguments[0];
        }

        private void IteratorOnEntry(RouMethod rouMethod, MethodDefinition moveNextMethodDef, FieldReference mosFieldRef, FieldReference contextFieldRef, FieldReference stateFieldRef)
        {
            var instructions = moveNextMethodDef.Body.Instructions;
            var originFirstIns = instructions.First();
            instructions.Insert(0, new[]
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, stateFieldRef),
                Create(OpCodes.Ldc_I4, -1),
                Create(OpCodes.Bne_Un, originFirstIns)
            });
            ExecuteMoMethod(Constants.METHOD_OnEntry, moveNextMethodDef, rouMethod.Mos.Count, null, mosFieldRef, contextFieldRef, originFirstIns, this.ConfigReverseCallEnding());
        }

        private void IteratorOnExitOrMoveNext(MethodDefinition methodDef, int mosCount, FieldReference mosFieldRef, FieldReference contextFieldRef, FieldReference? returnsFieldRef, FieldReference currentFieldRef, VariableDefinition returnVariable, Instruction finallyStart, Instruction finallyEnd)
        {
            var endFinally = finallyEnd.Previous;
            var yieldBrTo = endFinally;
            if (returnsFieldRef != null)
            {
                var listAddMethodRef = returnsFieldRef.FieldType.GenericTypeMethodReference(_methodListAddRef, ModuleDefinition);
                yieldBrTo = Create(OpCodes.Ldarg_0);
                var addCurrentToReturns = new[]
                {
                    yieldBrTo,
                    Create(OpCodes.Ldfld, returnsFieldRef),
                    Create(OpCodes.Ldarg_0),
                    Create(OpCodes.Ldfld, currentFieldRef),
                    Create(OpCodes.Callvirt, listAddMethodRef)
                };
                methodDef.Body.Instructions.InsertBefore(endFinally, addCurrentToReturns);
            }

            ExecuteMoMethod(Constants.METHOD_OnExit, methodDef, mosCount, null, mosFieldRef, contextFieldRef, yieldBrTo, this.ConfigReverseCallEnding());
            if (returnsFieldRef != null)
            {
                methodDef.Body.Instructions.InsertBefore(yieldBrTo, Create(OpCodes.Br_S, endFinally));
            }
            if (finallyStart.OpCode.Code != Code.Nop) throw new RougamoException("finally start is not nop");
            finallyStart.OpCode = OpCodes.Ldloc;
            finallyStart.Operand = returnVariable;
            var onExitStart = finallyStart.Next;
            var hasException = Create(OpCodes.Brtrue_S, onExitStart);
            methodDef.Body.Instructions.InsertAfter(finallyStart, new[]
            {
                Create(OpCodes.Brtrue_S, yieldBrTo),
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, contextFieldRef),
                Create(OpCodes.Callvirt, _methodMethodContextGetHasExceptionRef),
                hasException
            });
            if (returnsFieldRef != null)
            {
                var listToArrayMethodRef = returnsFieldRef.FieldType.GenericTypeMethodReference(_methodListToArrayRef, ModuleDefinition);
                methodDef.Body.Instructions.InsertAfter(hasException, new[]
                {
                    Create(OpCodes.Ldarg_0),
                    Create(OpCodes.Ldfld, contextFieldRef),
                    Create(OpCodes.Ldarg_0),
                    Create(OpCodes.Ldfld, returnsFieldRef),
                    Create(OpCodes.Callvirt, listToArrayMethodRef),
                    Create(OpCodes.Callvirt, _methodMethodContextSetReturnValueRef)
                });
            }
            ExecuteMoMethod(Constants.METHOD_OnSuccess, methodDef, mosCount, null, mosFieldRef, contextFieldRef, onExitStart, this.ConfigReverseCallEnding());
        }

        private void IteratorFieldDefinition(RouMethod rouMethod, string stateMachineName, out TypeDefinition stateTypeDef, out FieldDefinition mosFieldDef, out FieldDefinition contextFieldDef, out FieldDefinition? returnFieldDef)
        {
            StateMachineFieldDefinition(rouMethod, stateMachineName, out stateTypeDef, out mosFieldDef, out contextFieldDef);

            returnFieldDef = null;
            if (this.ConfigRecordingIteratorReturnValue())
            {
                var listReturnsRef = new GenericInstanceType(_typeListRef);
                listReturnsRef.GenericArguments.Add(((GenericInstanceType)rouMethod.MethodDef.ReturnType).GenericArguments[0]);
                returnFieldDef = new FieldDefinition(Constants.FIELD_IteratorReturnList, FieldAttributes.Public, listReturnsRef);
                stateTypeDef.Fields.Add(returnFieldDef);
            }
        }

        private void InitReturnsField(RouMethod rouMethod, FieldDefinition? returnsFieldDef, TypeReference stateMachineTypeRef, VariableDefinition stateMachineVariable, Instruction insertBeforeThisIns)
        {
            if (returnsFieldDef != null)
            {
                var instructions = rouMethod.MethodDef.Body.Instructions;
                var returnsFieldRef = new FieldReference(returnsFieldDef.Name, returnsFieldDef.FieldType, stateMachineTypeRef);
                var returnsTypeCtorDef = _typeListDef.GetZeroArgsCtor();
                var returnsTypeCtorRef = returnsFieldDef.FieldType.GenericTypeMethodReference(returnsTypeCtorDef, ModuleDefinition);
                instructions.InsertBefore(insertBeforeThisIns, stateMachineVariable.LdlocOrA());
                instructions.InsertBefore(insertBeforeThisIns, Create(OpCodes.Newobj, returnsTypeCtorRef));
                instructions.InsertBefore(insertBeforeThisIns, Create(OpCodes.Stfld, returnsFieldRef));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TypeReference IteratorGetEntryStateTypeReference(MethodBody body)
        {
            return ((MethodReference)body.Instructions.Single(x => x.OpCode.Code == Code.Newobj).Operand).DeclaringType;
        }
    }
}
