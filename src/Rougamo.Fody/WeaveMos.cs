using Mono.Cecil;
using Mono.Cecil.Cil;
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
                    //if (rouMethod.MethodDef.IsEmpty()) continue;
                    
                    if (rouMethod.IsIterator)
                    {
                        IteratorMethodWeave(rouMethod);
                    }
                    else if (rouMethod.IsAsyncIterator)
                    {
                        AsyncIteratorMethodWeave(rouMethod);
                    }
                    else if (rouMethod.IsAsyncTaskOrValueTask)
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

        private void SyncMethodWeave(RouMethod rouMethod)
        {
            var justThrow = GenerateTryCatchFinally(rouMethod.MethodDef, out var tryStart, out var catchStart, out var finallyStart, out var finallyEnd, out var exceptionVariable, out _);
            SetTryCatchFinally(rouMethod.MethodDef, tryStart, catchStart, finallyStart, justThrow ? null : finallyEnd);
            OnEntry(rouMethod, tryStart, out var moVariables, out var contextVariable);
            OnException(rouMethod.MethodDef, moVariables, contextVariable, exceptionVariable, catchStart);
            OnSuccess(rouMethod.MethodDef, moVariables, contextVariable, finallyStart, finallyEnd);
            OnExit(rouMethod.MethodDef, moVariables, contextVariable, finallyEnd);
            if (justThrow)
            {
                do
                {
                    rouMethod.MethodDef.Body.Instructions.Remove(finallyEnd);
                } while ((finallyEnd = finallyEnd.Next) != null);
            }
            rouMethod.MethodDef.Body.OptimizePlus();
        }

        private bool GenerateTryCatchFinally(MethodDefinition methodDef, out Instruction tryStart, out Instruction catchStart, out Instruction finallyStart, out Instruction finallyEnd, out VariableDefinition exceptionVariable, out VariableDefinition returnVariable)
        {
            var justThrow = false;
            returnVariable = null;
            var bodyIns = methodDef.Body.Instructions;
            tryStart = GetFirstInsAsNop(methodDef.Body);

            var manuaLeaves = new List<Instruction>();
            var returns = bodyIns.Where(ins => ins.OpCode == OpCodes.Ret).ToArray();
            if (returns.Length == 0)
            {
                finallyEnd = Instruction.Create(OpCodes.Nop);
                bodyIns.Add(finallyEnd);
                justThrow = true;
            }
            else
            {
                var isVoid = methodDef.ReturnType.Is(Constants.TYPE_Void);
                finallyEnd = Instruction.Create(OpCodes.Ret);
                bodyIns.Add(finallyEnd);
                if (!isVoid)
                {
                    returnVariable = methodDef.Body.CreateVariable(methodDef.ReturnType.ImportInto(ModuleDefinition));
                    var loadReturnValue = returnVariable.Ldloc();
                    bodyIns.InsertBefore(finallyEnd, loadReturnValue);
                    finallyEnd = loadReturnValue;
                }
                foreach (var @return in returns)
                {
                    if (!isVoid)
                    {
                        @return.OpCode = OpCodes.Stloc;
                        @return.Operand = returnVariable;
                        bodyIns.InsertAfter(@return, Instruction.Create(OpCodes.Leave, finallyEnd));
                    }
                    else
                    {
                        @return.OpCode = OpCodes.Leave;
                        @return.Operand = finallyEnd;
                    }
                }
            }

            exceptionVariable = methodDef.Body.CreateVariable(_typeExceptionRef);
            catchStart = Instruction.Create(OpCodes.Stloc, exceptionVariable);
            bodyIns.InsertBefore(finallyEnd, catchStart);
            bodyIns.InsertBefore(finallyEnd, Instruction.Create(OpCodes.Rethrow));
            finallyStart = Instruction.Create(OpCodes.Nop);
            bodyIns.InsertBefore(finallyEnd, finallyStart);
            bodyIns.InsertBefore(finallyEnd, Instruction.Create(OpCodes.Endfinally));

            return justThrow;
        }

        private void SetTryCatchFinally(MethodDefinition methodDef, Instruction tryStart, Instruction catchStart, Instruction finallyStart, Instruction finallyEnd)
        {
            var exceptionHandler = new ExceptionHandler(ExceptionHandlerType.Catch)
            {
                CatchType = _typeExceptionRef,
                TryStart = tryStart,
                TryEnd = catchStart,
                HandlerStart = catchStart,
                HandlerEnd = finallyStart
            };
            var finallyHandler = new ExceptionHandler(ExceptionHandlerType.Finally)
            {
                TryStart = tryStart,
                TryEnd = finallyStart,
                HandlerStart = finallyStart,
                HandlerEnd = finallyEnd
            };

            methodDef.Body.ExceptionHandlers.Add(exceptionHandler);
            methodDef.Body.ExceptionHandlers.Add(finallyHandler);
        }

        private void OnEntry(RouMethod rouMethod, Instruction tryStart, out VariableDefinition[] moVariables, out VariableDefinition contextVariable)
        {
            var ins = new List<Instruction>();
            moVariables = LoadMosOnStack(rouMethod, ins);
            contextVariable = CreateMethodContextVariable(rouMethod.MethodDef, ins);
            ExecuteMoMethod(Constants.METHOD_OnEntry, moVariables, contextVariable, ins);
            rouMethod.MethodDef.Body.Instructions.InsertBefore(tryStart, ins);
        }

        private void OnException(MethodDefinition methodDef, VariableDefinition[] moVariables, VariableDefinition contextVariable, VariableDefinition exceptionVariable, Instruction catchStart)
        {
            var ins = new List<Instruction>();
            ins.Add(Instruction.Create(OpCodes.Ldloc, contextVariable));
            ins.Add(Instruction.Create(OpCodes.Ldloc, exceptionVariable));
            ins.Add(Instruction.Create(OpCodes.Callvirt, _methodMethodContextSetExceptionRef));
            ExecuteMoMethod(Constants.METHOD_OnException, moVariables, contextVariable, ins);
            methodDef.Body.Instructions.InsertAfter(catchStart, ins);
        }

        private void OnSuccess(MethodDefinition methodDef, VariableDefinition[] moVariables, VariableDefinition contextVariable, Instruction finallyStart, Instruction finallyEnd)
        {
            var successEnd = Instruction.Create(OpCodes.Nop);
            methodDef.Body.Instructions.InsertAfter(finallyStart, successEnd);

            var ins = new List<Instruction>();
            ins.Add(Instruction.Create(OpCodes.Ldloc, contextVariable));
            ins.Add(Instruction.Create(OpCodes.Callvirt, _methodMethodContextGetHasExceptionRef));
            ins.Add(Instruction.Create(OpCodes.Brtrue_S, successEnd));
            if (finallyEnd.OpCode.Code != Code.Ret && finallyEnd.OpCode.Code != Code.Nop)
            {
                ins.Add(Instruction.Create(OpCodes.Ldloc, contextVariable));
                ins.Add(finallyEnd.Copy());
                if (methodDef.ReturnType.IsValueType || methodDef.ReturnType.IsEnum(out _) && !methodDef.ReturnType.IsArray || methodDef.ReturnType.IsGenericParameter)
                {
                    ins.Add(Instruction.Create(OpCodes.Box, methodDef.ReturnType.ImportInto(ModuleDefinition)));
                }
                ins.Add(Instruction.Create(OpCodes.Callvirt, _methodMethodContextSetReturnValueRef));
            }
            ExecuteMoMethod(Constants.METHOD_OnSuccess, moVariables, contextVariable, ins);
            methodDef.Body.Instructions.InsertAfter(finallyStart, ins);
        }

        private void OnExit(MethodDefinition methodDef, VariableDefinition[] moVariables, VariableDefinition contextVariable, Instruction finallyEnd)
        {
            var ins = new List<Instruction>();
            ExecuteMoMethod(Constants.METHOD_OnExit, moVariables, contextVariable, ins);
            methodDef.Body.Instructions.InsertBefore(finallyEnd.Previous, ins);
        }

        private void OnExceptionFromField(MethodDefinition methodDef, int mosCount, FieldReference moFieldRef, FieldReference contextFieldRef, VariableDefinition exceptionVariable, Instruction catchStart)
        {
            var ins = new List<Instruction>();
            ins.Add(Instruction.Create(OpCodes.Ldarg_0));
            ins.Add(Instruction.Create(OpCodes.Ldfld, contextFieldRef));
            ins.Add(Instruction.Create(OpCodes.Ldloc, exceptionVariable));
            ins.Add(Instruction.Create(OpCodes.Callvirt, _methodMethodContextSetExceptionRef));
            ExecuteMoMethod(Constants.METHOD_OnException, methodDef, mosCount, null, moFieldRef, contextFieldRef, catchStart.Next);
            methodDef.Body.Instructions.InsertAfter(catchStart, ins);
        }

        private Instruction GetFirstInsAsNop(MethodBody body)
        {
            var first = body.Instructions.First();
            while (first.OpCode.Code != Code.Nop || first.Next.OpCode.Code == Code.Ret)
            {
                first = Instruction.Create(OpCodes.Nop);
                body.Instructions.Insert(0, first);
            }
            return first;
        }

        private Instruction GetAsyncTaskMethodBuilderCreatePreviousIns(MethodBody body, out TypeDefinition builderDef)
        {
            for (int i = 0; i < body.Instructions.Count; i++)
            {
                var ins = body.Instructions[i];
                if (ins.OpCode == OpCodes.Call && ins.Operand is MethodReference methodRef &&
                    (methodRef.DeclaringType.FullName.StartsWith(Constants.TYPE_AsyncTaskMethodBuilder) ||
                    methodRef.DeclaringType.FullName.StartsWith(Constants.TYPE_AsyncValueTaskMethodBuilder)) &&
                    methodRef.Name == Constants.METHOD_Create)
                {
                    builderDef = methodRef.DeclaringType.Resolve();
                    return ins.Previous;
                }
            }
            throw new RougamoException("unable find call AsyncTaskMethodBuilder.Create instruction");
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

        private void InitMosField(RouMethod rouMethod, FieldReference mosFieldRef, VariableDefinition variable, Instruction insertBeforeThisIns)
        {
            var instructions = rouMethod.MethodDef.Body.Instructions;
            instructions.InsertBefore(insertBeforeThisIns, variable.LdlocOrA());
            instructions.InsertBefore(insertBeforeThisIns, Instruction.Create(OpCodes.Ldc_I4, rouMethod.Mos.Count));
            instructions.InsertBefore(insertBeforeThisIns, Instruction.Create(OpCodes.Newarr, _typeIMoRef));
            var i = 0;
            foreach (var mo in rouMethod.Mos)
            {
                instructions.InsertBefore(insertBeforeThisIns, Instruction.Create(OpCodes.Dup));
                instructions.InsertBefore(insertBeforeThisIns, Instruction.Create(OpCodes.Ldc_I4, i));
                if (mo.Attribute != null)
                {
                    instructions.InsertBefore(insertBeforeThisIns, LoadAttributeArgumentIns(mo.Attribute.ConstructorArguments));
                    instructions.InsertBefore(insertBeforeThisIns, Instruction.Create(OpCodes.Newobj, mo.Attribute.Constructor.ImportInto(ModuleDefinition)));
                    if (mo.Attribute.HasProperties)
                    {
                        instructions.InsertBefore(insertBeforeThisIns, LoadAttributePropertyDup(mo.Attribute.AttributeType.Resolve(), mo.Attribute.Properties));
                    }
                }
                else
                {
                    instructions.InsertBefore(insertBeforeThisIns, Instruction.Create(OpCodes.Newobj, mo.TypeDef.GetZeroArgsCtor().ImportInto(ModuleDefinition)));
                }
                instructions.InsertBefore(insertBeforeThisIns, Instruction.Create(OpCodes.Stelem_Ref));
                i++;
            }
            instructions.InsertBefore(insertBeforeThisIns, Instruction.Create(OpCodes.Stfld, mosFieldRef));
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
                //ins.Add(Instruction.Create(OpCodes.Nop));
            }

            return ins;
        }

        private Collection<Instruction> LoadAttributePropertyDup(TypeDefinition attrTypeDef, Collection<CustomAttributeNamedArgument> properties)
        {
            var ins = new Collection<Instruction>();
            for (var i = 0; i < properties.Count; i++)
            {
                ins.Add(Instruction.Create(OpCodes.Dup));
                ins.Add(LoadValueOnStack(properties[i].Argument.Type, properties[i].Argument.Value));
                ins.Add(Instruction.Create(OpCodes.Callvirt, attrTypeDef.RecursionImportPropertySet(ModuleDefinition, properties[i].Name)));
                //ins.Add(Instruction.Create(OpCodes.Nop));
            }

            return ins;
        }

        #endregion LoadMosOnStack

        private VariableDefinition CreateMethodContextVariable(MethodDefinition methodDef, List<Instruction> instructions)
        {
            var variable = methodDef.Body.CreateVariable(_typeMethodContextRef);

            InitMethodContext(methodDef, instructions);
            instructions.Add(Instruction.Create(OpCodes.Stloc, variable));

            return variable;
        }

        private void InitMethodContextField(RouMethod rouMethod, FieldReference contextFieldRef, VariableDefinition variable, Instruction insertBeforeThis)
        {
            var instructions = rouMethod.MethodDef.Body.Instructions;
            var contextInitIns = new List<Instruction>();
            instructions.InsertBefore(insertBeforeThis, variable.LdlocOrA());
            InitMethodContext(rouMethod.MethodDef, contextInitIns);
            instructions.InsertBefore(insertBeforeThis, contextInitIns);
            instructions.InsertBefore(insertBeforeThis, Instruction.Create(OpCodes.Stfld, contextFieldRef));
        }

        private void InitMethodContext(MethodDefinition methodDef, List<Instruction> instructions)
        {
            //instructions.AddRange(LoadMethodArgumentsOnStack(methodDef, out var argumentsVariable)); // variable
            instructions.Add(LoadThisOnStack(methodDef));
            instructions.AddRange(LoadDeclaringTypeOnStack(methodDef));
            instructions.AddRange(LoadMethodBaseOnStack(methodDef));
            instructions.AddRange(LoadMethodArgumentsOnStack(methodDef)); // dup
            //instructions.Add(Instruction.Create(OpCodes.Ldloc, argumentsVariable)); // variable
            instructions.Add(Instruction.Create(OpCodes.Newobj, _methodMethodContextCtorRef));
        }

        private void ExecuteMoMethod(string methodName, MethodDefinition methodDef, int mosCount, VariableDefinition stateMachineVariable, FieldReference mosField, FieldReference contextField, Instruction nextIns)
        {
            var instructions = methodDef.Body.Instructions;
            var flagVariable = methodDef.Body.CreateVariable(_typeIntRef);
            instructions.InsertBefore(nextIns, Instruction.Create(OpCodes.Ldc_I4_0));
            instructions.InsertBefore(nextIns, Instruction.Create(OpCodes.Stloc, flagVariable));
            var loopFirst = Instruction.Create(OpCodes.Ldloc, flagVariable);
            instructions.InsertBefore(nextIns, loopFirst);
            instructions.InsertBefore(nextIns, Instruction.Create(OpCodes.Ldc_I4, mosCount));
            instructions.InsertBefore(nextIns, Instruction.Create(OpCodes.Clt));
            instructions.InsertBefore(nextIns, Instruction.Create(OpCodes.Brfalse_S, nextIns));
            if (stateMachineVariable == null)
            {
                instructions.InsertBefore(nextIns, Instruction.Create(OpCodes.Ldarg_0));
            }
            else
            {
                instructions.InsertBefore(nextIns, stateMachineVariable.LdlocOrA());
            }
            instructions.InsertBefore(nextIns, Instruction.Create(OpCodes.Ldfld, mosField));
            instructions.InsertBefore(nextIns, Instruction.Create(OpCodes.Ldloc, flagVariable));
            instructions.InsertBefore(nextIns, Instruction.Create(OpCodes.Ldelem_Ref));
            if (stateMachineVariable == null)
            {
                instructions.InsertBefore(nextIns, Instruction.Create(OpCodes.Ldarg_0));
            }
            else
            {
                instructions.InsertBefore(nextIns, stateMachineVariable.LdlocOrA());
            }
            instructions.InsertBefore(nextIns, Instruction.Create(OpCodes.Ldfld, contextField));
            instructions.InsertBefore(nextIns, Instruction.Create(OpCodes.Callvirt, _methodIMosRef[methodName]));
            // maybe nop
            instructions.InsertBefore(nextIns, Instruction.Create(OpCodes.Ldloc, flagVariable));
            instructions.InsertBefore(nextIns, Instruction.Create(OpCodes.Ldc_I4_1));
            instructions.InsertBefore(nextIns, Instruction.Create(OpCodes.Add));
            instructions.InsertBefore(nextIns, Instruction.Create(OpCodes.Stloc, flagVariable));
            instructions.InsertBefore(nextIns, Instruction.Create(OpCodes.Br_S, loopFirst));
        }

        private void ExecuteMoMethod(string methodName, VariableDefinition[] mos, VariableDefinition methodContext, List<Instruction> instructions)
        {
            foreach (var mo in mos)
            {
                instructions.Add(Instruction.Create(OpCodes.Ldloc, mo));
                instructions.Add(Instruction.Create(OpCodes.Ldloc, methodContext));
                instructions.Add(Instruction.Create(OpCodes.Callvirt, _methodIMosRef[methodName]));
            }
        }
    }
}
