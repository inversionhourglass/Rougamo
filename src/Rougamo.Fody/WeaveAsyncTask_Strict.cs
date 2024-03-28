using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Rougamo.Fody.Enhances;
using Rougamo.Fody.Enhances.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void StrictAsyncTaskMethodWeave(RouMethod rouMethod, TypeDefinition stateMachineTypeDef)
        {
            var actualStateMachineTypeDef = StrictAsyncStateMachineClone(stateMachineTypeDef);
            if (actualStateMachineTypeDef == null) return;

            var actualMethodDef = StrictAsyncSetupMethodClone(rouMethod.MethodDef, stateMachineTypeDef, actualStateMachineTypeDef);
            rouMethod.MethodDef.DeclaringType.Methods.Add(actualMethodDef);

            var moveNextDef = stateMachineTypeDef.Methods.Single(m => m.Name == Constants.METHOD_MoveNext);
#if DEBUG
            //if (rouMethod.MethodDef.FullName.Contains("Strict_"))
#endif
            {
                moveNextDef.Clear();
                var bag = StrictAsyncProxyCall(rouMethod, stateMachineTypeDef, moveNextDef, actualMethodDef);
                StrictAsyncWeave(rouMethod, bag, moveNextDef);

                moveNextDef.Body.OptimizePlus(EmptyInstructions);
            }
        }

        private StrictAsyncBag StrictAsyncProxyCall(RouMethod rouMethod, TypeDefinition proxyStateMachineTypeDef, MethodDefinition proxyMoveNextDef, MethodDefinition actualMethodDef)
        {
            var genericMap = proxyStateMachineTypeDef.GenericParameters.ToDictionary(x => x.Name, x => x);

            var proxyStateMachineTypeRef = proxyStateMachineTypeDef.MakeReference();
            var returnTypeRef = actualMethodDef.ReturnType;
            var getAwaiterMethodDef = returnTypeRef.Resolve().Methods.Single(x => x.Name == Constants.METHOD_GetAwaiter);
            var awaiterTypeRef = getAwaiterMethodDef.ReturnType;
            var awaiterTypeDef = awaiterTypeRef.Resolve();
            var isCompletedMethodDef = awaiterTypeDef.Methods.Single(x => x.Name == Constants.Getter(Constants.PROP_IsCompleted));
            var getResultMethodDef = awaiterTypeDef.Methods.Single(x => x.Name == Constants.METHOD_GetResult);
            if (returnTypeRef is GenericInstanceType)
            {
                returnTypeRef = returnTypeRef.ReplaceGenericArgs(genericMap);
                var git = new GenericInstanceType(awaiterTypeRef.GetElementType().ImportInto(ModuleDefinition));
                git.GenericArguments.Add(((GenericInstanceType)returnTypeRef).GenericArguments);
                awaiterTypeRef = git;
            }
            else
            {
                returnTypeRef = returnTypeRef.ImportInto(ModuleDefinition);
                awaiterTypeRef = awaiterTypeRef.ImportInto(ModuleDefinition);
            }
            var getAwaiterMethodRef = getAwaiterMethodDef.WithGenericDeclaringType(returnTypeRef);
            var isCompletedMethodRef = isCompletedMethodDef.WithGenericDeclaringType(awaiterTypeRef);
            var getResultMethodRef = getResultMethodDef.WithGenericDeclaringType(awaiterTypeRef);
            var declaringTypeRef = actualMethodDef.DeclaringType.MakeReference().ReplaceGenericArgs(genericMap);
            var actualMethodRef = actualMethodDef.WithGenericDeclaringType(declaringTypeRef);
            if (proxyStateMachineTypeDef.HasGenericParameters)
            {
                var parentGenericNames = proxyStateMachineTypeDef.DeclaringType.GenericParameters.Select(x => x.Name).ToArray();
                var generics = proxyStateMachineTypeDef.GenericParameters.Where(x => !parentGenericNames.Contains(x.Name)).ToArray();
                actualMethodRef = actualMethodRef.WithGenerics(generics);
            }

            var fields = AsyncResolveFields(rouMethod, proxyStateMachineTypeDef);
            StrictSetAbsentFields(rouMethod, proxyStateMachineTypeDef, fields);

            var builderTypeRef = fields.Builder.FieldType;
            var builderTypeDef = builderTypeRef.Resolve();
            var awaitUnsafeOnCompletedMethodDef = builderTypeDef.Methods.Single(x => x.Name == Constants.METHOD_AwaitUnsafeOnCompleted && x.IsPublic);
            var awaitUnsafeOnCompletedMethodRef = awaitUnsafeOnCompletedMethodDef.WithGenericDeclaringType(builderTypeRef).WithGenerics(awaiterTypeRef, proxyStateMachineTypeRef);
            var setExceptionMethodRef = builderTypeDef.Methods.Single(x => x.Name == Constants.METHOD_SetException && x.IsPublic).WithGenericDeclaringType(builderTypeRef);
            var setResultMethodRef = builderTypeDef.Methods.Single(x => x.Name == Constants.METHOD_SetResult && x.IsPublic).WithGenericDeclaringType(builderTypeRef);

            StrictAsyncFieldCleanup(proxyStateMachineTypeDef, fields);
            var fAwaiter = new FieldDefinition(Constants.FIELD_Awaiter, FieldAttributes.Private, awaiterTypeRef);
            proxyStateMachineTypeDef.Fields.Add(fAwaiter);
            fields.Awaiter = fAwaiter;

            var vState = proxyMoveNextDef.Body.CreateVariable(_typeIntRef);
            var vAwaiter = proxyMoveNextDef.Body.CreateVariable(awaiterTypeRef);
            var vException = proxyMoveNextDef.Body.CreateVariable(_typeExceptionRef);
            VariableDefinition? vThis = null;
            VariableDefinition? vTargetReturn = null;
            VariableDefinition? vResult = null;
            if (!proxyStateMachineTypeDef.IsValueType)
            {
                vThis = proxyMoveNextDef.Body.CreateVariable(proxyStateMachineTypeRef);
            }
            if (returnTypeRef.IsValueType)
            {
                vTargetReturn = proxyMoveNextDef.Body.CreateVariable(returnTypeRef);
            }
            if (awaiterTypeRef is GenericInstanceType gitt)
            {
                vResult = proxyMoveNextDef.Body.CreateVariable(gitt.GenericArguments.Single());
            }

            var builderIsValueType = fields.Builder.FieldType.IsValueType;
            var opBuilderLdfld = builderIsValueType ? OpCodes.Ldflda : OpCodes.Ldfld;
            var opBuilderCall = builderIsValueType ? OpCodes.Call : OpCodes.Callvirt;
            var awaiterIsValueType = awaiterTypeRef.IsValueType;
            var opAwaiterLdloc = awaiterIsValueType ? OpCodes.Ldloca : OpCodes.Ldloc;
            var opAwaiterCall = awaiterIsValueType ? OpCodes.Call : OpCodes.Callvirt;
            var opGetAwaiterCall = returnTypeRef.IsValueType ? OpCodes.Call : OpCodes.Callvirt;

            var instructions = proxyMoveNextDef.Body.Instructions;

            var nopIfStateEqZeroStart = Create(OpCodes.Nop);
            var nopIfStateEqZeroEnd = Create(OpCodes.Nop);
            var nopTryStart = Create(OpCodes.Nop);
            var nopCatchStart = Create(OpCodes.Nop);
            var nopCatchEnd = Create(OpCodes.Nop);
            var ret = Create(OpCodes.Ret);

            // var state = this._state;
            instructions.Add(Create(OpCodes.Ldarg_0));
            instructions.Add(Create(OpCodes.Ldfld, fields.State));
            instructions.Add(Create(OpCodes.Stloc, vState));
            // -try
            {
                /**
                 * if (state != 0)
                 * {
                 *     var mo1 = new Mo1Attribute();
                 *     var context = new MethodContext(...);
                 *     mo1.OnEntry(context);
                 *     if (context.ReturnValueReplaced)
                 *     {
                 *         var result = context.ReturnValue;
                 *         mo1.OnExit(context);
                 *         goto nopCatchEnd;
                 *     }
                 *     if (context.RewriteArguments)
                 *     {
                 *         arg1 = (int)context.Argument[0];
                 *     }
                 * }
                 * 
                 * while (true)
                 * {
                 *     try
                 *     {
                 */
                instructions.Add(nopTryStart.Set(OpCodes.Ldloc, vState));
                instructions.Add(Create(OpCodes.Brfalse, nopIfStateEqZeroStart));
                // -if (state != 0)
                {
                    // var awaiter = ActualMethod(x, y, z).GetAwaiter();
                    if (fields.DeclaringThis != null)
                    {
                        var ldfld = fields.DeclaringThis.FieldType.IsValueType ? OpCodes.Ldflda : OpCodes.Ldfld;
                        instructions.Add(Create(OpCodes.Ldarg_0));
                        instructions.Add(Create(ldfld, fields.DeclaringThis));
                    }
                    foreach (var parameter in fields.Parameters)
                    {
                        instructions.Add(Create(OpCodes.Ldarg_0));
                        instructions.Add(Create(OpCodes.Ldfld, parameter));
                    }
                    instructions.Add(Create(OpCodes.Call, actualMethodRef));
                    if (vTargetReturn != null)
                    {// value type result has to use address to call
                        instructions.Add(Create(OpCodes.Stloc, vTargetReturn));
                        instructions.Add(Create(OpCodes.Ldloca, vTargetReturn));
                    }
                    instructions.Add(Create(opGetAwaiterCall, getAwaiterMethodRef));
                    instructions.Add(Create(OpCodes.Stloc, vAwaiter));

                    instructions.Add(Create(opAwaiterLdloc, vAwaiter));
                    instructions.Add(Create(opAwaiterCall, isCompletedMethodRef));
                    instructions.Add(Create(OpCodes.Brtrue, nopIfStateEqZeroEnd));
                    // -if (!awaiter.IsCompleted)
                    {
                        // state = (this._state = 0);
                        instructions.Add(Create(OpCodes.Ldarg_0));
                        instructions.Add(Create(OpCodes.Ldc_I4_0));
                        instructions.Add(Create(OpCodes.Dup));
                        instructions.Add(Create(OpCodes.Stloc, vState));
                        instructions.Add(Create(OpCodes.Stfld, fields.State));

                        // this._awaiter = awaiter;
                        instructions.Add(Create(OpCodes.Ldarg_0));
                        instructions.Add(Create(OpCodes.Ldloc, vAwaiter));
                        instructions.Add(Create(OpCodes.Stfld, fields.Awaiter));

                        if (vThis != null)
                        {// @this = this;
                            instructions.Add(Create(OpCodes.Ldarg_0));
                            instructions.Add(Create(OpCodes.Stloc, vThis));
                        }

                        // this._builder.AwaitUnsafeOnCompleted(ref awaiter, ref @this);
                        instructions.Add(Create(OpCodes.Ldarg_0));
                        instructions.Add(Create(opBuilderLdfld, fields.Builder));
                        instructions.Add(Create(OpCodes.Ldloca, vAwaiter));
                        if (vThis != null)
                        {
                            instructions.Add(Create(OpCodes.Ldloca, vThis));
                        }
                        else
                        {
                            instructions.Add(Create(OpCodes.Ldarg_0));
                        }
                        instructions.Add(Create(opBuilderCall, awaitUnsafeOnCompletedMethodRef));

                        // return;
                        instructions.Add(Create(OpCodes.Leave, ret));
                    }
                }
                // -else (state == 0)
                {
                    // awaiter = this._awaiter;
                    instructions.Add(nopIfStateEqZeroStart.Set(OpCodes.Ldarg_0));
                    instructions.Add(Create(OpCodes.Ldfld, fields.Awaiter));
                    instructions.Add(Create(OpCodes.Stloc, vAwaiter));

                    // this._awaiter = default;
                    instructions.Add(Create(OpCodes.Ldarg_0));
                    if (awaiterTypeRef.IsValueType)
                    {
                        instructions.Add(Create(OpCodes.Ldflda, fields.Awaiter));
                        instructions.Add(Create(OpCodes.Initobj, awaiterTypeRef));
                    }
                    else
                    {
                        instructions.Add(Create(OpCodes.Ldnull));
                        instructions.Add(Create(OpCodes.Stfld));
                    }

                    // state = (this._state = -1);
                    instructions.Add(Create(OpCodes.Ldarg_0));
                    instructions.Add(Create(OpCodes.Ldc_I4_M1));
                    instructions.Add(Create(OpCodes.Dup));
                    instructions.Add(Create(OpCodes.Stloc, vState));
                    instructions.Add(Create(OpCodes.Stfld, fields.State));
                }
                // var result = awaiter.GetResult(); <--> awaiter.GetResult();
                instructions.Add(nopIfStateEqZeroEnd.Set(opAwaiterLdloc, vAwaiter));
                instructions.Add(Create(opAwaiterCall, getResultMethodRef));
                if (vResult != null)
                {
                    instructions.Add(Create(OpCodes.Stloc, vResult));
                }

                /**
                 *     context.ResultValue = result;
                 *     context.Arguments[0] = arg1;
                 *     mo1.OnSuccess(context);
                 *     
                 *     if (context.RetryCount > 0) continue;
                 *     
                 *     if (context.ReturnValueReplaced)
                 *     {
                 *         result = (int) context.ReturnValue;
                 *     }
                 *     
                 *     mo1.OnExit(context);
                 *     
                 *     break;
                 */

                instructions.Add(Create(OpCodes.Leave, nopCatchEnd));

                /**
                 *     }
                 *     catch (Exception e)
                 *     {
                 *         context.Exception = e;
                 *         context.Arguments[0] = arg1;
                 *         mo1.OnException(context);
                 *         
                 *         if (context.RetryCount > 0) continue;
                 *         
                 *         if (context.ExceptionHandled)
                 *         {
                 *             result = (int) context.ReturnValue;
                 *             mo1.OnExit(context);
                 *             break;
                 *         }
                 *         
                 *         throw;
                 *     }
                 * } // while true end
                 */
            }
            // catch (Exception ex)
            {
                instructions.Add(nopCatchStart.Set(OpCodes.Stloc, vException));

                // this._state = -2;
                instructions.Add(Create(OpCodes.Ldarg_0));
                instructions.Add(Create(OpCodes.Ldc_I4, -2));
                instructions.Add(Create(OpCodes.Stfld, fields.State));

                StrictAsyncDefaultMoContext(instructions, fields);

                // this._builder.SetException(ex);
                instructions.Add(Create(OpCodes.Ldarg_0));
                instructions.Add(Create(opBuilderLdfld, fields.Builder));
                instructions.Add(Create(OpCodes.Ldloc, vException));
                instructions.Add(Create(opBuilderCall, setExceptionMethodRef));

                // return;
                instructions.Add(Create(OpCodes.Leave, ret));
            }
            // this._state = -2;
            instructions.Add(nopCatchEnd.Set(OpCodes.Ldarg_0));
            instructions.Add(Create(OpCodes.Ldc_I4, -2));
            instructions.Add(Create(OpCodes.Stfld, fields.State));

            StrictAsyncDefaultMoContext(instructions, fields);

            // this._builder.SetResult(result); <--> this._builder.SetResult();
            instructions.Add(Create(OpCodes.Ldarg_0));
            instructions.Add(Create(opBuilderLdfld, fields.Builder));
            if (vResult != null)
            {
                instructions.Add(Create(OpCodes.Ldloc, vResult));
            }
            instructions.Add(Create(opBuilderCall, setResultMethodRef));

            // return;
            instructions.Add(ret);

            proxyMoveNextDef.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
            {
                TryStart = nopTryStart,
                TryEnd = nopCatchStart,
                HandlerStart = nopCatchStart,
                HandlerEnd = nopCatchEnd,
                CatchType = _typeExceptionRef
            });

            return new(fields, new(vState, vThis, vResult));
        }

        private void StrictAsyncWeave(RouMethod rouMethod, StrictAsyncBag bag, MethodDefinition proxyMoveNextDef)
        {
            var outerHandler = proxyMoveNextDef.Body.ExceptionHandlers.Single();
            var tryStart = outerHandler.TryStart;
            var tryLastLeave = outerHandler.TryEnd.Previous;
            var tryLastLeaveCode = tryLastLeave.OpCode.Code;
            var outerCatchStart = outerHandler.HandlerStart;
            var outerCatchEnd = outerHandler.HandlerEnd;
            if (tryLastLeaveCode != Code.Leave && tryLastLeaveCode != Code.Leave_S) throw new RougamoException($"{proxyMoveNextDef.FullName} has changed, the last instruction in try scope is {tryLastLeaveCode} not leave.");

            var instructions = proxyMoveNextDef.Body.Instructions;

            var vInnerException = proxyMoveNextDef.Body.CreateVariable(_typeExceptionRef);

            var oldTryStart = instructions.InsertAfter(tryStart, tryStart.Clone());
            var innerCatchStart = Create(OpCodes.Stloc, vInnerException);

            var nopRetryLoopStart = Create(OpCodes.Nop);
            var nopOnEntryEnd = Create(OpCodes.Nop);
            var nopEntryReplaceCheckEnd = Create(OpCodes.Nop);
            var nopOnSuccessEnd = Create(OpCodes.Nop);
            var nopSuccessRetryCheckEnd = Create(OpCodes.Nop);
            var nopSuccessReplacedEnd = Create(OpCodes.Nop);
            var nopOnExceptionEnd = Create(OpCodes.Nop);
            var nopExceptionRetryCheckEnd = Create(OpCodes.Nop);
            var nopExceptionRethrowEnd = Create(OpCodes.Nop);

            /**
             * if (state != 0)
             * {
             *     var mo1 = new Mo1Attribute();
             *     var context = new MethodContext(...);
             *     mo1.OnEntry(context);
             *     if (context.ReturnValueReplaced)
             *     {
             *         var result = context.ReturnValue;
             *         mo1.OnExit(context);
             *         goto nopCatchEnd;
             *     }
             *     if (context.RewriteArguments)
             *     {
             *         arg1 = (int)context.Argument[0];
             *     }
             * }
             * 
             * while (true)
             * {
             *     try
             *     {
             */
            tryStart.Set(OpCodes.Ldloc, bag.Variables.State);
            instructions.InsertBefore(oldTryStart, Create(OpCodes.Brfalse, nopRetryLoopStart));
            // -if (state != 0)
            {
                instructions.InsertBefore(oldTryStart, StrictAsyncInitMos(proxyMoveNextDef, rouMethod.Mos, bag.Fields));
                instructions.InsertBefore(oldTryStart, StrictAsyncInitMethodContext(rouMethod, proxyMoveNextDef, bag));
                instructions.InsertBefore(oldTryStart, StateMachineOnEntry(rouMethod, proxyMoveNextDef, nopOnEntryEnd, bag.Fields));
                instructions.InsertBefore(oldTryStart, nopOnEntryEnd);
                instructions.InsertBefore(oldTryStart, StrictAsyncIfOnEntryReplacedReturn(rouMethod, proxyMoveNextDef, nopEntryReplaceCheckEnd, outerCatchEnd, bag));
                instructions.InsertBefore(oldTryStart, nopEntryReplaceCheckEnd);
                instructions.InsertBefore(oldTryStart, StateMachineRewriteArguments(rouMethod, nopRetryLoopStart, bag.Fields));
            }
            instructions.InsertBefore(oldTryStart, nopRetryLoopStart);

            /**
             *     context.ResultValue = result;
             *     context.Arguments[0] = arg1;
             *     mo1.OnSuccess(context);
             *     
             *     if (context.RetryCount > 0) continue;
             *     
             *     if (context.ReturnValueReplaced)
             *     {
             *         result = (int) context.ReturnValue;
             *     }
             *     
             *     mo1.OnExit(context);
             *     
             *     break;
             */
            instructions.InsertBefore(tryLastLeave, StrictAsyncSaveReturnValue(rouMethod, bag));
            instructions.InsertBefore(tryLastLeave, StateMachineOnSuccess(rouMethod, proxyMoveNextDef, nopOnSuccessEnd, bag.Fields));
            instructions.InsertBefore(tryLastLeave, nopOnSuccessEnd);
            instructions.InsertBefore(tryLastLeave, StrictAsyncIfRetry(rouMethod, nopRetryLoopStart, nopSuccessRetryCheckEnd, bag));
            instructions.InsertBefore(tryLastLeave, nopSuccessRetryCheckEnd);
            instructions.InsertBefore(tryLastLeave, StrictAsyncIfSuccessReplacedReturn(rouMethod, nopSuccessReplacedEnd, bag));
            instructions.InsertBefore(tryLastLeave, nopSuccessReplacedEnd);
            instructions.InsertBefore(tryLastLeave, StateMachineOnExit(rouMethod, proxyMoveNextDef, tryLastLeave, bag.Fields));

            /**
             *     }
             *     catch (Exception e)
             *     {
             *         context.Exception = e;
             *         context.Arguments[0] = arg1;
             *         mo1.OnException(context);
             *         
             *         if (context.RetryCount > 0) continue;
             *         
             *         if (!context.ExceptionHandled) throw;
             *         
             *         result = (int) context.ReturnValue;
             *         mo1.OnExit(context);
             *         break;
             *     }
             * } // while true end
             */
            instructions.InsertBefore(outerCatchStart, innerCatchStart);
            instructions.InsertBefore(outerCatchStart, StrictStateMachineSaveException(rouMethod, bag.Fields, vInnerException));
            instructions.InsertBefore(outerCatchStart, StateMachineOnException(rouMethod, proxyMoveNextDef, nopOnExceptionEnd, bag.Fields));
            instructions.InsertBefore(outerCatchStart, nopOnExceptionEnd);
            instructions.InsertBefore(outerCatchStart, StrictAsyncIfRetry(rouMethod, nopRetryLoopStart, nopExceptionRetryCheckEnd, bag));
            instructions.InsertBefore(outerCatchStart, nopExceptionRetryCheckEnd);
            instructions.InsertBefore(outerCatchStart, StrictAsyncIfExceptionNotHandled(rouMethod, nopExceptionRethrowEnd, bag.Fields));
            instructions.InsertBefore(outerCatchStart, nopExceptionRethrowEnd);
            instructions.InsertBefore(outerCatchStart, StrictAsyncExceptionHandled(rouMethod, proxyMoveNextDef, outerCatchEnd, bag));

            outerHandler.TryStart = tryStart;
            outerHandler.TryEnd = outerCatchStart;
            outerHandler.HandlerStart = outerCatchStart;
            outerHandler.HandlerEnd = outerCatchEnd;

            proxyMoveNextDef.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
            {
                TryStart = oldTryStart,
                TryEnd = innerCatchStart,
                HandlerStart = innerCatchStart,
                HandlerEnd = outerCatchStart,
                CatchType = _typeExceptionRef
            });
        }

        private TypeDefinition? StrictAsyncStateMachineClone(TypeDefinition stateMachineTypeDef)
        {
            var typeName = $"$Rougamo_{stateMachineTypeDef.Name}";
            if (stateMachineTypeDef.DeclaringType.NestedTypes.Any(x => x.Name == typeName)) return null;

            var actualTypeDef = stateMachineTypeDef.Clone(typeName);
            actualTypeDef.DeclaringType.NestedTypes.Add(actualTypeDef);

            return actualTypeDef;
        }

        private MethodDefinition StrictAsyncSetupMethodClone(MethodDefinition methodDef, TypeDefinition stateMachineTypeDef, TypeDefinition clonedStateMachineTypeDef)
        {
            var clonedMethodDef = methodDef.Clone($"$Rougamo_{methodDef.Name}");

            StrictAsyncAsyncStateMachineAttributeClone(clonedMethodDef, clonedStateMachineTypeDef);

            var genericMap = methodDef.DeclaringType.GenericParameters.ToDictionary(x => x.Name, x => x);
            genericMap.AddRange(clonedMethodDef.GenericParameters.ToDictionary(x => x.Name, x => x));
            var stateMachineVariableTypeRef = clonedStateMachineTypeDef.MakeReference().ReplaceGenericArgs(genericMap);
            var vStateMachine = clonedMethodDef.Body.Variables.Single(x => x.VariableType.Resolve() == stateMachineTypeDef);
            var index = clonedMethodDef.Body.Variables.IndexOf(vStateMachine);
            clonedMethodDef.Body.Variables.Remove(vStateMachine);
            var vClonedStateMachine = new VariableDefinition(stateMachineVariableTypeRef);
            clonedMethodDef.Body.Variables.Insert(index, vClonedStateMachine);

            var fieldMap = new Dictionary<FieldDefinition, FieldReference>();
            foreach (var fieldDef in stateMachineTypeDef.Fields)
            {
                fieldMap[fieldDef] = new FieldReference(fieldDef.Name, fieldDef.FieldType, stateMachineVariableTypeRef);
            }

            foreach (var instruction in clonedMethodDef.Body.Instructions)
            {
                if (instruction.Operand == null) continue;

                if (instruction.Operand is MethodReference methodRef)
                {
                    if (methodRef.Resolve().IsConstructor && methodRef.DeclaringType.Resolve() == stateMachineTypeDef)
                    {
                        var stateMachineCtorDef = clonedStateMachineTypeDef.Methods.Single(x => x.IsConstructor && !x.IsStatic);
                        var stateMachineCtorRef = stateMachineCtorDef.WithGenericDeclaringType(stateMachineVariableTypeRef);
                        instruction.Operand = stateMachineCtorRef;
                    }
                    else if (methodRef is GenericInstanceMethod gim)
                    {
                        var mr = new GenericInstanceMethod(gim.ElementMethod);
                        foreach (var generic in gim.GenericArguments)
                        {
                            if (generic == stateMachineTypeDef)
                            {
                                mr.GenericArguments.Add(clonedStateMachineTypeDef);
                            }
                            else if (generic is GenericInstanceType git && git.ElementType == stateMachineTypeDef)
                            {
                                mr.GenericArguments.Add(clonedStateMachineTypeDef.ReplaceGenericArgs(genericMap));
                            }
                            else
                            {
                                mr.GenericArguments.Add(generic.ReplaceGenericArgs(genericMap));
                            }
                        }
                        instruction.Operand = mr;
                    }
                }
                else if (instruction.Operand is FieldReference fr && fieldMap.TryGetValue(fr.Resolve(), out var fieldRef))
                {
                    instruction.Operand = fieldRef;
                }
                else if (instruction.Operand == vStateMachine)
                {
                    instruction.Operand = vClonedStateMachine;
                }
            }

            return clonedMethodDef;
        }

        private void StrictAsyncAsyncStateMachineAttributeClone(MethodDefinition clonedMethodDef, TypeDefinition clonedStateMachineTypeDef)
        {
            var asyncStateMachineAttribute = clonedMethodDef.CustomAttributes.Single(x => x.Is(Constants.TYPE_AsyncStateMachineAttribute));
            clonedMethodDef.CustomAttributes.Remove(asyncStateMachineAttribute);

            asyncStateMachineAttribute = new CustomAttribute(_methodAsyncStateMachineAttributeCtorRef);
            asyncStateMachineAttribute.ConstructorArguments.Add(new CustomAttributeArgument(_typeSystemRef, clonedStateMachineTypeDef));
            clonedMethodDef.CustomAttributes.Add(asyncStateMachineAttribute);
        }

        private void StrictAsyncFieldCleanup(TypeDefinition stateMachineTypeDef, AsyncFields fields)
        {
            var fieldNames = new Dictionary<string, object?>();
            foreach (var prop in fields.GetType().GetProperties())
            {
                var value = prop.GetValue(fields);
                if (value == null) continue;
                if (value is FieldReference fr) fieldNames[fr.Name] = null;
                if (value is FieldReference[] frs)
                {
                    foreach (var f in frs)
                    {
                        fieldNames[f.Name] = null;
                    }
                }
            }

            foreach (var field in stateMachineTypeDef.Fields.ToArray())
            {
                if (fieldNames.ContainsKey(field.Name)) continue;

                stateMachineTypeDef.Fields.Remove(field);
            }
        }

        private void StrictSetAbsentFields(RouMethod rouMethod, TypeDefinition stateMachineTypeDef, AsyncFields fields)
        {
            var instructions = rouMethod.MethodDef.Body.Instructions;
            var setState = instructions.Single(x => x.OpCode.Code == Code.Ldc_I4_M1 && x.Next.OpCode.Code == Code.Stfld && x.Next.Operand is FieldReference fr && fr.Resolve() == fields.State.Resolve());
            var vStateMachine = rouMethod.MethodDef.Body.Variables.Single(x => x.VariableType.Resolve() == stateMachineTypeDef);
            var genericMap = stateMachineTypeDef.GenericParameters.ToDictionary(x => x.Name, x => x);

            StrictAddAbsentField(stateMachineTypeDef, StrictSetAbsentFieldThis(rouMethod, fields, vStateMachine, setState, genericMap));

            StrictAddAbsentField(stateMachineTypeDef, StrictSetAbsentFieldParameters(rouMethod, fields, vStateMachine, setState, genericMap));
        }

        private IEnumerable<FieldDefinition> StrictSetAbsentFieldThis(RouMethod rouMethod, AsyncFields fields, VariableDefinition vStateMachine, Instruction setState, Dictionary<string, GenericParameter> genericMap)
        {
            if (rouMethod.MethodDef.IsStatic || fields.DeclaringThis != null) yield break;

            var thisTypeRef = vStateMachine.VariableType.DeclaringType.ReplaceGenericArgs(genericMap);
            var thisFieldDef = new FieldDefinition(Constants.FIELD_This, FieldAttributes.Public, thisTypeRef);
            var thisFieldRef = new FieldReference(thisFieldDef.Name, thisFieldDef.FieldType, vStateMachine.VariableType);

            fields.DeclaringThis = thisFieldDef;

            rouMethod.MethodDef.Body.Instructions.InsertBefore(setState, [
                vStateMachine.LdlocOrA(),
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Stfld, thisFieldRef)
            ]);

            yield return thisFieldDef;
        }

        private IEnumerable<FieldDefinition> StrictSetAbsentFieldParameters(RouMethod rouMethod, AsyncFields fields, VariableDefinition vStateMachine, Instruction setState, Dictionary<string, GenericParameter> genericMap)
        {
            if (fields.Parameters.All(x => x != null)) yield break;

            var instructions = rouMethod.MethodDef.Body.Instructions;
            var parameters = rouMethod.MethodDef.Parameters;

            for (var i = 0; i < fields.Parameters.Length; i++)
            {
                var parameterFieldRef = fields.Parameters[i];
                if (parameterFieldRef != null) continue;

                var parameter = parameters[i];
                var fieldTypeRef = parameter.ParameterType.ReplaceGenericArgs(genericMap);
                var parameterFieldDef = new FieldDefinition(parameter.Name, FieldAttributes.Public, fieldTypeRef);
                parameterFieldRef = new FieldReference(parameterFieldDef.Name, parameterFieldDef.FieldType, vStateMachine.VariableType);

                fields.SetParameter(i, parameterFieldDef);

                instructions.InsertBefore(setState, [
                    vStateMachine.LdlocOrA(),
                    Create(OpCodes.Ldarg, parameter),
                    Create(OpCodes.Stfld, parameterFieldRef)
                ]);

                yield return parameterFieldDef;
            }
        }

        private void StrictAddAbsentField(TypeDefinition typeDef, IEnumerable<FieldDefinition> fieldDefs)
        {
            foreach (var fieldDef in fieldDefs)
            {
                typeDef.Fields.Add(fieldDef);
            }
        }

        private IList<Instruction> StrictAsyncInitMos(MethodDefinition moveNextMethodDef, Mo[] mos, AsyncFields fields)
        {
            if (fields.MoArray != null)
            {
                var instructions = new List<Instruction> { Create(OpCodes.Ldarg_0) };
                instructions.AddRange(InitMoArray(moveNextMethodDef, mos));
                instructions.Add(Create(OpCodes.Stfld, fields.MoArray));

                return instructions;
            }

            return StrictAsyncInitMoFields(moveNextMethodDef, mos, fields.Mos);
        }

        private IList<Instruction> StrictAsyncInitMoFields(MethodDefinition methodDef, Mo[] mos, FieldReference[] moFields)
        {
            var instructions = new List<Instruction>();

            var i = 0;
            foreach (var mo in mos)
            {
                instructions.Add(Create(OpCodes.Ldarg_0));
                instructions.AddRange(InitMo(methodDef, mo, false));
                instructions.Add(Create(OpCodes.Stfld, moFields[i]));

                i++;
            }

            return instructions;
        }

        private IList<Instruction> StrictAsyncInitMethodContext(RouMethod rouMethod, MethodDefinition moveNextMethodDef, StrictAsyncBag bag)
        {
            var instructions = new List<Instruction>();
            VariableDefinition? moArray = null;
            if ((rouMethod.MethodContextOmits & Omit.Mos) == 0 && bag.Fields.MoArray == null)
            {
                moArray = moveNextMethodDef.Body.CreateVariable(_typeIMoArrayRef);
                instructions.AddRange(CreateTempMoArray(null, bag.Fields.Mos, rouMethod.Mos));
                instructions.Add(Create(OpCodes.Stloc, moArray));
            }

            instructions.Add(Create(OpCodes.Ldarg_0));
            instructions.AddRange(StrictStateMachineInitMethodContext(rouMethod.MethodDef, moArray, bag, rouMethod.MethodContextOmits));
            instructions.Add(Create(OpCodes.Stfld, bag.Fields.MethodContext));

            return instructions;
        }

        private List<Instruction> StrictStateMachineInitMethodContext(MethodDefinition methodDef, VariableDefinition? moArrayVariable, StrictAsyncBag bag, Omit omit)
        {
            var instructions = new List<Instruction>();

            if (bag.Fields.DeclaringThis != null)
            {
                instructions.Add(Create(OpCodes.Ldarg_0));
                instructions.Add(Create(OpCodes.Ldfld, bag.Fields.DeclaringThis));
                if (bag.Fields.DeclaringThis.FieldType.IsValueType)
                {
                    instructions.Add(Create(OpCodes.Box, bag.Fields.DeclaringThis.FieldType));
                }
            }
            else
            {
                instructions.Add(Create(OpCodes.Ldnull));
            }
            instructions.Add(Create(OpCodes.Ldtoken, methodDef.DeclaringType));
            instructions.Add(Create(OpCodes.Call, _methodGetTypeFromHandleRef));
            instructions.AddRange(LoadMethodBaseOnStack(methodDef));
            if ((omit & Omit.Mos) != 0)
            {
                instructions.Add(Create(OpCodes.Ldnull));
            }
            else if (bag.Fields.MoArray == null)
            {
                instructions.Add(Create(OpCodes.Ldloc, moArrayVariable));
            }
            else
            {
                instructions.Add(Create(OpCodes.Ldarg_0));
                instructions.Add(Create(OpCodes.Ldfld, bag.Fields.MoArray));
            }
            if ((omit & Omit.Arguments) != 0)
            {
                instructions.Add(Create(OpCodes.Ldnull));
            }
            else
            {
                instructions.AddRange(StrictAsyncLoadArguments(bag.Fields.Parameters));
            }
            instructions.Add(Create(OpCodes.Newobj, _methodMethodContext3CtorRef));

            return instructions;
        }

        private IList<Instruction>? StrictAsyncIfOnEntryReplacedReturn(RouMethod rouMethod, MethodDefinition moveNextMethodDef, Instruction endAnchor, Instruction tailAnchor, StrictAsyncBag bag)
        {
            if (!Feature.EntryReplace.IsMatch(rouMethod.Features) || (rouMethod.MethodContextOmits & Omit.ReturnValue) != 0) return null;

            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, bag.Fields.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueReplacedRef),
                Create(OpCodes.Brfalse_S, endAnchor),
            };
            if (bag.Variables.Result != null)
            {
                instructions.AddRange(AssignResultFromContext(bag));
            }
            var onExitEndAnchor = Create(OpCodes.Leave, tailAnchor);
            instructions.AddRange(StateMachineOnExit(rouMethod, moveNextMethodDef, onExitEndAnchor, bag.Fields));
            instructions.Add(onExitEndAnchor);

            return instructions;
        }

        private IList<Instruction>? StrictAsyncSaveReturnValue(RouMethod rouMethod, StrictAsyncBag bag)
        {
            if (bag.Variables.Result == null || (rouMethod.Features & (int)(Feature.OnSuccess | Feature.OnExit)) == 0 || (rouMethod.MethodContextOmits & Omit.ReturnValue) != 0) return null;

            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, bag.Fields.MethodContext),
                Create(OpCodes.Ldloc, bag.Variables.Result)
            };
            if (bag.Variables.Result.VariableType.NeedBox())
            {
                instructions.Add(Create(OpCodes.Box, bag.Variables.Result.VariableType));
            }
            instructions.Add(Create(OpCodes.Callvirt, _methodMethodContextSetReturnValueRef));

            return instructions;
        }

        private IList<Instruction>? StrictAsyncIfRetry(RouMethod rouMethod, Instruction loopStartAnchor, Instruction endAnchor, StrictAsyncBag bag)
        {
            if (!Feature.SuccessRetry.IsMatch(rouMethod.Features)) return null;

            return [
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, bag.Fields.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetRetryCountRef),
                Create(OpCodes.Ldc_I4_0),
                Create(OpCodes.Ble, endAnchor),
                Create(OpCodes.Leave_S, loopStartAnchor)
            ];
        }

        private IList<Instruction>? StrictAsyncIfSuccessReplacedReturn(RouMethod rouMethod, Instruction endAnchor, StrictAsyncBag bag)
        {
            if (bag.Variables.Result == null || !Feature.SuccessReplace.IsMatch(rouMethod.Features) || (rouMethod.MethodContextOmits & Omit.ReturnValue) != 0) return EmptyInstructions;

            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, bag.Fields.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueReplacedRef),
                Create(OpCodes.Brfalse_S, endAnchor),
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, bag.Fields.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueRef),
            };
            if (bag.Variables.Result.VariableType.FullName != typeof(object).FullName)
            {
                var castOp = bag.Variables.Result.VariableType.NeedBox() ? OpCodes.Unbox_Any : OpCodes.Castclass;
                instructions.Add(Create(castOp, bag.Variables.Result.VariableType));
            }
            instructions.Add(Create(OpCodes.Stloc, bag.Variables.Result));

            return instructions;
        }

        private IList<Instruction>? StrictStateMachineSaveException(RouMethod rouMethod, IStateMachineFields fields, VariableDefinition vException)
        {
            if ((rouMethod.Features & (int)(Feature.OnException | Feature.OnSuccess | Feature.OnExit)) == 0) return null;

            return [
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.MethodContext),
                Create(OpCodes.Ldloc, vException),
                Create(OpCodes.Callvirt, _methodMethodContextSetExceptionRef)
            ];
        }

        private IList<Instruction> StrictAsyncIfExceptionNotHandled(RouMethod rouMethod, Instruction endAnchor, IStateMachineFields fields)
        {
            if (!Feature.ExceptionHandle.IsMatch(rouMethod.Features) || (rouMethod.MethodContextOmits & Omit.ReturnValue) != 0) return [Create(OpCodes.Rethrow)];

            return [
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetExceptionHandledRef),
                Create(OpCodes.Brtrue, endAnchor),
                Create(OpCodes.Rethrow)
            ];
        }

        private IList<Instruction>? StrictAsyncExceptionHandled(RouMethod rouMethod, MethodDefinition moveNextMethodDef, Instruction tailAnchor, StrictAsyncBag bag)
        {
            if (!Feature.ExceptionHandle.IsMatch(rouMethod.Features) || (rouMethod.MethodContextOmits & Omit.ReturnValue) != 0) return null;

            var instructions = new List<Instruction>();

            var endAnchor = Create(OpCodes.Leave, tailAnchor);

            if (bag.Variables.Result != null)
            {
                instructions.AddRange(AssignResultFromContext(bag));
            }
            instructions.AddRange(StateMachineOnExit(rouMethod, moveNextMethodDef, endAnchor, bag.Fields));
            instructions.Add(endAnchor);

            return instructions;
        }

        private IList<Instruction> StrictAsyncLoadArguments(FieldReference?[] parameters)
        {
            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldc_I4, parameters.Length),
                Create(OpCodes.Newarr, _typeObjectRef)
            };
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                instructions.Add(Create(OpCodes.Dup));

                instructions.Add(Create(OpCodes.Ldc_I4, i));
                if (parameter == null)
                {
                    instructions.Add(Create(OpCodes.Ldnull));
                }
                else
                {
                    instructions.Add(Create(OpCodes.Ldarg_0));
                    instructions.Add(Create(OpCodes.Ldfld, parameter));
                    if (parameter.FieldType.IsValueType || parameter.FieldType.IsGenericParameter)
                    {
                        instructions.Add(Create(OpCodes.Box, parameter.FieldType));
                    }
                }
                instructions.Add(Create(OpCodes.Stelem_Ref));
            }

            return instructions;
        }

        private IList<Instruction> AssignResultFromContext(StrictAsyncBag bag)
        {
            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, bag.Fields.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueRef)
            };
            if (bag.Variables.Result!.VariableType.FullName != typeof(object).FullName)
            {
                var castOp = bag.Variables.Result.VariableType.NeedBox() ? OpCodes.Unbox_Any : OpCodes.Castclass;
                instructions.Add(Create(castOp, bag.Variables.Result.VariableType));
            }
            instructions.Add(Create(OpCodes.Stloc, bag.Variables.Result));

            return instructions;
        }

        private void StrictAsyncDefaultMoContext(Mono.Collections.Generic.Collection<Instruction> instructions, AsyncFields fields)
        {
            if (fields.MoArray != null)
            {
                // this._mos = null;
                instructions.Add(Create(OpCodes.Ldarg_0));
                instructions.Add(Create(OpCodes.Ldnull));
                instructions.Add(Create(OpCodes.Stfld, fields.MoArray));
            }
            else
            {
                foreach (var mo in fields.Mos)
                {
                    // this._mo = null;
                    instructions.Add(Create(OpCodes.Ldarg_0));
                    instructions.Add(Create(OpCodes.Ldnull));
                    instructions.Add(Create(OpCodes.Stfld, mo));
                }
            }

            // this._context = null;
            instructions.Add(Create(OpCodes.Ldarg_0));
            instructions.Add(Create(OpCodes.Ldnull));
            instructions.Add(Create(OpCodes.Stfld, fields.MethodContext));
        }
    }
}
