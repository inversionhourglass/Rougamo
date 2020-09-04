using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void WeaveMos()
        {
            foreach (var rouType in _rouTypes)
            {
                foreach (var rouMethod in rouType.Methods)
                {
                    if (rouMethod.IsIterator)
                    {
                        IteratorMethodWeave(rouMethod);
                    }
                    else if (rouMethod.IsAsync)
                    {
                        AsyncMethodWeave(rouMethod);
                    }
                    else
                    {
                        SyncMethodWeave(rouMethod);
                    }
                }
            }
        }

        private void IteratorMethodWeave(RouMethod rouMethod)
        {

        }

        private void AsyncMethodWeave(RouMethod rouMethod)
        {
        }

        private void SyncMethodWeave(RouMethod rouMethod)
        {
            var methodBody = rouMethod.MethodDef.Body;

            var rouFirstInstruction = methodBody.Instructions.First();
            while (rouFirstInstruction.OpCode != OpCodes.Nop || rouFirstInstruction.Next.OpCode == OpCodes.Ret)
            {
                rouFirstInstruction = Instruction.Create(OpCodes.Nop);
                methodBody.Instructions.Insert(0, rouFirstInstruction);
            }

            var initInstruction = new List<Instruction>();
            var mos = LoadMosOnStack(rouMethod, initInstruction);
            var methodContext = InitMethodContext(rouMethod.MethodDef, initInstruction);

            ExecuteMoMethod(Constants.METHOD_OnEntry, mos, methodContext, initInstruction);

            var successInstructions = CreateSuccessInstructions(rouMethod.MethodDef, mos, methodContext, out var beforeReturnInstruction);
            var catchInstructions = CreateCatchInstructions(methodBody, mos, methodContext);
            var finallyInstructions = CreateFinallyInstructions(methodBody, mos, methodContext);

            foreach (var handler in methodBody.ExceptionHandlers)
            {
                if (handler.HandlerEnd == beforeReturnInstruction)
                {
                    var handlerEndNop = Instruction.Create(OpCodes.Nop);
                    methodBody.Instructions.InsertBefore(beforeReturnInstruction, handlerEndNop);
                    handler.HandlerEnd = handlerEndNop;
                }
            }

            methodBody.Instructions.Insert(0, initInstruction);
            //methodBody.Instructions.InsertBefore(beforeReturnInstruction, successInstructions);
            methodBody.Instructions.InsertBefore(beforeReturnInstruction, catchInstructions);
            methodBody.Instructions.InsertBefore(beforeReturnInstruction, successInstructions);
            methodBody.Instructions.InsertBefore(beforeReturnInstruction, finallyInstructions);

            var leavesFinallyEnd = Instruction.Create(OpCodes.Leave_S, beforeReturnInstruction);
            methodBody.Instructions.InsertBefore(catchInstructions.First(), leavesFinallyEnd);
            methodBody.Instructions.InsertBefore(successInstructions.First(), Instruction.Create(OpCodes.Ldloc, methodContext));
            methodBody.Instructions.InsertBefore(successInstructions.First(), Instruction.Create(OpCodes.Callvirt, _methodMethodContextGetHasExceptionRef));
            methodBody.Instructions.InsertBefore(successInstructions.First(), Instruction.Create(OpCodes.Brfalse_S, finallyInstructions.First()));

            var exceptionHandler = new ExceptionHandler(ExceptionHandlerType.Catch)
            {
                CatchType = _typeExceptionRef,
                TryStart = rouFirstInstruction,
                TryEnd = catchInstructions.First(),
                HandlerStart = catchInstructions.First(),
                HandlerEnd = catchInstructions.Last()
            };
            var finallyHandler = new ExceptionHandler(ExceptionHandlerType.Finally)
            {
                TryStart = rouFirstInstruction,
                TryEnd = catchInstructions.Last(),
                HandlerStart = catchInstructions.Last(),
                HandlerEnd = beforeReturnInstruction
            };

            methodBody.ExceptionHandlers.Add(exceptionHandler);
            methodBody.ExceptionHandlers.Add(finallyHandler);

            rouMethod.MethodDef.Body.Optimize();
        }

        private void SyncMethodWeave1(RouMethod rouMethod)
        {
            TryCatchFinally(rouMethod.MethodDef, out var tryStart, out var catchStart, out var finallyStart, out var finallyEnd, out var exceptionVariable);
            OnEntry(rouMethod, tryStart, out var moVariables, out var contextVariable);
        }

        private void TryCatchFinally(MethodDefinition methodDef, out Instruction tryStart, out Instruction catchStart, out Instruction finallyStart, out Instruction finallyEnd, out VariableDefinition exceptionVariable)
        {
            var bodyIns = methodDef.Body.Instructions;
            tryStart = bodyIns.First();
            while (tryStart.OpCode != OpCodes.Nop || tryStart.Next.OpCode == OpCodes.Ret)
            {
                tryStart = Instruction.Create(OpCodes.Nop);
                bodyIns.Insert(0, tryStart);
            }

            var returns = methodDef.Body.Instructions.Where(ins => ins.OpCode == OpCodes.Ret);
            if (returns.Count() != 1)
            {
                throw new RougamoException($"[{methodDef.FullName}] multiple ret found");
            }
            var returnIns = returns.Single();
            finallyEnd = returnIns.Previous;
            if (methodDef.ReturnType.Is(Constants.TYPE_Void))
            {
                if (finallyEnd == null || finallyEnd.OpCode != OpCodes.Nop)
                {
                    finallyEnd = Instruction.Create(OpCodes.Nop);
                    methodDef.Body.Instructions.InsertBefore(returnIns, finallyEnd);
                }
            }
            else
            {
                while (true)
                {
                    if (finallyEnd == null) throw new RougamoException($"[{methodDef.FullName}] not found Ldloc before ret");
                    if (finallyEnd.OpCode == OpCodes.Ldloc_0 ||
                        finallyEnd.OpCode == OpCodes.Ldloc_1 ||
                        finallyEnd.OpCode == OpCodes.Ldloc_2 ||
                        finallyEnd.OpCode == OpCodes.Ldloc_3 ||
                        finallyEnd.OpCode == OpCodes.Ldloc ||
                        finallyEnd.OpCode == OpCodes.Ldloc_S) break;
                    finallyEnd = finallyEnd.Previous;
                }
            }

            exceptionVariable = methodDef.Body.CreateVariable(_typeExceptionRef);
            bodyIns.InsertBefore(finallyEnd, Instruction.Create(OpCodes.Leave_S, finallyEnd));
            catchStart = Instruction.Create(OpCodes.Stloc, exceptionVariable);
            bodyIns.InsertBefore(finallyEnd, catchStart);
            bodyIns.InsertBefore(finallyEnd, Instruction.Create(OpCodes.Ldloc, exceptionVariable));
            bodyIns.InsertBefore(finallyEnd, Instruction.Create(OpCodes.Throw));
            finallyStart = Instruction.Create(OpCodes.Nop);
            bodyIns.InsertBefore(finallyEnd, finallyStart);
            bodyIns.InsertBefore(finallyEnd, Instruction.Create(OpCodes.Endfinally));
        }

        private void OnEntry(RouMethod rouMethod, Instruction tryStart, out VariableDefinition[] moVariables, out VariableDefinition contextVariable)
        {
            var ins = new List<Instruction>();
            moVariables = LoadMosOnStack(rouMethod, ins);
            contextVariable = InitMethodContext(rouMethod.MethodDef, ins);
            ExecuteMoMethod(Constants.METHOD_OnEntry, moVariables, contextVariable, ins);
            rouMethod.MethodDef.Body.Instructions.InsertBefore(tryStart, ins);
        }

        private void OnException(VariableDefinition[] moVariables, VariableDefinition contextVariable)
        {

        }

        private IList<Instruction> CreateCatchInstructions(MethodBody methodBody, VariableDefinition[] mos, VariableDefinition methodContext)
        {
            var catchInstructions = new List<Instruction>();

            var exceptionVariable = methodBody.CreateVariable(_typeExceptionRef);
            catchInstructions.Add(Instruction.Create(OpCodes.Stloc, exceptionVariable));
            catchInstructions.Add(Instruction.Create(OpCodes.Ldloc, methodContext));
            catchInstructions.Add(Instruction.Create(OpCodes.Ldloc, exceptionVariable));
            catchInstructions.Add(Instruction.Create(OpCodes.Callvirt, _methodMethodContextSetExceptionRef));
            ExecuteMoMethod(Constants.METHOD_OnException, mos, methodContext, catchInstructions);
            catchInstructions.Add(Instruction.Create(OpCodes.Ldloc, exceptionVariable));
            catchInstructions.Add(Instruction.Create(OpCodes.Throw));
            catchInstructions.Add(Instruction.Create(OpCodes.Nop));

            return catchInstructions;
        }

        private IList<Instruction> CreateFinallyInstructions(MethodBody methodBody, VariableDefinition[] mos, VariableDefinition methodContext)
        {
            var finallyInstructions = new List<Instruction>();

            ExecuteMoMethod(Constants.METHOD_OnExit, mos, methodContext, finallyInstructions);
            finallyInstructions.Add(Instruction.Create(OpCodes.Endfinally));

            return finallyInstructions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="methodDef"></param>
        /// <param name="mos"></param>
        /// <param name="methodContext"></param>
        /// <param name="beforeReturnInstruction">nop if void, otherwise ldloc</param>
        /// <returns></returns>
        private IList<Instruction> CreateSuccessInstructions(MethodDefinition methodDef, VariableDefinition[] mos, VariableDefinition methodContext, out Instruction beforeReturnInstruction)
        {
            var beforeReturnInstructions = methodDef.Body.Instructions.Where(ins => ins.OpCode == OpCodes.Ret);
            if (beforeReturnInstructions.Count() != 1)
            {
                throw new RougamoException($"[{methodDef.FullName}] multiple ret found");
            }
            var successInstructions = new List<Instruction>();
            beforeReturnInstruction = beforeReturnInstructions.Single();
            successInstructions.Add(Instruction.Create(OpCodes.Ldloc, methodContext));
            if (methodDef.ReturnType.Is(Constants.TYPE_Void))
            {
                var beforeReturnNop = beforeReturnInstruction.Previous;
                if (beforeReturnNop == null || beforeReturnNop.OpCode != OpCodes.Nop)
                {
                    beforeReturnNop = Instruction.Create(OpCodes.Nop);
                    methodDef.Body.Instructions.InsertBefore(beforeReturnInstruction, beforeReturnNop);
                }
                beforeReturnInstruction = beforeReturnNop;
                successInstructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                successInstructions.Add(Instruction.Create(OpCodes.Callvirt, _methodMethodContextSetHasReturnValueRef));
            }
            else
            {
                while (true)
                {
                    beforeReturnInstruction = beforeReturnInstruction.Previous;
                    if (beforeReturnInstruction == null) throw new RougamoException($"[{methodDef.FullName}] not found Ldloc before ret");
                    if (beforeReturnInstruction.OpCode == OpCodes.Ldloc_0 ||
                        beforeReturnInstruction.OpCode == OpCodes.Ldloc_1 ||
                        beforeReturnInstruction.OpCode == OpCodes.Ldloc_2 ||
                        beforeReturnInstruction.OpCode == OpCodes.Ldloc_3 ||
                        beforeReturnInstruction.OpCode == OpCodes.Ldloc ||
                        beforeReturnInstruction.OpCode == OpCodes.Ldloc_S) break;
                }
                successInstructions.Add(beforeReturnInstruction.Copy());
                if (methodDef.ReturnType.IsValueType || methodDef.ReturnType.IsEnum(out _) && !methodDef.ReturnType.IsArray)
                {
                    successInstructions.Add(Instruction.Create(OpCodes.Box, _typeObjectRef));
                }
                successInstructions.Add(Instruction.Create(OpCodes.Callvirt, _methodMethodContextSetReturnValueRef));
            }
            ExecuteMoMethod(Constants.METHOD_OnSuccess, mos, methodContext, successInstructions);

            return successInstructions;
        }

        #region LoadMosOnStack

        private VariableDefinition[] LoadMosOnStack(RouMethod rouMethod, List<Instruction> instructions)
        {
            var mos = new VariableDefinition[rouMethod.Mos.Count];
            var i = 0;
            foreach (var mo in rouMethod.Mos)
            {
                mos[i++] = LoadMoOnStack(mo, rouMethod.MethodDef.Body, instructions);
            }
            return mos;
        }

        private VariableDefinition LoadMoOnStack(Mo mo, MethodBody methodBody, List<Instruction> instructions)
        {
            VariableDefinition variable;
            if (mo.Attribute != null)
            {
                variable = methodBody.CreateVariable(mo.Attribute.AttributeType.ImportInto(ModuleDefinition));
                instructions.AddRange(LoadAttributeArgumentIns(mo.Attribute.ConstructorArguments));
                instructions.Add(Instruction.Create(OpCodes.Newobj, mo.Attribute.Constructor.ImportInto(ModuleDefinition)));
                instructions.Add(Instruction.Create(OpCodes.Stloc, variable));
                if (mo.Attribute.HasProperties)
                {
                    instructions.AddRange(LoadAttributePropertyIns(mo.Attribute.AttributeType.Resolve(), mo.Attribute.Properties, variable));
                }
            }
            else
            {
                variable = methodBody.CreateVariable(mo.TypeDef.ImportInto(ModuleDefinition));
                instructions.Add(Instruction.Create(OpCodes.Newobj, mo.TypeDef.GetZeroArgsCtor().ImportInto(ModuleDefinition)));
                instructions.Add(Instruction.Create(OpCodes.Stloc, variable));
            }
            return variable;
        }

        private Collection<Instruction> LoadAttributeArgumentIns(Collection<CustomAttributeArgument> arguments)
        {
            var ins = new Collection<Instruction>();
            foreach (var arg in arguments)
            {
                ins.Add(LoadValueOnStack(arg.Type, arg.Value));
            }
            return ins;
        }

        private Collection<Instruction> LoadAttributePropertyIns(TypeDefinition attrTypeDef, Collection<CustomAttributeNamedArgument> properties, VariableDefinition attributeDef)
        {
            var ins = new Collection<Instruction>();
            for (var i = 0; i < properties.Count; i++)
            {
                ins.Add(Instruction.Create(OpCodes.Ldloc, attributeDef));
                ins.Add(LoadValueOnStack(properties[i].Argument.Type, properties[i].Argument.Value));
                ins.Add(Instruction.Create(OpCodes.Callvirt, attrTypeDef.RecursionImportPropertySet(ModuleDefinition, properties[i].Name)));
                ins.Add(Instruction.Create(OpCodes.Nop));
            }

            return ins;
        }

        #endregion LoadMosOnStack

        private VariableDefinition InitMethodContext(MethodDefinition methodDef, List<Instruction> instructions)
        {
            var variable = methodDef.Body.CreateVariable(_typeMethodContextRef);

            //instructions.AddRange(LoadMethodArgumentsOnStack(methodDef, out var argumentsVariable)); // variable
            instructions.Add(LoadThisOnStack(methodDef));
            instructions.AddRange(LoadDeclaringTypeOnStack(methodDef));
            instructions.AddRange(LoadMethodBaseOnStack(methodDef));
            instructions.AddRange(LoadMethodArgumentsOnStack(methodDef)); // dup
            //instructions.Add(Instruction.Create(OpCodes.Ldloc, argumentsVariable)); // variable
            instructions.Add(Instruction.Create(OpCodes.Newobj, _methodMethodContextCtorRef));
            instructions.Add(Instruction.Create(OpCodes.Stloc, variable));

            return variable;
        }

        private void ExecuteMoMethod(string methodName, VariableDefinition[] mos, VariableDefinition methodContext, List<Instruction> instructions)
        {
            foreach (var mo in mos)
            {
                instructions.Add(Instruction.Create(OpCodes.Ldloc, mo));
                instructions.Add(Instruction.Create(OpCodes.Ldloc, methodContext));
                var methodRef = mo.VariableType.Resolve().RecursionImportMethod(ModuleDefinition, methodName, md => md.Parameters.Count == 1 && md.Parameters.First().ParameterType.Is(Constants.TYPE_MethodContext));
                instructions.Add(Instruction.Create(OpCodes.Callvirt, methodRef));
            }
        }
    }
}
