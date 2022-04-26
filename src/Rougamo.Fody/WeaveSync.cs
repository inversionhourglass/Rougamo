using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void SyncMethodWeave(RouMethod rouMethod)
        {
            GenerateTryCatchFinally(rouMethod.MethodDef, out var tryStart, out var catchStart, out var finallyStart, out var finallyEnd, out var exceptionVariable, out _);
            SetTryCatchFinally(rouMethod.MethodDef, tryStart, catchStart, finallyStart, finallyEnd);
            SyncOnEntry(rouMethod, tryStart, out var moVariables, out var contextVariable);
            SyncOnException(rouMethod.MethodDef, moVariables, contextVariable, exceptionVariable, catchStart);
            SyncOnExit(rouMethod.MethodDef, moVariables, contextVariable, finallyStart);
            SyncOnSuccess(rouMethod.MethodDef, moVariables, contextVariable, finallyStart, finallyEnd);
            rouMethod.MethodDef.Body.OptimizePlus();
        }

        private void SyncOnEntry(RouMethod rouMethod, Instruction tryStart, out VariableDefinition[] moVariables, out VariableDefinition contextVariable)
        {
            var instructions = new List<Instruction>();
            moVariables = LoadMosOnStack(rouMethod, instructions);
            contextVariable = CreateMethodContextVariable(rouMethod.MethodDef, instructions);
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

        private void SyncOnSuccess(MethodDefinition methodDef, VariableDefinition[] moVariables, VariableDefinition contextVariable, Instruction finallyStart, Instruction finallyEnd)
        {
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
            methodDef.Body.Instructions.InsertAfter(finallyStart, instructions);
        }

        private void SyncOnExit(MethodDefinition methodDef, VariableDefinition[] moVariables, VariableDefinition contextVariable, Instruction finallyStart)
        {
            var instructions = new List<Instruction>();
            ExecuteMoMethod(Constants.METHOD_OnExit, moVariables, contextVariable, instructions, this.ConfigReverseCallEnding());
            methodDef.Body.Instructions.InsertAfter(finallyStart, instructions);
        }
    }
}
