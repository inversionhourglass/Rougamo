using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
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
            var stateMachineVariable = rouMethod.MethodDef.Body.Variables.Single(x => x.VariableType.Resolve() == stateTypeDef);
            var builderDef = ResolveBuilderTypeDef(rouMethod.MethodDef.Body, out var ldlocStateMachineIns);
            AsyncGetFiledRefs(stateTypeDef, mosFieldDef, contextFieldDef, out var mosFieldRef, out var contextFieldRef, out var builderFieldRef, out var stateFieldRef);
            InitMosField(rouMethod, mosFieldRef, stateMachineVariable, ldlocStateMachineIns);
            InitMethodContextField(rouMethod, contextFieldRef, stateMachineVariable, ldlocStateMachineIns, true, false);

            var returnType = rouMethod.MethodDef.ReturnType;
            var returnTypeRef = returnType.IsTask() || returnType.IsValueTask() || returnType.IsVoid() ? _typeVoidRef : ((GenericInstanceType)returnType).GenericArguments[0];
            var setResultIns = moveNextMethodDef.Body.Instructions.SingleOrDefault(x => x.Operand is MethodReference methodRef && methodRef.DeclaringType.Resolve() == builderDef && methodRef.Name == Constants.METHOD_SetResult);
            var returnValueType = returnTypeRef.IsValueType || returnTypeRef.IsEnum(out _) && !returnTypeRef.IsArray || returnTypeRef.IsGenericParameter;
            AsyncOnEntry(rouMethod, moveNextMethodDef, mosFieldRef, contextFieldRef, builderFieldRef, stateFieldRef, returnTypeRef, setResultIns, returnValueType);
            AsyncOnExceptionWithExit(rouMethod, moveNextMethodDef, mosFieldRef, contextFieldRef, builderFieldRef, returnTypeRef, setResultIns, returnValueType);
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

        private void AsyncOnEntry(RouMethod rouMethod, MethodDefinition moveNextMethodDef, FieldReference mosFieldRef, FieldReference contextFieldRef, FieldReference builderFieldRef, FieldReference stateFieldRef, TypeReference returnTypeRef, Instruction setResultIns, bool returnValueType)
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
            ExecuteMoMethod(Constants.METHOD_OnEntry, moveNextMethodDef, rouMethod.Mos.Count, null, mosFieldRef, contextFieldRef, afterOnEntryNop, this.ConfigReverseCallEnding());
            instructions.InsertBefore(originFirstIns, new[]
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, contextFieldRef),
                Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueReplacedRef),
                Create(OpCodes.Brfalse_S, originFirstIns),
            });
            Instruction ldlocResultIns = null;
            if (!returnTypeRef.IsVoid())
            {
                instructions.InsertBefore(originFirstIns, new[]
                {
                    Create(OpCodes.Ldarg_0),
                    Create(OpCodes.Ldfld, contextFieldRef),
                    Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueRef),
                });
                if (returnValueType)
                {
                    instructions.InsertBefore(originFirstIns, Create(OpCodes.Unbox_Any, returnTypeRef));
                }
                Instruction stlocResultIns;
                if(setResultIns == null)
                {
                    var resultVariable = moveNextMethodDef.Body.CreateVariable(returnTypeRef);
                    ldlocResultIns = Create(OpCodes.Ldloc, resultVariable);
                    stlocResultIns = Create(OpCodes.Stloc, resultVariable);
                }
                else
                {
                    ldlocResultIns = setResultIns.Previous.Copy();
                    stlocResultIns = setResultIns.Previous.Ldloc2Stloc($"[{moveNextMethodDef.DeclaringType.FullName}] offset: {setResultIns.Previous.Offset}, it should be ldloc");
                }
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
            ExecuteMoMethod(Constants.METHOD_OnExit, moveNextMethodDef, rouMethod.Mos.Count, null, mosFieldRef, contextFieldRef, afterOnExitNop, this.ConfigReverseCallEnding());
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

        private void AsyncOnExceptionWithExit(RouMethod rouMethod, MethodDefinition moveNextMethodDef, FieldReference mosFieldRef, FieldReference contextFieldRef, FieldReference builderFieldRef, TypeReference returnTypeRef, Instruction setResultIns, bool returnValueType)
        {
            var instructions = moveNextMethodDef.Body.Instructions;
            var exceptionHandler = GetOuterExceptionHandler(moveNextMethodDef.Body);
            var ldlocException = exceptionHandler.HandlerStart.Stloc2Ldloc($"[{rouMethod.MethodDef.FullName}] exception handler first instruction is not stloc.s exception");

            FindMoveNextSetExceptionFirstInstruction(moveNextMethodDef, exceptionHandler, out var setExceptionFirst, out var setExceptionLast);

            instructions.InsertBefore(setExceptionFirst, Create(OpCodes.Ldarg_0));
            instructions.InsertBefore(setExceptionFirst, Create(OpCodes.Ldfld, contextFieldRef));
            instructions.InsertBefore(setExceptionFirst, ldlocException);
            instructions.InsertBefore(setExceptionFirst, Create(OpCodes.Callvirt, _methodMethodContextSetExceptionRef));

            var beforeSetException = setExceptionFirst.Previous;
            ExecuteMoMethod(Constants.METHOD_OnExit, moveNextMethodDef, rouMethod.Mos.Count, null, mosFieldRef, contextFieldRef, setExceptionLast.Next, this.ConfigReverseCallEnding());
            if (builderFieldRef != null)
            {
                AsyncIfExceptionHandled(moveNextMethodDef, contextFieldRef, builderFieldRef, returnTypeRef, exceptionHandler, setResultIns, setExceptionFirst, setExceptionLast.Next, returnValueType);
            }
            ExecuteMoMethod(Constants.METHOD_OnException, moveNextMethodDef, rouMethod.Mos.Count, null, mosFieldRef, contextFieldRef, beforeSetException.Next, this.ConfigReverseCallEnding());
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
            ExecuteMoMethod(Constants.METHOD_OnExit, moveNextMethodDef, rouMethod.Mos.Count, null, mosFieldRef, contextFieldRef, beforeSetResultLdarg0Ins, this.ConfigReverseCallEnding());
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
            ExecuteMoMethod(Constants.METHOD_OnSuccess, moveNextMethodDef, rouMethod.Mos.Count, null, mosFieldRef, contextFieldRef, beforeOnSuccessWeave.Next, this.ConfigReverseCallEnding());
        }

        private void AsyncIfExceptionHandled(MethodDefinition moveNextMethodDef, FieldReference contextFieldRef, FieldReference builderFieldRef, TypeReference returnTypeRef, ExceptionHandler exceptionHandler, Instruction setResultIns, Instruction beforeThisInstruction, Instruction leaves, bool returnValueType)
        {
            var instructions = moveNextMethodDef.Body.Instructions;
            instructions.InsertBefore(beforeThisInstruction, new[]
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, contextFieldRef),
                Create(OpCodes.Callvirt, _methodMethodContextGetExceptionHandledRef),
                Create(OpCodes.Brfalse_S, beforeThisInstruction)
            });
            if (returnTypeRef.IsVoid())
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
                instructions.InsertBefore(beforeThisInstruction, new[]
                {
                    Create(OpCodes.Ldarg_0),
                    Create(OpCodes.Ldfld, contextFieldRef),
                    Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueRef),
                });
                if (returnValueType)
                {
                    instructions.InsertBefore(beforeThisInstruction, Create(OpCodes.Unbox_Any, returnTypeRef));
                }
                Instruction setReturnVariable;
                Instruction getReturnVariable;
                if (setResultIns == null)
                {
                    var returnVariable = moveNextMethodDef.Body.CreateVariable(Import(returnTypeRef));
                    setReturnVariable = Create(OpCodes.Stloc, returnVariable);
                    getReturnVariable = Create(OpCodes.Ldloc, returnVariable);
                }
                else
                {
                    setReturnVariable = setResultIns.Previous.Ldloc2Stloc($"[{moveNextMethodDef.DeclaringType.FullName}] offset: {setResultIns.Previous.Offset}, it should be ldloc");
                    getReturnVariable = setResultIns.Previous.Copy();
                }
                var setResultRef = GetGenericSetResult(builderFieldRef.FieldType);
                instructions.InsertBefore(beforeThisInstruction, new[]
                {
                    setReturnVariable,
                    Create(OpCodes.Ldarg_0),
                    Create(OpCodes.Ldflda, builderFieldRef),
                    getReturnVariable,
                    Create(OpCodes.Call, setResultRef)
                });
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
            while (true)
            {
                if (instruction.OpCode.Code == Code.Ldarg_0)
                {
                    setExceptionFirst = instruction;
                    break;
                }

                instruction = instruction.Previous;
                if (instruction == null || instruction == exceptionHandler.HandlerStart) throw new InvalidOperationException($"[{moveNextMethodDef.DeclaringType.FullName}] SetException' ldarg.0 not found");
            }
        }

        private ExceptionHandler GetOuterExceptionHandler(MethodBody methodBody)
        {
            ExceptionHandler exceptionHandler = null;
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
