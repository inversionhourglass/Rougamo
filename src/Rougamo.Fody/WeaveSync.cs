using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void SyncMethodWeave(RouMethod rouMethod)
        {
            GenerateTryCatchFinally(rouMethod.MethodDef, out var tryStart, out var catchStart, out var finallyStart, out var finallyEnd, out var rethrow, out var exceptionVariable, out var returnVariable);
            SetTryCatchFinally(rouMethod.MethodDef, tryStart, catchStart, finallyStart, finallyEnd);
            SyncOnEntry(rouMethod, tryStart, returnVariable, out var moVariables, out var contextVariable);
            SyncOnException(rouMethod.MethodDef, moVariables, contextVariable, exceptionVariable, catchStart);
            SyncIfExceptionHandled(rouMethod.MethodDef, contextVariable, returnVariable, rethrow, ref finallyEnd);
            if (finallyEnd == null) throw new RougamoException("sync method should set finally end after exception handler code weaved.");
            SyncOnExit(rouMethod.MethodDef, moVariables, contextVariable, finallyStart);
            SyncOnSuccess(rouMethod.MethodDef, moVariables, contextVariable, returnVariable, finallyStart, finallyEnd);
            rouMethod.MethodDef.Body.OptimizePlus();
        }

        private void SyncOnEntry(RouMethod rouMethod, Instruction tryStart, VariableDefinition? returnVariable, out VariableDefinition[] moVariables, out VariableDefinition contextVariable)
        {
            var instructions = new List<Instruction>();
            moVariables = LoadMosOnStack(rouMethod, instructions);
            contextVariable = CreateMethodContextVariable(rouMethod.MethodDef, false, false, instructions);

            ExecuteMoMethod(Constants.METHOD_OnEntry, moVariables, contextVariable, instructions, false);

            instructions.Add(Create(OpCodes.Ldloc, contextVariable));
            instructions.Add(Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueReplacedRef));
            instructions.Add(Create(OpCodes.Brfalse_S, tryStart));
            if (returnVariable != null)
            {
                instructions.AddRange(ReplaceReturnValue(contextVariable, returnVariable, rouMethod.MethodDef.ReturnType));
                ExecuteMoMethod(Constants.METHOD_OnExit, moVariables, contextVariable, instructions, this.ConfigReverseCallEnding());
                instructions.Add(Create(OpCodes.Ldloc, returnVariable));
            }
            instructions.Add(Create(OpCodes.Ret));

            rouMethod.MethodDef.Body.Instructions.InsertBefore(tryStart, instructions);
        }

        private void SyncOnException(MethodDefinition methodDef, VariableDefinition[] moVariables, VariableDefinition contextVariable, VariableDefinition exceptionVariable, Instruction catchStart)
        {
            var instructions = new List<Instruction>();
            instructions.Add(Create(OpCodes.Ldloc, contextVariable));
            instructions.Add(Create(OpCodes.Ldloc, exceptionVariable));
            instructions.Add(Create(OpCodes.Callvirt, _methodMethodContextSetExceptionRef));
            ExecuteMoMethod(Constants.METHOD_OnException, moVariables, contextVariable, instructions, this.ConfigReverseCallEnding());
            methodDef.Body.Instructions.InsertAfter(catchStart, instructions);
        }

        private void SyncIfExceptionHandled(MethodDefinition methodDef, VariableDefinition contextVariable, VariableDefinition? returnVariable, Instruction rethrow, ref Instruction? finallyEnd)
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
                    finallyEnd = Create(OpCodes.Ret);
                    instructions.Add(finallyEnd);
                }
                else
                {
                    finallyEnd = Create(OpCodes.Ldloc, returnVariable);
                    instructions.Add(finallyEnd);
                    instructions.Add(Create(OpCodes.Ret));
                }
                var finallyHandler = methodDef.Body.ExceptionHandlers.Single(x => x.HandlerType == ExceptionHandlerType.Finally && x.HandlerEnd == null);
                finallyHandler.HandlerEnd = finallyEnd;
            }
            instructions.InsertBefore(rethrow, Create(OpCodes.Leave, finallyEnd));
        }

        private void SyncOnSuccess(MethodDefinition methodDef, VariableDefinition[] moVariables, VariableDefinition contextVariable, VariableDefinition? returnVariable, Instruction finallyStart, Instruction finallyEnd)
        {
            var returnType = methodDef.ReturnType;
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
            if (finallyEnd != null && finallyEnd.OpCode.Code != Code.Ret)
            {
                instructions.Add(Create(OpCodes.Ldloc, contextVariable));
                instructions.Add(finallyEnd.Copy());
                if (returnType.IsValueType || returnType.IsEnum(out _) && !returnType.IsArray || returnType.IsGenericParameter)
                {
                    instructions.Add(Create(OpCodes.Box, returnType.ImportInto(ModuleDefinition)));
                }
                instructions.Add(Create(OpCodes.Callvirt, _methodMethodContextSetReturnValueRef));
            }
            ExecuteMoMethod(Constants.METHOD_OnSuccess, moVariables, contextVariable, instructions, this.ConfigReverseCallEnding());
            ReplaceReturnValueIfNeed(instructions, finallyStart.Next, contextVariable, returnVariable, returnType);

            methodDef.Body.Instructions.InsertAfter(finallyStart, instructions);
        }

        private void SyncOnExit(MethodDefinition methodDef, VariableDefinition[] moVariables, VariableDefinition contextVariable, Instruction finallyStart)
        {
            var instructions = new List<Instruction>();
            ExecuteMoMethod(Constants.METHOD_OnExit, moVariables, contextVariable, instructions, this.ConfigReverseCallEnding());
            methodDef.Body.Instructions.InsertAfter(finallyStart, instructions);
        }

        private void ReplaceReturnValueIfNeed(List<Instruction> instructions, Instruction ifNotOffset, VariableDefinition contextVariable, VariableDefinition? returnVariable, TypeReference returnType)
        {
            if (returnVariable != null)
            {
                instructions.Add(Create(OpCodes.Ldloc, contextVariable));
                instructions.Add(Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueReplacedRef));
                instructions.Add(Create(OpCodes.Brfalse_S, ifNotOffset));
                instructions.AddRange(ReplaceReturnValue(contextVariable, returnVariable, returnType));
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
    }
}
