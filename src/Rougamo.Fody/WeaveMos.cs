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
            // todo: IteratorMethodWeave
            //rouMethod.MethodDef.Body.OptimizePlus();
        }

        private void AsyncMethodWeave(RouMethod rouMethod)
        {
            var returnTypeRef = AsyncOnEntry(rouMethod, out var stateTypeDef, out var mosFieldDef, out var contextFieldDef, out var builderDef);
            AsyncStateMachineExtract(stateTypeDef, builderDef, out var builderFieldDef, out var moveNextMethodDef, out var setResultIns);
            AsyncOnExceptionWithExit(rouMethod, moveNextMethodDef, mosFieldDef, contextFieldDef);
            AsyncOnSuccessWithExit(rouMethod, moveNextMethodDef, mosFieldDef, contextFieldDef, setResultIns, returnTypeRef);
            moveNextMethodDef.Body.OptimizePlus();
            rouMethod.MethodDef.Body.OptimizePlus();
        }

        private TypeReference AsyncOnEntry(RouMethod rouMethod, out TypeDefinition stateTypeDef, out FieldDefinition mosFieldDef, out FieldDefinition contextFieldDef, out TypeDefinition builderDef)
        {
            AsyncFieldDefinition(rouMethod, out stateTypeDef, out mosFieldDef, out contextFieldDef);
            var tStateTypeDef = stateTypeDef;
            var stateMachineVariable = rouMethod.MethodDef.Body.Variables.Single(x => x.VariableType.Resolve() == tStateTypeDef);
            var taskBuilderPreviousIns = GetAsyncTaskMethodBuilderCreatePreviousIns(rouMethod.MethodDef.Body, out builderDef);
            InitMosField(rouMethod, mosFieldDef, stateMachineVariable, taskBuilderPreviousIns);
            InitMethodContextField(rouMethod, contextFieldDef, stateMachineVariable, taskBuilderPreviousIns);
            ExecuteMoMethod(Constants.METHOD_OnEntry, rouMethod.MethodDef, rouMethod.Mos.Count, stateMachineVariable, mosFieldDef, contextFieldDef, taskBuilderPreviousIns);

            return rouMethod.MethodDef.ReturnType.Is(Constants.TYPE_Task) ? null : ((GenericInstanceType)rouMethod.MethodDef.ReturnType).GenericArguments[0];
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

        private void AsyncStateMachineExtract(TypeDefinition stateMachineTypeDef, TypeDefinition builderTypeDef, out FieldDefinition builderFieldDef, out MethodDefinition moveNextMethodDef, out Instruction setResultIns)
        {
            builderFieldDef = stateMachineTypeDef.Fields.Single(f => f.FieldType.Resolve() == builderTypeDef);
            moveNextMethodDef = stateMachineTypeDef.Methods.Single(m => m.Name == Constants.METHOD_MoveNext);
            setResultIns = moveNextMethodDef.Body.Instructions.SingleOrDefault(x => x.Operand is MethodReference methodRef
                                        && methodRef.DeclaringType.Resolve() == builderTypeDef && methodRef.Name == Constants.METHOD_SetResult);
        }

        private void AsyncOnExceptionWithExit(RouMethod rouMethod, MethodDefinition moveNextMethodDef, FieldDefinition mosFieldDef, FieldDefinition contextFieldDef)
        {
            var instructions = moveNextMethodDef.Body.Instructions;
            var exceptionHandler = OuterExceptionHandler(moveNextMethodDef.Body);
            var ldlocException = exceptionHandler.HandlerStart.Stloc2Ldloc($"[{rouMethod.MethodDef.FullName}] exception handler first instruction is not stloc.s exception");

            var next = exceptionHandler.HandlerStart.Next;
            instructions.InsertBefore(next, Instruction.Create(OpCodes.Ldarg_0));
            instructions.InsertBefore(next, Instruction.Create(OpCodes.Ldfld, contextFieldDef));
            instructions.InsertBefore(next, ldlocException);
            instructions.InsertBefore(next, Instruction.Create(OpCodes.Callvirt, _methodMethodContextSetExceptionRef));
            var splitNop = Instruction.Create(OpCodes.Nop);
            instructions.InsertBefore(next, splitNop);
            ExecuteMoMethod(Constants.METHOD_OnException, moveNextMethodDef, rouMethod.Mos.Count, null, mosFieldDef, contextFieldDef, splitNop);
            ExecuteMoMethod(Constants.METHOD_OnExit, moveNextMethodDef, rouMethod.Mos.Count, null, mosFieldDef, contextFieldDef, next);
        }

        private void AsyncOnSuccessWithExit(RouMethod rouMethod, MethodDefinition moveNextMethodDef, FieldDefinition mosFieldDef, FieldDefinition contextFieldDef, Instruction setResultIns, TypeReference returnTypeRef)
        {
            if (setResultIns == null) return; // 100% throw exception

            var instructions = moveNextMethodDef.Body.Instructions;
            var closeThisIns = setResultIns.ClosePreviousLdarg0(rouMethod.MethodDef);
            if (returnTypeRef != null)
            {
                var ldlocResult = setResultIns.Previous.Copy();
                instructions.InsertBefore(closeThisIns, Instruction.Create(OpCodes.Ldarg_0));
                instructions.InsertBefore(closeThisIns, Instruction.Create(OpCodes.Ldfld, contextFieldDef));
                instructions.InsertBefore(closeThisIns, ldlocResult);
                if(returnTypeRef.IsValueType || returnTypeRef.IsEnum(out _) && !returnTypeRef.IsArray)
                {
                    instructions.InsertBefore(closeThisIns, Instruction.Create(OpCodes.Box, returnTypeRef.ImportInto(ModuleDefinition)));
                }
                instructions.InsertBefore(closeThisIns, Instruction.Create(OpCodes.Callvirt, _methodMethodContextSetReturnValueRef));
            }
            var splitNop = Instruction.Create(OpCodes.Nop);
            instructions.InsertBefore(closeThisIns, splitNop);
            ExecuteMoMethod(Constants.METHOD_OnSuccess, moveNextMethodDef, rouMethod.Mos.Count, null, mosFieldDef, contextFieldDef, splitNop);
            ExecuteMoMethod(Constants.METHOD_OnExit, moveNextMethodDef, rouMethod.Mos.Count, null, mosFieldDef, contextFieldDef, closeThisIns);
        }

        private ExceptionHandler OuterExceptionHandler(MethodBody methodBody)
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

        private void SyncMethodWeave(RouMethod rouMethod)
        {
            GenerateTryCatchFinally(rouMethod.MethodDef, out var tryStart, out var catchStart, out var finallyStart, out var finallyEnd, out var exceptionVariable);
            SetTryCatchFinally(rouMethod.MethodDef, tryStart, catchStart, finallyStart, finallyEnd);
            OnEntry(rouMethod, tryStart, out var moVariables, out var contextVariable);
            OnException(rouMethod.MethodDef, moVariables, contextVariable, exceptionVariable, catchStart);
            OnSuccess(rouMethod.MethodDef, moVariables, contextVariable, finallyStart, finallyEnd);
            OnExit(rouMethod.MethodDef, moVariables, contextVariable, finallyEnd);
            rouMethod.MethodDef.Body.OptimizePlus();
        }

        private void GenerateTryCatchFinally(MethodDefinition methodDef, out Instruction tryStart, out Instruction catchStart, out Instruction finallyStart, out Instruction finallyEnd, out VariableDefinition exceptionVariable)
        {
            var skipRedirects = new List<Instruction>();
            var bodyIns = methodDef.Body.Instructions;
            tryStart = GetFirstInsAsNop(methodDef.Body);

            var leaves = new List<Instruction>();
            var returns = bodyIns.Where(ins => ins.OpCode == OpCodes.Ret).ToArray();
            var returnIns = returns.Length == 1 ? returns[0] : MergeReturns(methodDef, returns.ToArray(), leaves);
            finallyEnd = returnIns.Previous;
            if (methodDef.ReturnType.Is(Constants.TYPE_Void))
            {
                if (finallyEnd == null || finallyEnd.OpCode != OpCodes.Nop)
                {
                    finallyEnd = Instruction.Create(OpCodes.Nop);
                    bodyIns.InsertBefore(returnIns, finallyEnd);
                }
            }
            else
            {
                while (true)
                {
                    if (finallyEnd == null) throw new RougamoException($"[{methodDef.FullName}] not found Ldloc before ret");
                    if (finallyEnd.OpCode.Code != Code.Nop) break;
                    finallyEnd = finallyEnd.Previous;
                }
                if (finallyEnd.OpCode.Code != Code.Ldloc_0 && finallyEnd.OpCode.Code != Code.Ldloc_1 &&
                    finallyEnd.OpCode.Code != Code.Ldloc_2 && finallyEnd.OpCode.Code != Code.Ldloc_3 &&
                    finallyEnd.OpCode.Code != Code.Ldloc && finallyEnd.OpCode.Code != Code.Ldloc_S)
                {
                    var returnVariable = methodDef.Body.CreateVariable(methodDef.ReturnType.ImportInto(ModuleDefinition));
                    var next = finallyEnd.Next;
                    finallyEnd = returnVariable.LdlocOrA();
                    bodyIns.InsertBefore(next, Instruction.Create(OpCodes.Stloc, returnVariable));
                    bodyIns.InsertBefore(next, finallyEnd);
                }
            }

            exceptionVariable = methodDef.Body.CreateVariable(_typeExceptionRef);
            if (!leaves.Contains(finallyEnd.Previous))
            {
                bodyIns.InsertBefore(finallyEnd, skipRedirects.AddAndGet(Instruction.Create(OpCodes.Leave, finallyEnd)));
            }
            catchStart = Instruction.Create(OpCodes.Stloc, exceptionVariable);
            bodyIns.InsertBefore(finallyEnd, catchStart);
            bodyIns.InsertBefore(finallyEnd, Instruction.Create(OpCodes.Rethrow));
            //bodyIns.InsertBefore(finallyEnd, skipRedirects.AddAndGet(Instruction.Create(OpCodes.Leave, finallyEnd))); // maybe
            finallyStart = Instruction.Create(OpCodes.Nop);
            bodyIns.InsertBefore(finallyEnd, finallyStart);
            bodyIns.InsertBefore(finallyEnd, Instruction.Create(OpCodes.Endfinally));

            AdjustRedirects(methodDef, skipRedirects, catchStart, finallyEnd);
        }

        private Instruction MergeReturns(MethodDefinition methodDef, Instruction[] returns, List<Instruction> leaves)
        {
            var returnVariable = methodDef.Body.CreateVariable(methodDef.ReturnType.ImportInto(ModuleDefinition));
            var ldlocReturn = returnVariable.LdlocOrA();
            var ret = Instruction.Create(OpCodes.Ret);
            methodDef.Body.Instructions.Add(ldlocReturn);
            methodDef.Body.Instructions.Add(ret);

            foreach (var @return in returns)
            {
                @return.OpCode = OpCodes.Stloc;
                @return.Operand = returnVariable;
                methodDef.Body.Instructions.InsertAfter(@return, leaves.AddAndGet(Instruction.Create(OpCodes.Leave, ldlocReturn)));
            }

            return ret;
        }

        private void AdjustRedirects(MethodDefinition methodDef, List<Instruction> skipRedirects, Instruction catchStart, Instruction finallyEnd)
        {
            var closePreviousOffset = catchStart.Previous.ClosePreviousOffset(methodDef);
            var redirectToEnds = methodDef.Body.Instructions.Where(x => _brs.Contains(x.OpCode.Code) && ((Instruction)x.Operand).Offset > closePreviousOffset.Offset && !skipRedirects.Contains(x));
            if (redirectToEnds.Any())
            {
                foreach (var item in redirectToEnds)
                {
                    item.Operand = catchStart.Previous;
                }
            }

            foreach (var handler in methodDef.Body.ExceptionHandlers)
            {
                if (handler.HandlerEnd.Offset > closePreviousOffset.Offset)
                {
                    handler.HandlerEnd = catchStart.Previous;
                }
            }
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
            if (finallyEnd.OpCode.Code != Code.Nop)
            {
                ins.Add(Instruction.Create(OpCodes.Ldloc, contextVariable));
                ins.Add(finallyEnd.Copy());
                if (methodDef.ReturnType.IsValueType || methodDef.ReturnType.IsEnum(out _) && !methodDef.ReturnType.IsArray)
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
                if (ins.OpCode == OpCodes.Call && ins.Operand is MethodReference methodRef && methodRef.DeclaringType.FullName.StartsWith(Constants.TYPE_AsyncTaskMethodBuilder) && methodRef.Name == Constants.METHOD_Create)
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

        private void InitMosField(RouMethod rouMethod, FieldDefinition mosFieldDef, VariableDefinition stateMachineVariable, Instruction taskBuilderPreviousIns)
        {
            var instructions = rouMethod.MethodDef.Body.Instructions;
            instructions.InsertBefore(taskBuilderPreviousIns, Instruction.Create(OpCodes.Ldloca, stateMachineVariable));
            instructions.InsertBefore(taskBuilderPreviousIns, Instruction.Create(OpCodes.Ldc_I4, rouMethod.Mos.Count));
            instructions.InsertBefore(taskBuilderPreviousIns, Instruction.Create(OpCodes.Newarr, _typeIMoRef));
            var i = 0;
            foreach (var mo in rouMethod.Mos)
            {
                instructions.InsertBefore(taskBuilderPreviousIns, Instruction.Create(OpCodes.Dup));
                instructions.InsertBefore(taskBuilderPreviousIns, Instruction.Create(OpCodes.Ldc_I4, i));
                if (mo.Attribute != null)
                {
                    instructions.InsertBefore(taskBuilderPreviousIns, LoadAttributeArgumentIns(mo.Attribute.ConstructorArguments));
                    instructions.InsertBefore(taskBuilderPreviousIns, Instruction.Create(OpCodes.Newobj, mo.Attribute.Constructor.ImportInto(ModuleDefinition)));
                    if (mo.Attribute.HasProperties)
                    {
                        instructions.InsertBefore(taskBuilderPreviousIns, LoadAttributePropertyDup(mo.Attribute.AttributeType.Resolve(), mo.Attribute.Properties));
                    }
                }
                else
                {
                    instructions.InsertBefore(taskBuilderPreviousIns, Instruction.Create(OpCodes.Newobj, mo.TypeDef.GetZeroArgsCtor().ImportInto(ModuleDefinition)));
                }
                instructions.InsertBefore(taskBuilderPreviousIns, Instruction.Create(OpCodes.Stelem_Ref));
                i++;
            }
            instructions.InsertBefore(taskBuilderPreviousIns, Instruction.Create(OpCodes.Stfld, mosFieldDef));
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

        private Collection<Instruction> LoadAttributePropertyDup(TypeDefinition attrTypeDef, Collection<CustomAttributeNamedArgument> properties)
        {
            var ins = new Collection<Instruction>();
            for (var i = 0; i < properties.Count; i++)
            {
                ins.Add(Instruction.Create(OpCodes.Dup));
                ins.Add(LoadValueOnStack(properties[i].Argument.Type, properties[i].Argument.Value));
                ins.Add(Instruction.Create(OpCodes.Callvirt, attrTypeDef.RecursionImportPropertySet(ModuleDefinition, properties[i].Name)));
                ins.Add(Instruction.Create(OpCodes.Nop));
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

        private void InitMethodContextField(RouMethod rouMethod, FieldDefinition contextFieldDef, VariableDefinition stateMachineVariable, Instruction taskBuilderPreviousIns)
        {
            var instructions = rouMethod.MethodDef.Body.Instructions;
            instructions.InsertBefore(taskBuilderPreviousIns, Instruction.Create(OpCodes.Ldloca, stateMachineVariable));
            var contextInitIns = new List<Instruction>();
            InitMethodContext(rouMethod.MethodDef, contextInitIns);
            instructions.InsertBefore(taskBuilderPreviousIns, contextInitIns);
            instructions.InsertBefore(taskBuilderPreviousIns, Instruction.Create(OpCodes.Stfld, contextFieldDef));
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

        private void ExecuteMoMethod(string methodName, MethodDefinition methodDef, int mosCount, VariableDefinition stateMachineVariable, FieldDefinition mosField, FieldDefinition contextField, Instruction nextIns)
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
                instructions.InsertBefore(nextIns, Instruction.Create(OpCodes.Ldloca, stateMachineVariable));
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
                instructions.InsertBefore(nextIns, Instruction.Create(OpCodes.Ldloca, stateMachineVariable));
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
                var methodRef = mo.VariableType.Resolve().RecursionImportMethod(ModuleDefinition, methodName, md => md.Parameters.Count == 1 && md.Parameters.First().ParameterType.Is(Constants.TYPE_MethodContext));
                instructions.Add(Instruction.Create(OpCodes.Callvirt, methodRef));
            }
        }
    }
}
