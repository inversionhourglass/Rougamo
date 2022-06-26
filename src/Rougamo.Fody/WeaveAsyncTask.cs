using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
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
            var returnTypeRef = AsyncOnEntry(rouMethod, out var stateTypeDef, out var mosFieldDef, out var contextFieldDef, out var builderDef);
            var moveNextMethodDef = stateTypeDef.Methods.Single(m => m.Name == Constants.METHOD_MoveNext);
            var builderFieldDef = stateTypeDef.Fields.Single(x => x.Name == "<>t__builder");
            var setResultIns = moveNextMethodDef.Body.Instructions.SingleOrDefault(x => x.Operand is MethodReference methodRef && methodRef.DeclaringType.Resolve() == builderDef && methodRef.Name == Constants.METHOD_SetResult);
            FieldReference mosFieldRef = mosFieldDef;
            FieldReference contextFieldRef = contextFieldDef;
            FieldReference builderFieldRef = builderFieldDef;
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
            }
            AsyncOnExceptionWithExit(rouMethod, moveNextMethodDef, mosFieldRef, contextFieldRef, builderFieldRef, returnTypeRef, setResultIns);
            AsyncOnSuccessWithExit(rouMethod, moveNextMethodDef, mosFieldRef, contextFieldRef, setResultIns, returnTypeRef);
            moveNextMethodDef.Body.OptimizePlus();
            rouMethod.MethodDef.Body.OptimizePlus();
        }

        private TypeReference AsyncOnEntry(RouMethod rouMethod, out TypeDefinition stateTypeDef, out FieldDefinition mosFieldDef, out FieldDefinition contextFieldDef, out TypeDefinition builderDef)
        {
            var returnType = rouMethod.MethodDef.ReturnType;
            AsyncFieldDefinition(rouMethod, out stateTypeDef, out mosFieldDef, out contextFieldDef);
            var tStateTypeDef = stateTypeDef;
            var stateMachineVariable = rouMethod.MethodDef.Body.Variables.Single(x => x.VariableType.Resolve() == tStateTypeDef);
            var taskBuilderPreviousIns = AsyncGetTaskMethodBuilderCreatePreviousInstruction(rouMethod.MethodDef.Body, out builderDef);
            var mosFieldRef = new FieldReference(mosFieldDef.Name, mosFieldDef.FieldType, stateMachineVariable.VariableType);
            var contextFieldRef = new FieldReference(contextFieldDef.Name, contextFieldDef.FieldType, stateMachineVariable.VariableType);
            InitMosField(rouMethod, mosFieldRef, stateMachineVariable, taskBuilderPreviousIns);
            InitMethodContextField(rouMethod, contextFieldRef, stateMachineVariable, taskBuilderPreviousIns, true, false);
            var returnReplaceStartNop = Create(OpCodes.Nop);
            rouMethod.MethodDef.Body.Instructions.InsertBefore(taskBuilderPreviousIns, returnReplaceStartNop);
            ExecuteMoMethod(Constants.METHOD_OnEntry, rouMethod.MethodDef, rouMethod.Mos.Count, stateMachineVariable, mosFieldRef, contextFieldRef, returnReplaceStartNop, false);

            rouMethod.MethodDef.Body.Instructions.InsertBefore(taskBuilderPreviousIns, AsyncIfReturnValueReplacedOnEntry(rouMethod.MethodDef, stateMachineVariable, contextFieldRef, taskBuilderPreviousIns, returnType));

            return returnType.Is(Constants.TYPE_Task) || returnType.Is(Constants.TYPE_ValueTask) || returnType.Is(Constants.TYPE_Void) ? _typeVoidRef : ((GenericInstanceType)returnType).GenericArguments[0];
        }

        private List<Instruction> AsyncIfReturnValueReplacedOnEntry(MethodDefinition methodDef, VariableDefinition stateMachineVariable, FieldReference contextFieldRef, Instruction ifNotOffset, TypeReference returnType)
        {
            var instructions = new List<Instruction>();
            instructions.Add(Create(OpCodes.Ldloc, stateMachineVariable));
            instructions.Add(Create(OpCodes.Ldfld, contextFieldRef));
            instructions.Add(Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueReplacedRef));
            instructions.Add(Create(OpCodes.Brfalse_S, ifNotOffset));
            if (returnType.IsTask())
            {
                var getCompletedTaskMethodDef = returnType.Resolve().Properties.Single(x => x.Name == "CompletedTask").GetMethod;
                instructions.Add(Create(OpCodes.Call, Import(getCompletedTaskMethodDef)));
            }
            else if (returnType.IsValueTask())
            {
                var returnVariable = methodDef.Body.CreateVariable(returnType);
                instructions.Add(Create(OpCodes.Ldloca, returnVariable));
                instructions.Add(Create(OpCodes.Initobj, returnType));
                instructions.Add(Create(OpCodes.Ldloc, returnVariable));
            }
            else if (returnType.IsVoid())
            {
                // do nothing
            }
            else
            {
                var valueType = ((GenericInstanceType)returnType).GenericArguments[0];
                instructions.Add(Create(OpCodes.Ldloc, stateMachineVariable));
                instructions.Add(Create(OpCodes.Ldfld, contextFieldRef));
                instructions.Add(Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueRef));
                if (valueType.IsValueType || (!valueType.IsArray && valueType.IsEnum(out _)) || valueType.IsGenericParameter)
                {
                    instructions.Add(Create(OpCodes.Unbox_Any, valueType));
                }
                if (returnType.IsGenericTask())
                {
                    var fromResultMethodDef = returnType.Resolve().BaseType.Resolve().Methods.Single(x => x.Name == "FromResult");
                    var fromResultMethodRef = Import(fromResultMethodDef).GenericMethodReference(valueType);
                    instructions.Add(Create(OpCodes.Call, fromResultMethodRef));
                }
                else if (returnType.IsGenericValueTask())
                {
                    var ctorDef = returnType.Resolve().GetConstructors().Single(x => x.Parameters.Count == 1 && x.Parameters[0].ParameterType is GenericParameter);
                    var ctorRef = returnType.GenericTypeMethodReference(Import(ctorDef), ModuleDefinition);
                    instructions.Add(Create(OpCodes.Newobj, ctorRef));
                }
                else
                {
                    throw new NotImplementedException($"unknow async return type: {returnType.Resolve()?.FullName}");
                }
            }
            instructions.Add(Create(OpCodes.Ret));

            return instructions;
        }

        private void AsyncOnExceptionWithExit(RouMethod rouMethod, MethodDefinition moveNextMethodDef, FieldReference mosFieldRef, FieldReference contextFieldRef, FieldReference builderFieldRef, TypeReference returnTypeRef, Instruction setResultIns)
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
                AsyncIfExceptionHandled(moveNextMethodDef, contextFieldRef, builderFieldRef, returnTypeRef, exceptionHandler, setResultIns, setExceptionFirst, setExceptionLast.Next);
            }
            ExecuteMoMethod(Constants.METHOD_OnException, moveNextMethodDef, rouMethod.Mos.Count, null, mosFieldRef, contextFieldRef, beforeSetException.Next, this.ConfigReverseCallEnding());
        }

        private void AsyncOnSuccessWithExit(RouMethod rouMethod, MethodDefinition moveNextMethodDef, FieldReference mosFieldRef, FieldReference contextFieldRef, Instruction setResultIns, TypeReference returnTypeRef)
        {
            if (setResultIns == null) return; // 100% throw exception

            var instructions = moveNextMethodDef.Body.Instructions;
            var closeThisIns = setResultIns.ClosePreviousLdarg0(rouMethod.MethodDef);
            var isValueType = returnTypeRef.IsValueType || returnTypeRef.IsEnum(out _) && !returnTypeRef.IsArray || returnTypeRef.IsGenericParameter;
            if (!returnTypeRef.Is(Constants.TYPE_Void))
            {
                var ldlocResult = setResultIns.Previous.Copy();
                instructions.InsertBefore(closeThisIns, Create(OpCodes.Ldarg_0));
                instructions.InsertBefore(closeThisIns, Create(OpCodes.Ldfld, contextFieldRef));
                instructions.InsertBefore(closeThisIns, ldlocResult);
                if (isValueType)
                {
                    instructions.InsertBefore(closeThisIns, Create(OpCodes.Box, ldlocResult.GetVariableType(moveNextMethodDef.Body)));
                }
                instructions.InsertBefore(closeThisIns, Create(OpCodes.Callvirt, _methodMethodContextSetReturnValueRef));
            }
            var beforeOnSuccessWeave = closeThisIns.Previous;
            ExecuteMoMethod(Constants.METHOD_OnExit, moveNextMethodDef, rouMethod.Mos.Count, null, mosFieldRef, contextFieldRef, closeThisIns, this.ConfigReverseCallEnding());
            var onExitFirst = beforeOnSuccessWeave.Next;
            if (!returnTypeRef.Is(Constants.TYPE_Void))
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

        private void AsyncIfExceptionHandled(MethodDefinition moveNextMethodDef, FieldReference contextFieldRef, FieldReference builderFieldRef, TypeReference returnTypeRef, ExceptionHandler exceptionHandler, Instruction setResultIns, Instruction beforeThisInstruction, Instruction leaves)
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
                var setResultRef = builderFieldRef.FieldType.Resolve().Methods.Single(x => x.Name == "SetResult" && x.Parameters.Count == 0).ImportInto(ModuleDefinition);
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
                if (returnTypeRef.IsValueType || (!returnTypeRef.IsArray && returnTypeRef.IsEnum(out _)) || returnTypeRef.IsGenericParameter)
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
                var setResultRef = builderFieldRef.FieldType.GenericTypeMethodReference(builderFieldRef.FieldType.Resolve().Methods.Single(x => x.Name == "SetResult" && x.Parameters.Count == 1 && !x.Parameters[0].ParameterType.DerivesFrom(Constants.TYPE_Task)), ModuleDefinition);
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

        private void AsyncFieldDefinition(RouMethod rouMethod, out TypeDefinition stateTypeDef, out FieldDefinition mosFieldDef, out FieldDefinition contextFieldDef)
        {
            stateTypeDef = rouMethod.MethodDef.ResolveAsyncStateMachine();
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

        private Instruction AsyncGetTaskMethodBuilderCreatePreviousInstruction(MethodBody body, out TypeDefinition builderDef)
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
                    builderDef = methodRef.DeclaringType.Resolve();
                    return ins.Previous;
                }
            }
            throw new RougamoException("unable find call AsyncTaskMethodBuilder.Create instruction");
        }
    }
}
