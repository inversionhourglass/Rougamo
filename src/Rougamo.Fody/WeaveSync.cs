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
        private void SyncMethodWeave(RouMethod rouMethod)
        {
            var instructions = rouMethod.MethodDef.Body.Instructions;
            GenerateTryCatchFinally(rouMethod.MethodDef, out var tryStart, out var catchStart, out var finallyStart, out var finallyEnd, out var returnStart, out var rethrow, out var exceptionVariable, out var returnVariable);
            SetTryCatchFinally(rouMethod.MethodDef, tryStart, catchStart, finallyStart, finallyEnd);

            var rewriteArgStart = instructions.InsertBefore(tryStart, Create(OpCodes.Nop));
            var retryStart = instructions.InsertBefore(tryStart, Create(OpCodes.Nop));
            var retryVariable = rouMethod.MethodDef.Body.CreateVariable(_typeBoolRef);

            SyncOnEntry(rouMethod, rewriteArgStart, returnVariable, out var moVariables, out var contextVariable);
            SyncRewriteArguments(rouMethod, retryStart, contextVariable);
            SyncResetRetryVariable(instructions, retryVariable, retryStart);
            SyncOnException(rouMethod, moVariables, contextVariable, exceptionVariable, catchStart);
            var unhandleRethrow = instructions.InsertBefore(rethrow, Create(OpCodes.Nop));
            SyncExceptionIfRetry(rouMethod, contextVariable, tryStart, unhandleRethrow);
            SyncIfExceptionHandled(rouMethod.MethodDef, contextVariable, returnVariable, rethrow, ref finallyEnd, ref returnStart);
            if (finallyEnd == null) throw new RougamoException("sync method should set finally end after exception handler code weaved.");
            SyncOnExit(rouMethod, moVariables, contextVariable, finallyStart);
            SyncOnSuccess(rouMethod, moVariables, contextVariable, returnVariable, retryVariable, retryStart, finallyStart, finallyEnd, returnStart);
            rouMethod.MethodDef.Body.OptimizePlus();
        }

        private void SyncOnEntry(RouMethod rouMethod, Instruction rewriteArgStart, VariableDefinition? returnVariable, out VariableDefinition mosVariable, out VariableDefinition contextVariable)
        {
            var instructions = InitMosArrayVariable(rouMethod, out mosVariable);
            contextVariable = CreateMethodContextVariable(rouMethod.MethodDef, mosVariable, false, false, instructions);

            ExecuteMoMethod(Constants.METHOD_OnEntry, rouMethod.MethodDef, rouMethod.Mos.Count, mosVariable, contextVariable, instructions, false);

            instructions.Add(Create(OpCodes.Ldloc, contextVariable));
            instructions.Add(Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueReplacedRef));
            instructions.Add(Create(OpCodes.Brfalse_S, rewriteArgStart));
            if (returnVariable != null)
            {
                instructions.AddRange(ReplaceReturnValue(contextVariable, returnVariable, rouMethod.MethodDef.ReturnType));
                ExecuteMoMethod(Constants.METHOD_OnExit, rouMethod.MethodDef, rouMethod.Mos.Count, mosVariable, contextVariable, instructions, this.ConfigReverseCallEnding());
                instructions.Add(Create(OpCodes.Ldloc, returnVariable));
            }
            instructions.Add(Create(OpCodes.Ret));

            rouMethod.MethodDef.Body.Instructions.InsertBefore(rewriteArgStart, instructions);
        }

        private void SyncRewriteArguments(RouMethod rouMethod, Instruction tryStart, VariableDefinition contextVariable)
        {
            var instructions = rouMethod.MethodDef.Body.Instructions;
            instructions.InsertBefore(tryStart, Create(OpCodes.Ldloc, contextVariable));
            instructions.InsertBefore(tryStart, Create(OpCodes.Callvirt, _methodMethodContextGetRewriteArgumentsRef));
            instructions.InsertBefore(tryStart, Create(OpCodes.Brfalse_S, tryStart));
            for (var i = 0; i < rouMethod.MethodDef.Parameters.Count; i++)
            {
                SyncRewriteArgument(i, contextVariable, rouMethod.MethodDef.Parameters[i], instruction => instructions.InsertBefore(tryStart, instruction));
            }
        }

        private void SyncRewriteArgument(int index, VariableDefinition contextVariable, ParameterDefinition parameterDef, Action<Instruction> append)
        {
            if (parameterDef.IsOut) return;

            var parameterTypeRef = parameterDef.ParameterType.ImportInto(ModuleDefinition);
            var isByReference = parameterDef.ParameterType.IsByReference;
            if (isByReference)
            {
                parameterTypeRef = ((ByReferenceType)parameterDef.ParameterType).ElementType.ImportInto(ModuleDefinition);
            }
            Instruction? afterNullNop = null;
            if(parameterTypeRef.MetadataType == MetadataType.Class ||
                parameterTypeRef.MetadataType == MetadataType.Array ||
                parameterTypeRef.IsGenericParameter ||
                parameterTypeRef.IsString() || parameterTypeRef.IsNullable())
            {
                var notNullNop = Create(OpCodes.Nop);
                afterNullNop = Create(OpCodes.Nop);
                append(Create(OpCodes.Ldloc, contextVariable));
                append(Create(OpCodes.Callvirt, _methodMethodContextGetArgumentsRef));
                append(Create(OpCodes.Ldc_I4, index));
                append(Create(OpCodes.Ldelem_Ref));
                append(Create(OpCodes.Ldnull));
                append(Create(OpCodes.Ceq));
                append(Create(OpCodes.Brfalse_S, notNullNop));
                if (parameterTypeRef.IsGenericParameter || parameterTypeRef.IsNullable())
                {
                    if (isByReference)
                    {
                        append(Create(OpCodes.Ldarg_S, parameterDef));
                    }
                    else
                    {
                        append(Create(OpCodes.Ldarga_S, parameterDef));
                    }
                    append(Create(OpCodes.Initobj, parameterTypeRef));
                }
                else if (isByReference)
                {
                    append(Create(OpCodes.Ldarg_S, parameterDef));
                    append(Create(OpCodes.Ldnull));
                    append(parameterTypeRef.Stind());
                }
                else
                {
                    append(Create(OpCodes.Ldnull));
                    append(Create(OpCodes.Starg_S, parameterDef));
                }
                append(Create(OpCodes.Br_S, afterNullNop));
                append(notNullNop);
            }
            if (isByReference)
            {
                append(Create(OpCodes.Ldarg_S, parameterDef));
            }
            append(Create(OpCodes.Ldloc, contextVariable));
            append(Create(OpCodes.Callvirt, _methodMethodContextGetArgumentsRef));
            append(Create(OpCodes.Ldc_I4, index));
            append(Create(OpCodes.Ldelem_Ref));
            if (parameterTypeRef.IsUnboxable())
            {
                append(Create(OpCodes.Unbox_Any, parameterTypeRef));
            }
            else if (!parameterTypeRef.Is(typeof(object).FullName))
            {
                append(Create(OpCodes.Castclass, parameterTypeRef));
            }
            if (isByReference)
            {
                append(parameterTypeRef.Stind());
            }
            else
            {
                append(Create(OpCodes.Starg_S, parameterDef));
            }
            if(afterNullNop != null)
            {
                append(afterNullNop);
            }
        }

        private void SyncResetRetryVariable(Mono.Collections.Generic.Collection<Instruction> instructions, VariableDefinition retryVariable, Instruction retryStart)
        {
            instructions.InsertAfter(retryStart, new[]
            {
                Create(OpCodes.Ldc_I4_0),
                Create(OpCodes.Stloc, retryVariable)
            });
        }

        private void SyncOnException(RouMethod rouMethod, VariableDefinition mosVariable, VariableDefinition contextVariable, VariableDefinition exceptionVariable, Instruction catchStart)
        {
            var instructions = new List<Instruction>();
            instructions.Add(Create(OpCodes.Ldloc, contextVariable));
            instructions.Add(Create(OpCodes.Ldloc, exceptionVariable));
            instructions.Add(Create(OpCodes.Callvirt, _methodMethodContextSetExceptionRef));
            ExecuteMoMethod(Constants.METHOD_OnException, rouMethod.MethodDef, rouMethod.Mos.Count, mosVariable, contextVariable, instructions, this.ConfigReverseCallEnding());
            rouMethod.MethodDef.Body.Instructions.InsertAfter(catchStart, instructions);
        }

        private void SyncExceptionIfRetry(RouMethod rouMethod, VariableDefinition contextVariable, Instruction tryStart, Instruction unhandleRethrow)
        {
            rouMethod.MethodDef.Body.Instructions.InsertBefore(unhandleRethrow, new[]
            {
                Create(OpCodes.Ldloc, contextVariable),
                Create(OpCodes.Callvirt, _methodMethodContextGetRetryCountRef),
                Create(OpCodes.Ldc_I4_0),
                Create(OpCodes.Cgt),
                Create(OpCodes.Brfalse_S, unhandleRethrow),
                Create(OpCodes.Leave_S, tryStart)
            });
        }

        private void SyncIfExceptionHandled(MethodDefinition methodDef, VariableDefinition contextVariable, VariableDefinition? returnVariable, Instruction rethrow, ref Instruction? finallyEnd, ref Instruction? returnStart)
        {
            var returnType = methodDef.ReturnType;
            var instructions = methodDef.Body.Instructions;
            instructions.InsertBefore(rethrow, new[]
            {
                Create(OpCodes.Ldloc, contextVariable),
                Create(OpCodes.Callvirt, _methodMethodContextGetExceptionHandledRef),
                Create(OpCodes.Brfalse_S, rethrow)
            });
            if (returnVariable != null)
            {
                instructions.InsertBefore(rethrow, ReplaceReturnValue(contextVariable, returnVariable, returnType));
            }
            if (finallyEnd == null)
            {// always throws exception
                if(returnVariable == null)
                {
                    finallyEnd = Create(OpCodes.Nop);
                    returnStart = Create(OpCodes.Ret);
                    instructions.Add(finallyEnd);
                    instructions.Add(returnStart);
                }
                else
                {
                    finallyEnd = Create(OpCodes.Nop);
                    returnStart = Create(OpCodes.Ldloc, returnVariable);
                    instructions.Add(finallyEnd);
                    instructions.Add(returnStart);
                    instructions.Add(Create(OpCodes.Ret));
                }
                var finallyHandler = methodDef.Body.ExceptionHandlers.Single(x => x.HandlerType == ExceptionHandlerType.Finally && x.HandlerEnd == null);
                finallyHandler.HandlerEnd = finallyEnd;
            }
            instructions.InsertBefore(rethrow, Create(OpCodes.Leave, finallyEnd));
        }

        private void SyncOnSuccess(RouMethod rouMethod, VariableDefinition mosVariable, VariableDefinition contextVariable, VariableDefinition? returnVariable, VariableDefinition retryVariable, Instruction retryStart, Instruction finallyStart, Instruction finallyEnd, Instruction returnStart)
        {
            var returnType = rouMethod.MethodDef.ReturnType;
            var onExitFirstInstruction = finallyStart.Next;

            if (finallyStart.OpCode.Code != Code.Nop) throw new RougamoException($"sync on success finally start placeholder is not nop");
            finallyStart.OpCode = OpCodes.Ldloc;
            finallyStart.Operand = contextVariable;

            var instructions = new List<Instruction>();
            instructions.Add(Create(OpCodes.Callvirt, _methodMethodContextGetHasExceptionRef));
            instructions.Add(Create(OpCodes.Brtrue_S, onExitFirstInstruction));
            instructions.Add(Create(OpCodes.Ldloc, contextVariable));
            instructions.Add(Create(OpCodes.Callvirt, _methodMethodContextGetExceptionHandledRef));
            instructions.Add(Create(OpCodes.Brtrue_S, onExitFirstInstruction));
            if (returnStart.OpCode.Code != Code.Ret)
            {
                instructions.Add(Create(OpCodes.Ldloc, contextVariable));
                instructions.Add(returnStart.Copy());
                if (returnType.IsValueType || returnType.IsEnum(out _) && !returnType.IsArray || returnType.IsGenericParameter)
                {
                    instructions.Add(Create(OpCodes.Box, returnType.ImportInto(ModuleDefinition)));
                }
                instructions.Add(Create(OpCodes.Callvirt, _methodMethodContextSetReturnValueRef));
            }
            ExecuteMoMethod(Constants.METHOD_OnSuccess, rouMethod.MethodDef, rouMethod.Mos.Count, mosVariable, contextVariable, instructions, this.ConfigReverseCallEnding());
            ReplaceReturnValueIfNeed(rouMethod.MethodDef, instructions, finallyStart.Next, finallyEnd, contextVariable, returnVariable, retryVariable, returnType);
            SyncSuccessIfRetryStep2(rouMethod.MethodDef, retryVariable, retryStart, finallyEnd, returnStart);

            rouMethod.MethodDef.Body.Instructions.InsertAfter(finallyStart, instructions);
        }

        private void SyncOnExit(RouMethod rouMethod, VariableDefinition mosVariable, VariableDefinition contextVariable, Instruction finallyStart)
        {
            var instructions = new List<Instruction>();
            ExecuteMoMethod(Constants.METHOD_OnExit, rouMethod.MethodDef, rouMethod.Mos.Count, mosVariable, contextVariable, instructions, this.ConfigReverseCallEnding());
            rouMethod.MethodDef.Body.Instructions.InsertAfter(finallyStart, instructions);
        }

        private void ReplaceReturnValueIfNeed(MethodDefinition methodDef, List<Instruction> instructions, Instruction ifNotOffset, Instruction finallyEnd, VariableDefinition contextVariable, VariableDefinition? returnVariable, VariableDefinition retryVariable, TypeReference returnType)
        {
            if (returnVariable != null)
            {
                var tryReplaceReturn = Create(OpCodes.Ldloc, contextVariable);
                SyncSuccessIfRetryStep1(methodDef, contextVariable, retryVariable, instructions, tryReplaceReturn, finallyEnd.Previous);
                instructions.Add(tryReplaceReturn);
                instructions.Add(Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueReplacedRef));
                instructions.Add(Create(OpCodes.Brfalse_S, ifNotOffset));
                instructions.AddRange(ReplaceReturnValue(contextVariable, returnVariable, returnType));
            }
            else
            {
                SyncSuccessIfRetryStep1(methodDef, contextVariable, retryVariable, instructions, ifNotOffset, finallyEnd.Previous);
            }
        }

        private List<Instruction> ReplaceReturnValue(VariableDefinition contextVariable, VariableDefinition returnVariable, TypeReference returnType)
        {
            var instructions = new List<Instruction>();
            instructions.Add(Create(OpCodes.Ldloc, contextVariable));
            instructions.Add(Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueRef));
            if (returnType.IsValueType || (!returnType.IsArray && returnType.IsEnum(out _)) || returnType.IsGenericParameter)
            {
                instructions.Add(Create(OpCodes.Unbox_Any, returnType));
            }
            instructions.Add(Create(OpCodes.Stloc, returnVariable));
            return instructions;
        }

        private void SyncSuccessIfRetryStep1(MethodDefinition methodDef, VariableDefinition contextVariable, VariableDefinition retryVariable, List<Instruction> instructions, Instruction ifNotRetry, Instruction endFinally)
        {
            instructions.Add(Create(OpCodes.Ldloc, contextVariable));
            instructions.Add(Create(OpCodes.Callvirt, _methodMethodContextGetRetryCountRef));
            instructions.Add(Create(OpCodes.Ldc_I4_0));
            instructions.Add(Create(OpCodes.Cgt));
            instructions.Add(Create(OpCodes.Brfalse_S, ifNotRetry));
            instructions.Add(Create(OpCodes.Ldc_I4_1));
            instructions.Add(Create(OpCodes.Stloc, retryVariable));
            instructions.Add(Create(OpCodes.Leave_S, endFinally));
        }

        private void SyncSuccessIfRetryStep2(MethodDefinition methodDef, VariableDefinition retryVariable, Instruction retry, Instruction finallyEnd, Instruction returnStart)
        {
            var instructions = methodDef.Body.Instructions;

            instructions.InsertAfter(finallyEnd, new[]
            {
                Create(OpCodes.Ldloc, retryVariable),
                Create(OpCodes.Brfalse, returnStart),
                Create(OpCodes.Br_S, retry)
            });
        }
    }
}
