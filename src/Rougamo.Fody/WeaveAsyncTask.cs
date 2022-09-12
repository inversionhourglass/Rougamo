using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void AsyncTaskMethodWeave(RouMethod rouMethod)
        {
            StateMachineFieldDefinition(rouMethod, Constants.TYPE_AsyncStateMachineAttribute, out var stateTypeDef, out var mosFieldDef, out var contextFieldDef);
            var moveNextMethodDef = stateTypeDef.Methods.Single(m => m.Name == Constants.METHOD_MoveNext);
            var moveNextInstructions = moveNextMethodDef.Body.Instructions;
            var stateMachineVariable = rouMethod.MethodDef.Body.Variables.Single(x => x.VariableType.Resolve() == stateTypeDef);
            var builderDef = ResolveBuilderTypeDef(rouMethod.MethodDef.Body, out var ldlocStateMachineIns);
            AsyncGetFiledRefs(stateTypeDef, mosFieldDef, contextFieldDef, out var mosFieldRef, out var contextFieldRef, out var builderFieldRef, out var stateFieldRef);
            StateMachineInitMosField(rouMethod, mosFieldDef, stateMachineVariable, ldlocStateMachineIns);
            StateMachineInitMethodContextField(rouMethod, contextFieldDef, stateMachineVariable, ldlocStateMachineIns, true, false);

            var returnType = rouMethod.MethodDef.ReturnType;
            var returnTypeRef = returnType.IsTask() || returnType.IsValueTask() || returnType.IsVoid() ? _typeVoidRef : ((GenericInstanceType)returnType).GenericArguments[0];
            var setResultIns = moveNextInstructions.SingleOrDefault(x => x.Operand is MethodReference methodRef && methodRef.DeclaringType.Resolve() == builderDef && methodRef.Name == Constants.METHOD_SetResult);
            var returnValueType = returnTypeRef.ToReturnType();

            AsyncResolveReturnOperations(moveNextMethodDef, returnTypeRef, setResultIns, returnValueType, out var returnLdlocFunc, out var returnStlocFunc);
            AsyncOnEntry(rouMethod, moveNextMethodDef, mosFieldRef, contextFieldRef, builderFieldRef, stateFieldRef, returnTypeRef, returnValueType, returnLdlocFunc, returnStlocFunc);
            
            var afterOnExceptionNop = AsyncOnException(rouMethod, moveNextMethodDef, mosFieldRef, contextFieldRef, out var setExceptionFirst, out var setExceptionLast);
            var afterOnExitNop = moveNextInstructions.InsertBefore(setExceptionFirst, Create(OpCodes.Nop));
            moveNextInstructions.InsertAfter(afterOnExceptionNop, AsyncSaveExceptionHandleStatus(moveNextMethodDef, contextFieldRef, returnTypeRef, returnValueType, out var exceptionHandledVariable, out var returnValueVariable));
            ExecuteMoMethod(Constants.METHOD_OnExit, moveNextMethodDef, rouMethod.Mos.Count, mosFieldRef, contextFieldRef, afterOnExitNop, this.ConfigReverseCallEnding());
            AsyncIfExceptionHandled(moveNextMethodDef, exceptionHandledVariable, returnValueVariable, builderFieldRef, returnTypeRef, setExceptionFirst, setExceptionLast.Next, returnValueType);

            AsyncOnSuccessWithExit(rouMethod, moveNextMethodDef, mosFieldRef, contextFieldRef, setResultIns, returnTypeRef);

            moveNextMethodDef.Body.OptimizePlus();
            rouMethod.MethodDef.Body.OptimizePlus();
        }

        private void AsyncGetFiledRefs(TypeDefinition stateTypeDef, FieldDefinition mosFieldDef, FieldDefinition contextFieldDef, out FieldReference mosFieldRef, out FieldReference contextFieldRef, out FieldReference builderFieldRef, out FieldReference stateFieldRef)
        {
            var builderFieldDef = stateTypeDef.Fields.Single(x => x.Name == "<>t__builder");
            var stateFieldDef = stateTypeDef.Fields.Single(x => x.Name == "<>1__state");
            mosFieldRef = mosFieldDef;
            contextFieldRef = contextFieldDef;
            builderFieldRef = builderFieldDef;
            stateFieldRef = stateFieldDef;
            if (stateTypeDef.HasGenericParameters)
            {
                // generic return type will get in
                // public async Task<MyClass<T>> Mt<T>()
                var stateTypeRef = new GenericInstanceType(stateTypeDef);
                foreach (var parameter in stateTypeDef.GenericParameters)
                {
                    stateTypeRef.GenericArguments.Add(parameter);
                }
                mosFieldRef = new FieldReference(mosFieldDef.Name, mosFieldDef.FieldType, stateTypeRef);
                contextFieldRef = new FieldReference(contextFieldDef.Name, contextFieldDef.FieldType, stateTypeRef);
                builderFieldRef = new FieldReference(builderFieldDef.Name, builderFieldDef.FieldType, stateTypeRef);
                stateFieldRef = new FieldReference(stateFieldDef.Name, stateFieldDef.FieldType, stateTypeRef);
            }
        }

        private void AsyncResolveReturnOperations(MethodDefinition moveNextMethodDef, TypeReference returnTypeRef, Instruction? setResultIns, ReturnType returnValueType, out Func<Instruction?> returnLdlocFunc, out Func<Instruction?> returnStlocFunc)
        {
            if(returnValueType == ReturnType.Void)
            {
                returnLdlocFunc = () => null;
                returnStlocFunc = () => null;
            }
            else if (setResultIns == null)
            {
                var resultVariable = moveNextMethodDef.Body.CreateVariable(returnTypeRef);
                returnLdlocFunc = () => Create(OpCodes.Ldloc, resultVariable);
                returnStlocFunc = () => Create(OpCodes.Stloc, resultVariable);
            }
            else
            {
                returnLdlocFunc = () => setResultIns.Previous.Copy();
                returnStlocFunc = () => setResultIns.Previous.Ldloc2Stloc($"[{moveNextMethodDef.DeclaringType.FullName}] offset: {setResultIns.Previous.Offset}, it should be ldloc");
            }
        }

        private void AsyncOnEntry(RouMethod rouMethod, MethodDefinition moveNextMethodDef, FieldReference mosFieldRef, FieldReference contextFieldRef, FieldReference builderFieldRef, FieldReference stateFieldRef, TypeReference returnTypeRef, ReturnType returnValueType, Func<Instruction?> returnLdlocFunc, Func<Instruction?> returnStlocFunc)
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
            var afterOnEntryNop = Create(OpCodes.Nop);
            instructions.InsertBefore(originFirstIns, afterOnEntryNop);
            ExecuteMoMethod(Constants.METHOD_OnEntry, moveNextMethodDef, rouMethod.Mos.Count, mosFieldRef, contextFieldRef, afterOnEntryNop, !this.ConfigReverseCallEnding());
            instructions.InsertBefore(originFirstIns, new[]
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, contextFieldRef),
                Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueReplacedRef),
                Create(OpCodes.Brfalse_S, originFirstIns),
            });
            var ldlocResultIns = returnLdlocFunc();
            if (returnValueType != ReturnType.Void)
            {
                instructions.InsertBefore(originFirstIns, new[]
                {
                    Create(OpCodes.Ldarg_0),
                    Create(OpCodes.Ldfld, contextFieldRef),
                    Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueRef),
                });
                if (returnValueType == ReturnType.ValueType)
                {
                    instructions.InsertBefore(originFirstIns, Create(OpCodes.Unbox_Any, returnTypeRef));
                }
                var stlocResultIns = returnStlocFunc()!;
                instructions.InsertBefore(originFirstIns, stlocResultIns);
            }
            instructions.InsertBefore(originFirstIns, new[]
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldc_I4, -2),
                Create(OpCodes.Stfld, stateFieldRef)
            });
            var afterOnExitNop = Create(OpCodes.Nop);
            instructions.InsertBefore(originFirstIns, afterOnExitNop);
            ExecuteMoMethod(Constants.METHOD_OnExit, moveNextMethodDef, rouMethod.Mos.Count, mosFieldRef, contextFieldRef, afterOnExitNop, this.ConfigReverseCallEnding());
            instructions.InsertBefore(originFirstIns, new[]
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldflda, builderFieldRef)
            });
            if (ldlocResultIns != null) instructions.InsertBefore(originFirstIns, ldlocResultIns);
            var setResultRef = returnTypeRef.IsVoid() ? GetSetResult(builderFieldRef.FieldType) : GetGenericSetResult(builderFieldRef.FieldType);
            instructions.InsertBefore(originFirstIns, Create(OpCodes.Call, setResultRef));
            instructions.InsertBefore(originFirstIns, Create(OpCodes.Ret));
        }

        private Instruction AsyncOnException(RouMethod rouMethod, MethodDefinition moveNextMethodDef, FieldReference mosFieldRef, FieldReference contextFieldRef, out Instruction setExceptionFirst, out Instruction setExceptionLast)
        {
            var instructions = moveNextMethodDef.Body.Instructions;
            var exceptionHandler = GetOuterExceptionHandler(moveNextMethodDef.Body);
            var ldlocException = exceptionHandler.HandlerStart.Stloc2Ldloc($"[{rouMethod.MethodDef.FullName}] exception handler first instruction is not stloc.s exception");

            FindMoveNextSetExceptionFirstInstruction(moveNextMethodDef, exceptionHandler, out setExceptionFirst, out setExceptionLast);

            instructions.InsertBefore(setExceptionFirst, Create(OpCodes.Ldarg_0));
            instructions.InsertBefore(setExceptionFirst, Create(OpCodes.Ldfld, contextFieldRef));
            instructions.InsertBefore(setExceptionFirst, ldlocException);
            instructions.InsertBefore(setExceptionFirst, Create(OpCodes.Callvirt, _methodMethodContextSetExceptionRef));

            var afterOnExceptionNop = instructions.InsertBefore(setExceptionFirst, Create(OpCodes.Nop));
            ExecuteMoMethod(Constants.METHOD_OnException, moveNextMethodDef, rouMethod.Mos.Count, mosFieldRef, contextFieldRef, afterOnExceptionNop, this.ConfigReverseCallEnding());

            return afterOnExceptionNop;
        }

        private void AsyncOnSuccessWithExit(RouMethod rouMethod, MethodDefinition moveNextMethodDef, FieldReference mosFieldRef, FieldReference contextFieldRef, Instruction setResultIns, TypeReference returnTypeRef)
        {
            if (setResultIns == null) return; // 100% throw exception

            var instructions = moveNextMethodDef.Body.Instructions;
            var beforeSetResultLdarg0Ins = setResultIns.ClosePreviousLdarg0(rouMethod.MethodDef);
            var isValueType = returnTypeRef.IsValueType || returnTypeRef.IsEnum(out _) && !returnTypeRef.IsArray || returnTypeRef.IsGenericParameter;
            if (!returnTypeRef.IsVoid())
            {
                var ldlocResult = setResultIns.Previous.Copy();
                instructions.InsertBefore(beforeSetResultLdarg0Ins, Create(OpCodes.Ldarg_0));
                instructions.InsertBefore(beforeSetResultLdarg0Ins, Create(OpCodes.Ldfld, contextFieldRef));
                instructions.InsertBefore(beforeSetResultLdarg0Ins, ldlocResult);
                if (isValueType)
                {
                    instructions.InsertBefore(beforeSetResultLdarg0Ins, Create(OpCodes.Box, ldlocResult.GetVariableType(moveNextMethodDef.Body)));
                }
                instructions.InsertBefore(beforeSetResultLdarg0Ins, Create(OpCodes.Callvirt, _methodMethodContextSetReturnValueRef));
            }
            var beforeOnSuccessWeave = beforeSetResultLdarg0Ins.Previous;
            ExecuteMoMethod(Constants.METHOD_OnExit, moveNextMethodDef, rouMethod.Mos.Count, mosFieldRef, contextFieldRef, beforeSetResultLdarg0Ins, this.ConfigReverseCallEnding());
            var onExitFirst = beforeOnSuccessWeave.Next;
            if (!returnTypeRef.IsVoid())
            {
                instructions.InsertBefore(onExitFirst, new[]
                {
                    Create(OpCodes.Ldarg_0),
                    Create(OpCodes.Ldfld, contextFieldRef),
                    Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueReplacedRef),
                    Create(OpCodes.Brfalse_S, onExitFirst),
                    Create(OpCodes.Ldarg_0),
                    Create(OpCodes.Ldfld, contextFieldRef),
                    Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueRef),
                });
                if (isValueType)
                {
                    instructions.InsertBefore(onExitFirst, Create(OpCodes.Unbox_Any, returnTypeRef));
                }
                instructions.InsertBefore(onExitFirst, setResultIns.Previous.Ldloc2Stloc($"[{moveNextMethodDef.DeclaringType.FullName}] offset: {setResultIns.Previous.Offset}, it should be ldloc"));
            }
            ExecuteMoMethod(Constants.METHOD_OnSuccess, moveNextMethodDef, rouMethod.Mos.Count, mosFieldRef, contextFieldRef, beforeOnSuccessWeave.Next, this.ConfigReverseCallEnding());
        }

        private Instruction[] AsyncSaveExceptionHandleStatus(MethodDefinition moveNextMethodDef, FieldReference contextFieldRef, TypeReference returnTypeRef, ReturnType returnValueType, out VariableDefinition exceptionHandledVariable, out VariableDefinition returnValueVariable)
        {
            exceptionHandledVariable = moveNextMethodDef.Body.CreateVariable(_typeBoolRef);
            returnValueVariable = moveNextMethodDef.Body.CreateVariable(_typeObjectRef);
            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, contextFieldRef),
                Create(OpCodes.Callvirt, _methodMethodContextGetExceptionHandledRef),
                Create(OpCodes.Stloc, exceptionHandledVariable)
            };
            if(returnValueType != ReturnType.Void)
            {
                instructions.AddRange(new[]
                {
                    Create(OpCodes.Ldarg_0),
                    Create(OpCodes.Ldfld, contextFieldRef),
                    Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueRef),
                    Create(OpCodes.Stloc, returnValueVariable)
                });
            }
            return instructions.ToArray();
        }

        private void AsyncIfExceptionHandled(MethodDefinition moveNextMethodDef, VariableDefinition exceptionHandledVariable, VariableDefinition returnValueVariable, FieldReference builderFieldRef, TypeReference returnTypeRef, Instruction beforeThisInstruction, Instruction leaves, ReturnType returnValueType)
        {
            var instructions = moveNextMethodDef.Body.Instructions;
            instructions.InsertBefore(beforeThisInstruction, new[]
            {
                Create(OpCodes.Ldloc, exceptionHandledVariable),
                Create(OpCodes.Brfalse_S, beforeThisInstruction)
            });
            if (returnValueType == ReturnType.Void)
            {
                var setResultRef = GetSetResult(builderFieldRef.FieldType);
                instructions.InsertBefore(beforeThisInstruction, new[]
                {
                    Create(OpCodes.Ldarg_0),
                    Create(OpCodes.Ldflda, builderFieldRef),
                    Create(OpCodes.Call, setResultRef)
                });
            }
            else
            {
                var setResultRef = GetGenericSetResult(builderFieldRef.FieldType);
                instructions.InsertBefore(beforeThisInstruction, new[]
                {
                    Create(OpCodes.Ldarg_0),
                    Create(OpCodes.Ldflda, builderFieldRef),
                    Create(OpCodes.Ldloc, returnValueVariable)
                });
                if (returnValueType == ReturnType.ValueType)
                {
                    instructions.InsertBefore(beforeThisInstruction, Create(OpCodes.Unbox_Any, returnTypeRef));
                }
                instructions.InsertBefore(beforeThisInstruction, Create(OpCodes.Call, setResultRef));
            }
            instructions.InsertBefore(beforeThisInstruction, Create(OpCodes.Br, leaves));
        }

        private void StateMachineFieldDefinition(RouMethod rouMethod, string stateMachineName, out TypeDefinition stateTypeDef, out FieldDefinition mosFieldDef, out FieldDefinition contextFieldDef)
        {
            stateTypeDef = rouMethod.MethodDef.ResolveStateMachine(stateMachineName);
            var fieldAttributes = FieldAttributes.Public;
            mosFieldDef = new FieldDefinition(Constants.FIELD_RougamoMos, fieldAttributes, _typeIMoArrayRef);
            contextFieldDef = new FieldDefinition(Constants.FIELD_RougamoContext, fieldAttributes, _typeMethodContextRef);
            stateTypeDef.Fields.Add(mosFieldDef);
            stateTypeDef.Fields.Add(contextFieldDef);
        }

        private void FindMoveNextSetExceptionFirstInstruction(MethodDefinition moveNextMethodDef, ExceptionHandler exceptionHandler, out Instruction setExceptionFirst, out Instruction setExceptionLast)
        {
            var instruction = exceptionHandler.HandlerStart;
            while (true)
            {
                if (instruction.OpCode.Code == Code.Call && instruction.Operand is MethodReference setException &&
                    new[] {
                        Constants.TYPE_AsyncTaskMethodBuilder,
                        Constants.TYPE_AsyncValueTaskMethodBuilder,
                        Constants.TYPE_AsyncVoidMethodBuilder,
                        Constants.TYPE_ManualResetValueTaskSourceCore
                    }.Any(x => setException.DeclaringType.FullName.StartsWith(x)) &&
                    setException.Name == "SetException")
                {
                    setExceptionLast = instruction;
                    break;
                }
                instruction = instruction.Next;
                if (instruction == null || instruction == exceptionHandler.HandlerEnd) throw new InvalidOperationException($"[{moveNextMethodDef.DeclaringType.FullName}] SetException instruction not found");
            }
            setExceptionFirst = instruction.Previous.Previous.Previous;
            if (setExceptionFirst.OpCode.Code != Code.Ldarg_0) throw new RougamoException($"Offset {setExceptionFirst.Offset} of {moveNextMethodDef.FullName} is {setExceptionFirst.OpCode.Code}, it should be Ldarg0 of SetResult which offset is {instruction.Offset}");
        }

        private ExceptionHandler GetOuterExceptionHandler(MethodBody methodBody)
        {
            ExceptionHandler? exceptionHandler = null;
            int offset = methodBody.Instructions.First().Offset;
            foreach (var handler in methodBody.ExceptionHandlers)
            {
                if (handler.HandlerType != ExceptionHandlerType.Catch) continue;
                if (handler.TryEnd.Offset > offset)
                {
                    exceptionHandler = handler;
                    offset = handler.TryEnd.Offset;
                }
            }
            return exceptionHandler ?? throw new RougamoException("can not find outer exception handler");
        }

        private TypeDefinition ResolveBuilderTypeDef(MethodBody body, out Instruction ldlocStateMachineIns)
        {
            for (int i = 0; i < body.Instructions.Count; i++)
            {
                var ins = body.Instructions[i];
                if (ins.OpCode == OpCodes.Call && ins.Operand is MethodReference methodRef &&
                    (methodRef.DeclaringType.FullName.StartsWith(Constants.TYPE_AsyncTaskMethodBuilder) ||
                    methodRef.DeclaringType.FullName.StartsWith(Constants.TYPE_AsyncValueTaskMethodBuilder) ||
                    methodRef.DeclaringType.FullName.StartsWith(Constants.TYPE_AsyncVoidMethodBuilder)) &&
                    methodRef.Name == Constants.METHOD_Create)
                {
                    ldlocStateMachineIns = ins.Previous;
                    return methodRef.DeclaringType.Resolve();
                }
            }
            throw new RougamoException("unable find call AsyncTaskMethodBuilder.Create instruction");
        }

        private MethodReference GetSetResult(TypeReference builderTypeRef)
        {
            return builderTypeRef.Resolve().Methods.Single(x => x.Name == "SetResult" && x.Parameters.Count == 0 && x.IsPublic).ImportInto(ModuleDefinition);
        }

        private MethodReference GetGenericSetResult(TypeReference builderTypeRef)
        {
            return builderTypeRef.GenericTypeMethodReference(builderTypeRef.Resolve().Methods.Single(x => x.Name == "SetResult" && x.Parameters.Count == 1 && x.IsPublic), ModuleDefinition);
        }
    }
}
