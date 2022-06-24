using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void SyncMethodWeave(RouMethod rouMethod)
        {
            GenerateTryCatchFinally(rouMethod.MethodDef, out var tryStart, out var catchStart, out var finallyStart, out var finallyEnd, out var rethrow, out var exceptionVariable, out var returnVariable);
            SetTryCatchFinally(rouMethod.MethodDef, tryStart, catchStart, finallyStart, finallyEnd);
            SyncOnEntry(rouMethod, tryStart, out var moVariables, out var contextVariable);
            SyncOnException(rouMethod.MethodDef, moVariables, contextVariable, exceptionVariable, catchStart);
            SyncIfExceptionHandled(rouMethod.MethodDef, contextVariable, ref returnVariable, rethrow, ref finallyEnd);
            SyncOnExit(rouMethod.MethodDef, moVariables, contextVariable, finallyStart);
            SyncOnSuccess(rouMethod.MethodDef, moVariables, contextVariable, returnVariable, finallyStart, finallyEnd);
            rouMethod.MethodDef.Body.OptimizePlus();
        }

        private void SyncOnEntry(RouMethod rouMethod, Instruction tryStart, out VariableDefinition[] moVariables, out VariableDefinition contextVariable)
        {
            var instructions = new List<Instruction>();
            moVariables = LoadMosOnStack(rouMethod, instructions);
            contextVariable = CreateMethodContextVariable(rouMethod.MethodDef, false, false, instructions);
            ExecuteMoMethod(Constants.METHOD_OnEntry, moVariables, contextVariable, instructions, false);
            rouMethod.MethodDef.Body.Instructions.InsertBefore(tryStart, instructions);
        }

        private void SyncOnException(MethodDefinition methodDef, VariableDefinition[] moVariables, VariableDefinition contextVariable, VariableDefinition exceptionVariable, Instruction catchStart)
        {
            var instructions = new List<Instruction>();
            instructions.Add(Instruction.Create(OpCodes.Ldloc, contextVariable));
            instructions.Add(Instruction.Create(OpCodes.Ldloc, exceptionVariable));
            instructions.Add(Instruction.Create(OpCodes.Callvirt, _methodMethodContextSetExceptionRef));
            ExecuteMoMethod(Constants.METHOD_OnException, moVariables, contextVariable, instructions, this.ConfigReverseCallEnding());
            methodDef.Body.Instructions.InsertAfter(catchStart, instructions);
        }

        private void SyncIfExceptionHandled(MethodDefinition methodDef, VariableDefinition contextVariable, ref VariableDefinition returnVariable, Instruction rethrow, ref Instruction finallyEnd)
        {
            var returnType = methodDef.ReturnType;
            var instructions = methodDef.Body.Instructions;
            instructions.InsertBefore(rethrow, new[]
            {
                Instruction.Create(OpCodes.Ldloc, contextVariable),
                Instruction.Create(OpCodes.Callvirt, _methodMethodContextGetExceptionHandledRef),
                Instruction.Create(OpCodes.Brfalse_S, rethrow)
            });
            if (!returnType.Is(Constants.TYPE_Void))
            {
                if (returnVariable == null)
                {
                    returnVariable = methodDef.Body.CreateVariable(Import(methodDef.ReturnType));
                }
                instructions.InsertBefore(rethrow, ReplaceReturnValue(contextVariable, returnVariable, returnType));
            }
            if (finallyEnd == null)
            {// always throws exception
                if(returnVariable == null)
                {
                    finallyEnd = Instruction.Create(OpCodes.Ret);
                    instructions.Add(finallyEnd);
                }
                else
                {
                    finallyEnd = Instruction.Create(OpCodes.Ldloc, returnVariable);
                    instructions.Add(finallyEnd);
                    instructions.Add(Instruction.Create(OpCodes.Ret));
                }
                var finallyHandler = methodDef.Body.ExceptionHandlers.Single(x => x.HandlerType == ExceptionHandlerType.Finally && x.HandlerEnd == null);
                finallyHandler.HandlerEnd = finallyEnd;
            }
            instructions.InsertBefore(rethrow, Instruction.Create(OpCodes.Leave, finallyEnd));
        }

        private void SyncOnSuccess(MethodDefinition methodDef, VariableDefinition[] moVariables, VariableDefinition contextVariable, VariableDefinition returnVariable, Instruction finallyStart, Instruction finallyEnd)
        {
            var returnType = methodDef.ReturnType;
            var onExitFirstInstruction = finallyStart.Next;

            if (finallyStart.OpCode.Code != Code.Nop) throw new RougamoException($"sync on success finally start placeholder is not nop");
            finallyStart.OpCode = OpCodes.Ldloc;
            finallyStart.Operand = contextVariable;

            var instructions = new List<Instruction>();
            instructions.Add(Instruction.Create(OpCodes.Callvirt, _methodMethodContextGetHasExceptionRef));
            instructions.Add(Instruction.Create(OpCodes.Brtrue_S, onExitFirstInstruction));
            if (finallyEnd != null && finallyEnd.OpCode.Code != Code.Ret)
            {
                instructions.Add(Instruction.Create(OpCodes.Ldloc, contextVariable));
                instructions.Add(finallyEnd.Copy());
                if (methodDef.ReturnType.IsValueType || methodDef.ReturnType.IsEnum(out _) && !methodDef.ReturnType.IsArray || methodDef.ReturnType.IsGenericParameter)
                {
                    instructions.Add(Instruction.Create(OpCodes.Box, methodDef.ReturnType.ImportInto(ModuleDefinition)));
                }
                instructions.Add(Instruction.Create(OpCodes.Callvirt, _methodMethodContextSetReturnValueRef));
            }
            ExecuteMoMethod(Constants.METHOD_OnSuccess, moVariables, contextVariable, instructions, this.ConfigReverseCallEnding());
            
            if (!returnType.Is(Constants.TYPE_Void))
            {
                instructions.Add(Instruction.Create(OpCodes.Ldloc, contextVariable));
                instructions.Add(Instruction.Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueReplacedRef));
                instructions.Add(Instruction.Create(OpCodes.Brfalse_S, finallyStart.Next));
                instructions.AddRange(ReplaceReturnValue(contextVariable, returnVariable, returnType));
            }

            methodDef.Body.Instructions.InsertAfter(finallyStart, instructions);
        }

        private void SyncOnExit(MethodDefinition methodDef, VariableDefinition[] moVariables, VariableDefinition contextVariable, Instruction finallyStart)
        {
            var instructions = new List<Instruction>();
            ExecuteMoMethod(Constants.METHOD_OnExit, moVariables, contextVariable, instructions, this.ConfigReverseCallEnding());
            methodDef.Body.Instructions.InsertAfter(finallyStart, instructions);
        }

        private List<Instruction> ReplaceReturnValue(VariableDefinition contextVariable, VariableDefinition returnVariable, TypeReference returnType)
        {
            var instructions = new List<Instruction>();
            instructions.Add(Instruction.Create(OpCodes.Ldloc, contextVariable));
            instructions.Add(Instruction.Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueRef));
            if (returnType.IsValueType || (!returnType.IsArray && returnType.IsEnum(out _)) || returnType.IsGenericParameter)
            {
                instructions.Add(Instruction.Create(OpCodes.Unbox_Any, returnType));
            }
            instructions.Add(Instruction.Create(OpCodes.Stloc, returnVariable));
            return instructions;
        }
    }
}
