using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void IteratorMethodWeave(RouMethod rouMethod)
        {
            IteratorOnEntry(rouMethod, out var stateTypeDef, out var mosFieldDef, out var contextFieldDef, out var returnsFieldDef);
            var moveNextMethodDef = stateTypeDef.Methods.Single(m => m.Name == Constants.METHOD_MoveNext);
            FieldReference mosFieldRef = mosFieldDef;
            FieldReference contextFieldRef = contextFieldDef;
            FieldReference returnsFieldRef = returnsFieldDef;
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
                currentFieldRef = new FieldReference(currentFieldRef.Name, currentFieldRef.FieldType, stateTypeRef);
            }
            // will never just throw
            GenerateTryCatchFinally(moveNextMethodDef, out var tryStart, out var catchStart, out var finallyStart, out var finallyEnd, out var exceptionVariable, out var returnVariable);
            SetTryCatchFinally(moveNextMethodDef, tryStart, catchStart, finallyStart, finallyEnd);
            OnExceptionFromField(moveNextMethodDef, rouMethod.Mos.Count, mosFieldRef, contextFieldRef, exceptionVariable, catchStart);
            IteratorOnExitOrMoveNext(moveNextMethodDef, rouMethod.Mos.Count, mosFieldRef, contextFieldRef, returnsFieldRef, currentFieldRef, returnVariable, finallyStart, finallyEnd);

            rouMethod.MethodDef.Body.OptimizePlus();
            moveNextMethodDef.Body.OptimizePlus();
        }

        private TypeReference IteratorOnEntry(RouMethod rouMethod, out TypeDefinition stateTypeDef, out FieldDefinition mosFieldDef, out FieldDefinition contextFieldDef, out FieldDefinition returnsFieldDef)
        {
            IteratorFieldDefinition(rouMethod, out stateTypeDef, out mosFieldDef, out contextFieldDef, out returnsFieldDef);
            var stateMachineTypeRef = IteratorGetEntryStateTypeReference(rouMethod.MethodDef.Body);
            var stateMachineVariable = rouMethod.MethodDef.Body.CreateVariable(stateMachineTypeRef);
            var mosFieldRef = new FieldReference(mosFieldDef.Name, mosFieldDef.FieldType, stateMachineTypeRef);
            var contextFieldRef = new FieldReference(contextFieldDef.Name, contextFieldDef.FieldType, stateMachineTypeRef);
            var retIns = rouMethod.MethodDef.Body.Instructions.Last();
            rouMethod.MethodDef.Body.Instructions.InsertBefore(retIns, Instruction.Create(OpCodes.Stloc, stateMachineVariable));
            InitReturnsField(rouMethod, returnsFieldDef, stateMachineTypeRef, stateMachineVariable, retIns);
            InitMosField(rouMethod, mosFieldRef, stateMachineVariable, retIns);
            InitMethodContextField(rouMethod, contextFieldRef, stateMachineVariable, retIns);
            rouMethod.MethodDef.Body.Instructions.InsertBefore(retIns, Instruction.Create(OpCodes.Ldloc, stateMachineVariable));
            ExecuteMoMethod(Constants.METHOD_OnEntry, rouMethod.MethodDef, rouMethod.Mos.Count, stateMachineVariable, mosFieldRef, contextFieldRef, retIns.Previous);
            var returnType = rouMethod.MethodDef.ReturnType;
            return ((GenericInstanceType)returnType).GenericArguments[0];
        }

        private void IteratorOnExitOrMoveNext(MethodDefinition methodDef, int mosCount, FieldReference mosFieldRef, FieldReference contextFieldRef, FieldReference returnsFieldRef, FieldReference currentFieldRef, VariableDefinition returnVariable, Instruction finallyStart, Instruction finallyScopeEnd)
        {
            var finallyEnd = finallyScopeEnd.Previous;
            var yieldBrTo = finallyEnd;
            if (returnsFieldRef != null)
            {
                var listAddMethodRef = returnsFieldRef.FieldType.GenericTypeMethodReference(_methodListAddRef, ModuleDefinition);
                yieldBrTo = Instruction.Create(OpCodes.Ldarg_0);
                var addCurrentToReturns = new[]
                {
                    yieldBrTo,
                    Instruction.Create(OpCodes.Ldfld, returnsFieldRef),
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldfld, currentFieldRef),
                    Instruction.Create(OpCodes.Callvirt, listAddMethodRef)
                };
                methodDef.Body.Instructions.InsertBefore(finallyEnd, addCurrentToReturns);
            }

            ExecuteMoMethod(Constants.METHOD_OnExit, methodDef, mosCount, null, mosFieldRef, contextFieldRef, yieldBrTo);
            if (returnsFieldRef != null)
            {
                methodDef.Body.Instructions.InsertBefore(yieldBrTo, Instruction.Create(OpCodes.Br_S, finallyEnd));
            }
            if (finallyStart.OpCode.Code != Code.Nop) throw new RougamoException("finally start is not nop");
            finallyStart.OpCode = OpCodes.Ldloc;
            finallyStart.Operand = returnVariable;
            var onExitStart = finallyStart.Next;
            var hasException = Instruction.Create(OpCodes.Brtrue_S, onExitStart);
            methodDef.Body.Instructions.InsertAfter(finallyStart, new[]
            {
                Instruction.Create(OpCodes.Brtrue_S, yieldBrTo),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, contextFieldRef),
                Instruction.Create(OpCodes.Callvirt, _methodMethodContextGetHasExceptionRef),
                hasException
            });
            if (returnsFieldRef != null)
            {
                var listToArrayMethodRef = returnsFieldRef.FieldType.GenericTypeMethodReference(_methodListToArrayRef, ModuleDefinition);
                methodDef.Body.Instructions.InsertAfter(hasException, new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldfld, contextFieldRef),
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldfld, returnsFieldRef),
                    Instruction.Create(OpCodes.Callvirt, listToArrayMethodRef),
                    Instruction.Create(OpCodes.Callvirt, _methodMethodContextSetReturnValueRef)
                });
            }
            ExecuteMoMethod(Constants.METHOD_OnSuccess, methodDef, mosCount, null, mosFieldRef, contextFieldRef, onExitStart);
        }

        private void IteratorFieldDefinition(RouMethod rouMethod, out TypeDefinition stateTypeDef, out FieldDefinition mosFieldDef, out FieldDefinition contextFieldDef, out FieldDefinition returnFieldDef)
        {
            stateTypeDef = rouMethod.MethodDef.ResolveIteratorStateMachine();
            var fieldAttributes = FieldAttributes.Public;
            mosFieldDef = new FieldDefinition(Constants.FIELD_RougamoMos, fieldAttributes, _typeIMoArrayRef);
            contextFieldDef = new FieldDefinition(Constants.FIELD_RougamoContext, fieldAttributes, _typeMethodContextRef);
            stateTypeDef.Fields.Add(mosFieldDef);
            stateTypeDef.Fields.Add(contextFieldDef);

            returnFieldDef = null;
            if (this.ConfigRecordingIteratorReturnValue())
            {
                var listReturnsRef = new GenericInstanceType(_typeListRef);
                listReturnsRef.GenericArguments.Add(((GenericInstanceType)rouMethod.MethodDef.ReturnType).GenericArguments[0]);
                returnFieldDef = new FieldDefinition(Constants.FIELD_IteratorReturnList, fieldAttributes, listReturnsRef);
                stateTypeDef.Fields.Add(returnFieldDef);
            }
        }

        private void InitReturnsField(RouMethod rouMethod, FieldDefinition returnsFieldDef, TypeReference stateMachineTypeRef, VariableDefinition stateMachineVariable, Instruction insertBeforeThisIns)
        {
            if (returnsFieldDef != null)
            {
                var instructions = rouMethod.MethodDef.Body.Instructions;
                var returnsFieldRef = new FieldReference(returnsFieldDef.Name, returnsFieldDef.FieldType, stateMachineTypeRef);
                var returnsTypeCtorDef = _typeListDef.GetZeroArgsCtor();
                var returnsTypeCtorRef = returnsFieldDef.FieldType.GenericTypeMethodReference(returnsTypeCtorDef, ModuleDefinition);
                instructions.InsertBefore(insertBeforeThisIns, stateMachineVariable.LdlocOrA());
                instructions.InsertBefore(insertBeforeThisIns, Instruction.Create(OpCodes.Newobj, returnsTypeCtorRef));
                instructions.InsertBefore(insertBeforeThisIns, Instruction.Create(OpCodes.Stfld, returnsFieldRef));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TypeReference IteratorGetEntryStateTypeReference(MethodBody body)
        {
            return ((MethodReference)body.Instructions.Single(x => x.OpCode.Code == Code.Newobj).Operand).DeclaringType;
        }
    }
}
