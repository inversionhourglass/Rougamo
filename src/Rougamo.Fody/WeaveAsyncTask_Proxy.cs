using Mono.Cecil;
using Mono.Cecil.Cil;
using Rougamo.Fody.Enhances;
using Rougamo.Fody.Enhances.Async;
using Rougamo.Fody.Simulations;
using Rougamo.Fody.Simulations.Types;
using System.Collections.Generic;
using System.Linq;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void ProxyAsyncTaskMethodWeave(RouMethod rouMethod, TypeDefinition stateMachineTypeDef)
        {
            var actualStateMachineTypeDef = ProxyStateMachineClone(stateMachineTypeDef);
            if (actualStateMachineTypeDef == null) return;

            var actualMethodDef = ProxyStateMachineSetupMethodClone(rouMethod.MethodDef, stateMachineTypeDef, actualStateMachineTypeDef, Constants.TYPE_AsyncStateMachineAttribute);
            rouMethod.MethodDef.DeclaringType.Methods.Add(actualMethodDef);

            var actualMoveNextDef = actualStateMachineTypeDef.Methods.Single(m => m.Name == Constants.METHOD_MoveNext);
            actualMoveNextDef.DebugInformation.StateMachineKickOffMethod = actualMethodDef;

            var moveNextDef = stateMachineTypeDef.Methods.Single(m => m.Name == Constants.METHOD_MoveNext);
            moveNextDef.Clear();
            var context = ProxyCallAsync(rouMethod, stateMachineTypeDef, moveNextDef, actualMethodDef);
            ProxyAsyncWeave(rouMethod, context, moveNextDef);

            moveNextDef.DebuggerStepThrough(_methodDebuggerStepThroughCtorRef);
            moveNextDef.Body.OptimizePlus(EmptyInstructions);
        }

        private void ProxyCallWithWeaving(RouMethod rouMethod, TypeDefinition proxyStateMachineTypeDef, MethodDefinition proxyMoveNextDef, MethodDefinition actualMethodDef)
        {
            var genericMap = proxyStateMachineTypeDef.GenericParameters.ToDictionary(x => x.Name, x => x);

            var needInnerCatch = (Feature.OnException | Feature.OnExit).HasIntersection(rouMethod.Features);
            var anyExceptionExitAsync = rouMethod.Mos.Any(x => !(ForceSync.OnException | ForceSync.OnExit).SubsetOf(x.ForceSync));
            var anyAsync = rouMethod.Mos.Any(x => x.ForceSync != ForceSync.All);

            var proxyStateMachineTypeRef = proxyStateMachineTypeDef.MakeReference();
            var returnType = actualMethodDef.ReturnType.Simulate<TsAsyncable>(ModuleDefinition);
            var awaiterType = returnType.M_GetAwaiter.ReturnType;
            if (returnType.Ref is GenericInstanceType)
            {
                returnType.ReplaceGenerics(genericMap);
                awaiterType.SetGenerics(((GenericInstanceType)returnType.Ref).GenericArguments.ToArray());
            }
            var declaringTypeRef = actualMethodDef.DeclaringType.MakeReference().ReplaceGenericArgs(genericMap);
            var actualMethodRef = actualMethodDef.WithGenericDeclaringType(declaringTypeRef);
            if (proxyStateMachineTypeDef.HasGenericParameters)
            {
                var parentGenericNames = proxyStateMachineTypeDef.DeclaringType.GenericParameters.Select(x => x.Name).ToArray();
                var generics = proxyStateMachineTypeDef.GenericParameters.Where(x => !parentGenericNames.Contains(x.Name)).ToArray();
                actualMethodRef = actualMethodRef.WithGenerics(generics);
            }

            var fields = AsyncResolveFields(rouMethod, proxyStateMachineTypeDef);
            ProxyAsyncSetAbsentFields(rouMethod, proxyStateMachineTypeDef, fields);

            var builderType = fields.Builder.FieldType.Simulate<TsAsyncBuilder>(ModuleDefinition);
            //builderType.MAwaitUnsafeOnCompleted.SetGenerics(awaiterType, proxyStateMachineTypeRef);

            ProxyFieldCleanup(proxyStateMachineTypeDef, fields);
            var fAwaiter = new FieldDefinition(Constants.FIELD_Awaiter, FieldAttributes.Private, awaiterType);
            proxyStateMachineTypeDef.Fields.Add(fAwaiter);
            fields.Awaiter = fAwaiter;
            if (awaiterType.Ref is GenericInstanceType gitt)
            {
                var fResult = new FieldDefinition(Constants.FIELD_Result, FieldAttributes.Private, gitt.GenericArguments.Single());
                proxyStateMachineTypeDef.Fields.Add(fResult);
                fields.Result = fResult;
            }
            var diffTypeOfAwaiters = false;
            var moAwaiterType = awaiterType;
            if (anyAsync)
            {
                if (returnType.Ref.Is(Constants.TYPE_ValueTask))
                {
                    fields.MoAwaiter = fields.Awaiter;
                }
                else
                {
                    moAwaiterType = _typeValuetask.M_GetAwaiter.ReturnType;
                    var fMoAwaiter = new FieldDefinition(Constants.FIELD_MoAwaiter, FieldAttributes.Private, moAwaiterType);
                    proxyStateMachineTypeDef.Fields.Add(fMoAwaiter);
                    fields.MoAwaiter = fMoAwaiter;
                    diffTypeOfAwaiters = true;
                }
            }

            var vState = proxyMoveNextDef.Body.CreateVariable(_typeIntRef);
            var vException = proxyMoveNextDef.Body.CreateVariable(_typeExceptionRef);
            var vInnerException = needInnerCatch ? proxyMoveNextDef.Body.CreateVariable(_typeExceptionRef) : null;
            var vTempInnerException = needInnerCatch && anyExceptionExitAsync ? proxyMoveNextDef.Body.CreateVariable(_typeExceptionRef) : null;
            var vAwaiter = proxyMoveNextDef.Body.CreateVariable(awaiterType);
            var vMoAwaiter = diffTypeOfAwaiters ? proxyMoveNextDef.Body.CreateVariable(moAwaiterType) : vAwaiter;
            VariableDefinition? vThis = null;
            VariableDefinition? vTargetReturn = null;
            if (!proxyStateMachineTypeDef.IsValueType)
            {
                vThis = proxyMoveNextDef.Body.CreateVariable(proxyStateMachineTypeRef);
            }
            if (returnType.IsValueType)
            {
                vTargetReturn = proxyMoveNextDef.Body.CreateVariable(returnType);
            }

            var builderIsValueType = fields.Builder.FieldType.IsValueType;
            var opBuilderLdfld = builderIsValueType ? OpCodes.Ldflda : OpCodes.Ldfld;
            var opBuilderCall = builderIsValueType ? OpCodes.Call : OpCodes.Callvirt;
            var awaiterIsValueType = awaiterType.IsValueType;
            var opAwaiterLdloc = awaiterIsValueType ? OpCodes.Ldloca : OpCodes.Ldloc;
            var opAwaiterCall = awaiterIsValueType ? OpCodes.Call : OpCodes.Callvirt;
            var opGetAwaiterCall = returnType.IsValueType ? OpCodes.Call : OpCodes.Callvirt;

            var switchs = Create(OpCodes.Switch, []);
            var nopCase0 = Create(OpCodes.Nop);
            var nopInnerCatchStart = Create(OpCodes.Nop);
            var nopInnerCatchEnd = Create(OpCodes.Nop);
            var nopOuterCatchEnd = Create(OpCodes.Nop);
            var ret = Create(OpCodes.Ret);

            var instructions = proxyMoveNextDef.Body.Instructions;

            // var state = this._state;
            instructions.Add(Create(OpCodes.Ldarg_0));
            instructions.Add(Create(OpCodes.Ldfld, fields.State));
            instructions.Add(Create(OpCodes.Stloc, vState));
            // -try
            {
                instructions.Add(switchs);
                // switch (state)
                {
                    // default:
                    {
                        instructions.Add(ProxyStateMachineInitMos(proxyMoveNextDef, rouMethod.Mos, fields));
                        instructions.Add(ProxyStateMachineInitMethodContext(rouMethod, proxyMoveNextDef, fields));
                        for (var i = 0; i < rouMethod.Mos.Length; i++)
                        {
                            var mo = rouMethod.Mos[i];
                            if (mo.ForceSync.Contains(ForceSync.OnEntry))
                            {
                                instructions.Add(ExecuteMoMethod(Constants.METHOD_OnEntry, mo, fields.Mos[i], fields.MethodContext));
                            }
                            else
                            {
                                instructions.Add(ExecuteMoMethod(Constants.METHOD_OnEntryAsync, mo, fields.Mos[i], fields.MethodContext));
                                //instructions.Add()
                            }
                        }
                        //instructions.Add(StateMachineOnEntry(rouMethod, proxyMoveNextDef, null, context.Fields));
                    }
                    // case 0:
                    // -try
                    {
                        var nopStateNotZero = Create(OpCodes.Nop);
                        var nopAwiterGetResult = Create(OpCodes.Nop);
                        instructions.Add(nopCase0.Set(OpCodes.Ldloc, vState));
                        instructions.Add(Create(OpCodes.Brfalse, nopStateNotZero));
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
                            instructions.Add(Create(opGetAwaiterCall, returnType.M_GetAwaiter));
                            instructions.Add(Create(OpCodes.Stloc, vAwaiter));

                            instructions.Add(Create(opAwaiterLdloc, vAwaiter));
                            instructions.Add(Create(opAwaiterCall, awaiterType.P_IsCompleted.Getter));
                            instructions.Add(Create(OpCodes.Brtrue, nopAwiterGetResult));
                            // -if (!awaiter.IsCompleted)
                            {
                                // this._state = 0;
                                instructions.Add(Create(OpCodes.Ldarg_0));
                                instructions.Add(Create(OpCodes.Ldc_I4_0));
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
                                instructions.Add(Create(opBuilderCall, builderType.M_AwaitUnsafeOnCompleted));

                                // return;
                                instructions.Add(Create(OpCodes.Leave, ret));
                            }
                        }
                        // -else (state == 0)
                        {
                            // awaiter = this._awaiter;
                            instructions.Add(nopStateNotZero.Set(OpCodes.Ldarg_0));
                            instructions.Add(Create(OpCodes.Ldfld, fields.Awaiter));
                            instructions.Add(Create(OpCodes.Stloc, vAwaiter));
                        }
                        // this._result = awaiter.GetResult(); <--> awaiter.GetResult();
                        if (fields.Result != null) instructions.Add(Create(OpCodes.Ldarg_0));
                        instructions.Add(nopAwiterGetResult.Set(opAwaiterLdloc, vAwaiter));
                        instructions.Add(Create(opAwaiterCall, awaiterType.M_GetResult));
                        if (fields.Result != null) instructions.Add(Create(OpCodes.Stfld, fields.Result));

                        // this._context.ReturnValue = this._result;
                        instructions.Add(ProxyAsyncSaveReturnValue(rouMethod, fields));

                        if (needInnerCatch) instructions.Add(Create(OpCodes.Leave, nopInnerCatchEnd));
                    }
                    // -catch (Exception ex)
                    if (needInnerCatch)
                    {
                        instructions.Add(nopInnerCatchStart.Set(OpCodes.Stloc, vInnerException));
                        if (anyExceptionExitAsync)
                        {
                            // tempEx = ex;
                            instructions.Add(Create(OpCodes.Ldloc, vInnerException));
                            instructions.Add(Create(OpCodes.Stloc, vTempInnerException));
                        }
                        else
                        {
                            // this._context.Exception = ex;
                            instructions.Add(ProxyStateMachineSaveException(rouMethod, fields, vInnerException!));
                            // this._mo.OnException(this._context);
                            instructions.Add(StateMachineOnException(rouMethod, proxyMoveNextDef, null, fields));
                            // if (this._context.RetryCount > 0) goto case 0;
                            instructions.Add(ProxyAsyncIfExceptionRetry(rouMethod, nopCase0, null, fields));
                            // if (this._context.ExceptionHandled) this._result = this._context.ReturnValue;
                            instructions.Add(ProxyAsyncSaveExceptionHandledResult(rouMethod, fields, null));
                            // this._mo.OnExit(this._context);
                            instructions.Add(StateMachineOnExit(rouMethod, proxyMoveNextDef, null, fields));
                            // if (this._context.ExceptionHandled) goto end;
                            // throw;
                            instructions.Add(ProxyAsyncCheckExceptionHandled(rouMethod, nopOuterCatchEnd, fields));
                        }
                    }
                    // -if (tempEx != null)
                    if (anyExceptionExitAsync)
                    {
                        // this._context.Exception = tempEx;
                        instructions.Add(ProxyStateMachineSaveException(rouMethod, fields, vTempInnerException!));
                        // moAwaiter = this._mo.OnExceptionAsync(this._context).GetAwaiter();

                    }
                }
            }
            // -catch (Exception e)
            {

            }
        }

        private ProxyAsyncContext ProxyCallAsync(RouMethod rouMethod, TypeDefinition proxyStateMachineTypeDef, MethodDefinition proxyMoveNextDef, MethodDefinition actualMethodDef)
        {
            var genericMap = proxyStateMachineTypeDef.GenericParameters.ToDictionary(x => x.Name, x => x);

            var proxyStateMachineTypeRef = proxyStateMachineTypeDef.MakeReference();
            var returnType = actualMethodDef.ReturnType.Simulate<TsAsyncable>(ModuleDefinition);
            var awaiterType = returnType.M_GetAwaiter.ReturnType;
            if (returnType.Ref is GenericInstanceType)
            {
                returnType.ReplaceGenerics(genericMap);
                awaiterType.SetGenerics(((GenericInstanceType)returnType.Ref).GenericArguments.ToArray());
            }
            var declaringTypeRef = actualMethodDef.DeclaringType.MakeReference().ReplaceGenericArgs(genericMap);
            var actualMethodRef = actualMethodDef.WithGenericDeclaringType(declaringTypeRef);
            if (proxyStateMachineTypeDef.HasGenericParameters)
            {
                var parentGenericNames = proxyStateMachineTypeDef.DeclaringType.GenericParameters.Select(x => x.Name).ToArray();
                var generics = proxyStateMachineTypeDef.GenericParameters.Where(x => !parentGenericNames.Contains(x.Name)).ToArray();
                actualMethodRef = actualMethodRef.WithGenerics(generics);
            }

            var fields = AsyncResolveFields(rouMethod, proxyStateMachineTypeDef);
            ProxyAsyncSetAbsentFields(rouMethod, proxyStateMachineTypeDef, fields);

            var builderType = fields.Builder.FieldType.Simulate<TsAsyncBuilder>(ModuleDefinition);

            ProxyFieldCleanup(proxyStateMachineTypeDef, fields);
            var fAwaiter = new FieldDefinition(Constants.FIELD_Awaiter, FieldAttributes.Private, awaiterType);
            proxyStateMachineTypeDef.Fields.Add(fAwaiter);
            fields.Awaiter = fAwaiter;

            var vState = proxyMoveNextDef.Body.CreateVariable(_typeIntRef);
            var vAwaiter = proxyMoveNextDef.Body.CreateVariable(awaiterType);
            var vException = proxyMoveNextDef.Body.CreateVariable(_typeExceptionRef);
            VariableDefinition? vThis = null;
            VariableDefinition? vTargetReturn = null;
            VariableDefinition? vResult = null;
            if (!proxyStateMachineTypeDef.IsValueType)
            {
                vThis = proxyMoveNextDef.Body.CreateVariable(proxyStateMachineTypeRef);
            }
            if (returnType.IsValueType)
            {
                vTargetReturn = proxyMoveNextDef.Body.CreateVariable(returnType);
            }
            if (awaiterType.Ref is GenericInstanceType gitt)
            {
                vResult = proxyMoveNextDef.Body.CreateVariable(gitt.GenericArguments.Single());
            }

            var builderIsValueType = fields.Builder.FieldType.IsValueType;
            var opBuilderLdfld = builderIsValueType ? OpCodes.Ldflda : OpCodes.Ldfld;
            var opBuilderCall = builderIsValueType ? OpCodes.Call : OpCodes.Callvirt;
            var awaiterIsValueType = awaiterType.IsValueType;
            var opAwaiterLdloc = awaiterIsValueType ? OpCodes.Ldloca : OpCodes.Ldloc;
            var opAwaiterCall = awaiterIsValueType ? OpCodes.Call : OpCodes.Callvirt;
            var opGetAwaiterCall = returnType.IsValueType ? OpCodes.Call : OpCodes.Callvirt;

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
                    instructions.Add(Create(opGetAwaiterCall, returnType.M_GetAwaiter));
                    instructions.Add(Create(OpCodes.Stloc, vAwaiter));

                    instructions.Add(Create(opAwaiterLdloc, vAwaiter));
                    instructions.Add(Create(opAwaiterCall, awaiterType.P_IsCompleted.Getter));
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
                        instructions.Add(Create(opBuilderCall, builderType.M_AwaitUnsafeOnCompleted.Ref.WithGenerics(awaiterType, proxyStateMachineTypeRef)));

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
                    if (awaiterIsValueType)
                    {
                        instructions.Add(Create(OpCodes.Ldflda, fields.Awaiter));
                        instructions.Add(Create(OpCodes.Initobj, awaiterType));
                    }
                    else
                    {
                        instructions.Add(Create(OpCodes.Ldnull));
                        instructions.Add(Create(OpCodes.Stfld, fields.Awaiter));
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
                instructions.Add(Create(opAwaiterCall, awaiterType.M_GetResult));
                if (vResult != null)
                {
                    instructions.Add(Create(OpCodes.Stloc, vResult));
                }

                instructions.Add(Create(OpCodes.Leave, nopCatchEnd));
            }
            // catch (Exception ex)
            {
                instructions.Add(nopCatchStart.Set(OpCodes.Stloc, vException));

                // this._state = -2;
                instructions.Add(Create(OpCodes.Ldarg_0));
                instructions.Add(Create(OpCodes.Ldc_I4, -2));
                instructions.Add(Create(OpCodes.Stfld, fields.State));

                ProxyAsyncDefaultMoContext(instructions, fields);

                // this._builder.SetException(ex);
                instructions.Add(Create(OpCodes.Ldarg_0));
                instructions.Add(Create(opBuilderLdfld, fields.Builder));
                instructions.Add(Create(OpCodes.Ldloc, vException));
                instructions.Add(Create(opBuilderCall, builderType.M_SetException));

                // return;
                instructions.Add(Create(OpCodes.Leave, ret));
            }
            // this._state = -2;
            instructions.Add(nopCatchEnd.Set(OpCodes.Ldarg_0));
            instructions.Add(Create(OpCodes.Ldc_I4, -2));
            instructions.Add(Create(OpCodes.Stfld, fields.State));

            ProxyAsyncDefaultMoContext(instructions, fields);

            // this._builder.SetResult(result); <--> this._builder.SetResult();
            instructions.Add(Create(OpCodes.Ldarg_0));
            instructions.Add(Create(opBuilderLdfld, fields.Builder));
            if (vResult != null)
            {
                instructions.Add(Create(OpCodes.Ldloc, vResult));
            }
            instructions.Add(Create(opBuilderCall, builderType.M_SetResult));

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

        private void ProxyAsyncWeave(RouMethod rouMethod, ProxyAsyncContext context, MethodDefinition proxyMoveNextDef)
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
            tryStart.Set(OpCodes.Ldloc, context.Variables.State);
            instructions.InsertBefore(oldTryStart, Create(OpCodes.Brfalse, nopRetryLoopStart));
            // -if (state != 0)
            {
                instructions.InsertBefore(oldTryStart, ProxyStateMachineInitMos(proxyMoveNextDef, rouMethod.Mos, context.Fields));
                instructions.InsertBefore(oldTryStart, ProxyStateMachineInitMethodContext(rouMethod, proxyMoveNextDef, context.Fields));
                instructions.InsertBefore(oldTryStart, StateMachineOnEntry(rouMethod, proxyMoveNextDef, null, context.Fields));
                instructions.InsertBefore(oldTryStart, ProxyAsyncIfOnEntryReplacedReturn(rouMethod, proxyMoveNextDef, null, outerCatchEnd, context));
                instructions.InsertBefore(oldTryStart, StateMachineRewriteArguments(rouMethod, nopRetryLoopStart, context.Fields));
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
            instructions.InsertBefore(tryLastLeave, ProxyAsyncSaveReturnValue(rouMethod, context));
            instructions.InsertBefore(tryLastLeave, StateMachineOnSuccess(rouMethod, proxyMoveNextDef, null, context.Fields));
            instructions.InsertBefore(tryLastLeave, ProxyAsyncIfSuccessRetry(rouMethod, nopRetryLoopStart, null, context.Fields));
            instructions.InsertBefore(tryLastLeave, ProxyAsyncIfSuccessReplacedReturn(rouMethod, null, context));
            instructions.InsertBefore(tryLastLeave, StateMachineOnExit(rouMethod, proxyMoveNextDef, tryLastLeave, context.Fields));

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
             *         }
             *         mo1.OnExit(context);
             *         
             *         if (context.ExceptionHandled) break;
             *         throw;
             *     }
             * } // while true end
             */
            if ((rouMethod.Features & (int)(Feature.OnException | Feature.OnExit)) != 0)
            {
                instructions.InsertBefore(outerCatchStart, innerCatchStart);
                instructions.InsertBefore(outerCatchStart, ProxyStateMachineSaveException(rouMethod, context.Fields, vInnerException));
                instructions.InsertBefore(outerCatchStart, StateMachineOnException(rouMethod, proxyMoveNextDef, null, context.Fields));
                instructions.InsertBefore(outerCatchStart, ProxyAsyncIfExceptionRetry(rouMethod, nopRetryLoopStart, null, context.Fields));
                instructions.InsertBefore(outerCatchStart, ProxyAsyncSaveExceptionHandledResult(rouMethod, context.Fields, context.Variables.Result));
                instructions.InsertBefore(outerCatchStart, StateMachineOnExit(rouMethod, proxyMoveNextDef, null, context.Fields));
                instructions.InsertBefore(outerCatchStart, ProxyAsyncCheckExceptionHandled(rouMethod, outerCatchEnd, context.Fields));
            }

            outerHandler.TryStart = tryStart;
            outerHandler.TryEnd = outerCatchStart;
            outerHandler.HandlerStart = outerCatchStart;
            outerHandler.HandlerEnd = outerCatchEnd;

            if ((rouMethod.Features & (int)(Feature.OnException | Feature.OnExit)) != 0)
            {
                proxyMoveNextDef.Body.ExceptionHandlers.Insert(0, new ExceptionHandler(ExceptionHandlerType.Catch)
                {
                    TryStart = oldTryStart,
                    TryEnd = innerCatchStart,
                    HandlerStart = innerCatchStart,
                    HandlerEnd = outerCatchStart,
                    CatchType = _typeExceptionRef
                });
            }
        }

        private TypeDefinition? ProxyStateMachineClone(TypeDefinition stateMachineTypeDef)
        {
            var typeName = $"$Rougamo_{stateMachineTypeDef.Name}";
            if (stateMachineTypeDef.DeclaringType.NestedTypes.Any(x => x.Name == typeName)) return null;

            var actualTypeDef = stateMachineTypeDef.Clone(typeName);
            actualTypeDef.DeclaringType.NestedTypes.Add(actualTypeDef);

            return actualTypeDef;
        }

        private MethodDefinition ProxyStateMachineSetupMethodClone(MethodDefinition methodDef, TypeDefinition stateMachineTypeDef, TypeDefinition clonedStateMachineTypeDef, string stateMachineAttributeTypeName)
        {
            var clonedMethodDef = methodDef.Clone($"$Rougamo_{methodDef.Name}");

            ProxyStateMachineAttributeClone(clonedMethodDef, clonedStateMachineTypeDef, stateMachineAttributeTypeName);

            var genericMap = methodDef.DeclaringType.GenericParameters.ToDictionary(x => x.Name, x => x);
            genericMap.AddRange(clonedMethodDef.GenericParameters.ToDictionary(x => x.Name, x => x));
            var stateMachineVariableTypeRef = clonedStateMachineTypeDef.MakeReference().ReplaceGenericArgs(genericMap);
            var vStateMachine = clonedMethodDef.Body.Variables.SingleOrDefault(x => x.VariableType.Resolve() == stateMachineTypeDef);
            VariableDefinition? vClonedStateMachine = null;
            if (vStateMachine != null)
            {
                var index = clonedMethodDef.Body.Variables.IndexOf(vStateMachine);
                clonedMethodDef.Body.Variables.Remove(vStateMachine);
                vClonedStateMachine = new VariableDefinition(stateMachineVariableTypeRef);
                clonedMethodDef.Body.Variables.Insert(index, vClonedStateMachine);
            }

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

        private void ProxyStateMachineAttributeClone(MethodDefinition clonedMethodDef, TypeDefinition clonedStateMachineTypeDef, string stateMachineAttributeTypeName)
        {
            var stateMachineAttribute = clonedMethodDef.CustomAttributes.Single(x => x.Is(stateMachineAttributeTypeName));
            clonedMethodDef.CustomAttributes.Remove(stateMachineAttribute);

            if (!_stateMachineCtorRefs.TryGetValue(stateMachineAttributeTypeName, out var ctor))
            {
                ctor = stateMachineAttribute.AttributeType.Resolve().Methods.Single(x => x.IsConstructor && !x.IsStatic && x.Parameters.Single().ParameterType.Is(Constants.TYPE_Type)).ImportInto(ModuleDefinition);
                _stateMachineCtorRefs[stateMachineAttributeTypeName] = ctor;
            }
            stateMachineAttribute = new CustomAttribute(ctor);
            stateMachineAttribute.ConstructorArguments.Add(new CustomAttributeArgument(_typeSystemRef, clonedStateMachineTypeDef));
            clonedMethodDef.CustomAttributes.Add(stateMachineAttribute);
        }

        private void ProxyFieldCleanup(TypeDefinition stateMachineTypeDef, IStateMachineFields fields)
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

        private void ProxyAsyncSetAbsentFields(RouMethod rouMethod, TypeDefinition stateMachineTypeDef, AsyncFields fields)
        {
            var instructions = rouMethod.MethodDef.Body.Instructions;
            var setState = instructions.Single(x => x.OpCode.Code == Code.Ldc_I4_M1 && x.Next.OpCode.Code == Code.Stfld && x.Next.Operand is FieldReference fr && fr.Resolve() == fields.State.Resolve());
            var vStateMachine = rouMethod.MethodDef.Body.Variables.Single(x => x.VariableType.Resolve() == stateMachineTypeDef);
            var genericMap = stateMachineTypeDef.GenericParameters.ToDictionary(x => x.Name, x => x);

            ProxyAddAbsentField(stateMachineTypeDef, ProxySetAbsentFieldThis(rouMethod, fields, vStateMachine.VariableType, vStateMachine.LdlocAny(), setState, genericMap));

            ProxyAddAbsentField(stateMachineTypeDef, ProxySetAbsentFieldParameters(rouMethod, fields, vStateMachine.VariableType, vStateMachine.LdlocAny(), setState, genericMap));
        }

        private IEnumerable<FieldDefinition> ProxySetAbsentFieldThis(RouMethod rouMethod, IStateMachineFields fields, TypeReference stateMachineTypeRef, Instruction loadStateMachine, Instruction anchor, Dictionary<string, GenericParameter> genericMap)
        {
            if (rouMethod.MethodDef.IsStatic || fields.DeclaringThis != null) yield break;

            var thisTypeRef = stateMachineTypeRef.DeclaringType.ReplaceGenericArgs(genericMap);
            var thisFieldDef = new FieldDefinition(Constants.FIELD_This, FieldAttributes.Public, thisTypeRef);
            var thisFieldRef = new FieldReference(thisFieldDef.Name, thisFieldDef.FieldType, stateMachineTypeRef);

            fields.DeclaringThis = thisFieldDef;

            rouMethod.MethodDef.Body.Instructions.InsertBefore(anchor, [
                loadStateMachine.Clone(),
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Stfld, thisFieldRef)
            ]);

            yield return thisFieldDef;
        }

        private IEnumerable<FieldDefinition> ProxySetAbsentFieldParameters(RouMethod rouMethod, IStateMachineFields fields, TypeReference stateMachineTypeRef, Instruction loadStateMachine, Instruction anchor, Dictionary<string, GenericParameter> genericMap)
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
                parameterFieldRef = new FieldReference(parameterFieldDef.Name, parameterFieldDef.FieldType, stateMachineTypeRef);

                fields.SetParameter(i, parameterFieldDef);

                instructions.InsertBefore(anchor, [
                    loadStateMachine.Clone(),
                    Create(OpCodes.Ldarg, parameter),
                    Create(OpCodes.Stfld, parameterFieldRef)
                ]);

                yield return parameterFieldDef;
            }
        }

        private void ProxyAddAbsentField(TypeDefinition typeDef, IEnumerable<FieldDefinition> fieldDefs)
        {
            foreach (var fieldDef in fieldDefs)
            {
                typeDef.Fields.Add(fieldDef);
            }
        }

        private IList<Instruction> ProxyStateMachineInitMos(MethodDefinition moveNextMethodDef, Mo[] mos, IStateMachineFields fields)
        {
            if (fields.MoArray != null)
            {
                var instructions = new List<Instruction> { Create(OpCodes.Ldarg_0) };
                instructions.AddRange(InitMoArray(moveNextMethodDef, mos));
                instructions.Add(Create(OpCodes.Stfld, fields.MoArray));

                return instructions;
            }

            return ProxyStateMachineInitMoFields(moveNextMethodDef, mos, fields.Mos);
        }

        private IList<Instruction> ProxyStateMachineInitMoFields(MethodDefinition methodDef, Mo[] mos, FieldReference[] moFields)
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

        private IList<Instruction> ProxyStateMachineInitMethodContext(RouMethod rouMethod, MethodDefinition moveNextMethodDef, IStateMachineFields fields)
        {
            var instructions = new List<Instruction>();
            VariableDefinition? moArray = null;
            if ((rouMethod.MethodContextOmits & Omit.Mos) == 0 && fields.MoArray == null)
            {
                moArray = moveNextMethodDef.Body.CreateVariable(_typeIMoArrayRef);
                instructions.AddRange(CreateTempMoArray(null, fields.Mos, rouMethod.Mos));
                instructions.Add(Create(OpCodes.Stloc, moArray));
            }

            instructions.Add(Create(OpCodes.Ldarg_0));
            instructions.AddRange(ProxyStateMachineInitMethodContext(rouMethod.MethodDef, rouMethod.IsAsyncTaskOrValueTask || rouMethod.IsAsyncIterator, rouMethod.IsIterator || rouMethod.IsAsyncIterator, moArray, fields, rouMethod.MethodContextOmits));
            instructions.Add(Create(OpCodes.Stfld, fields.MethodContext));

            return instructions;
        }

        private List<Instruction> ProxyStateMachineInitMethodContext(MethodDefinition methodDef, bool isAsync, bool isIterator, VariableDefinition? moArrayVariable, IStateMachineFields fields, Omit omit)
        {
            var instructions = new List<Instruction>();

            var isAsyncCode = isAsync ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0;
            var isIteratorCode = isIterator ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0;
            var mosNonEntryFIFO = _config.ReverseCallNonEntry ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1;
            if (fields.DeclaringThis != null)
            {
                instructions.Add(Create(OpCodes.Ldarg_0));
                instructions.Add(Create(OpCodes.Ldfld, fields.DeclaringThis));
                if (fields.DeclaringThis.FieldType.IsValueType)
                {
                    instructions.Add(Create(OpCodes.Box, fields.DeclaringThis.FieldType));
                }
            }
            else
            {
                instructions.Add(Create(OpCodes.Ldnull));
            }
            instructions.Add(Create(OpCodes.Ldtoken, methodDef.DeclaringType));
            instructions.Add(Create(OpCodes.Call, _methodGetTypeFromHandleRef));
            instructions.AddRange(LoadMethodBaseOnStack(methodDef));
            instructions.Add(Create(isAsyncCode));
            instructions.Add(Create(isIteratorCode));
            instructions.Add(Create(mosNonEntryFIFO));
            if ((omit & Omit.Mos) != 0)
            {
                instructions.Add(Create(OpCodes.Ldnull));
            }
            else if (fields.MoArray == null)
            {
                instructions.Add(Create(OpCodes.Ldloc, moArrayVariable));
            }
            else
            {
                instructions.Add(Create(OpCodes.Ldarg_0));
                instructions.Add(Create(OpCodes.Ldfld, fields.MoArray));
            }
            if ((omit & Omit.Arguments) != 0)
            {
                instructions.Add(Create(OpCodes.Ldnull));
            }
            else
            {
                instructions.AddRange(ProxyAsyncLoadArguments(fields.Parameters));
            }
            instructions.Add(Create(OpCodes.Newobj, _methodMethodContextCtorRef));

            return instructions;
        }

        private IList<Instruction>? ProxyAsyncIfOnEntryReplacedReturn(RouMethod rouMethod, MethodDefinition moveNextMethodDef, Instruction? endAnchor, Instruction tailAnchor, ProxyAsyncContext context)
        {
            if (!Feature.EntryReplace.SubsetOf(rouMethod.Features) || (rouMethod.MethodContextOmits & Omit.ReturnValue) != 0) return null;

            var managedEndAnchor = endAnchor == null;
            if (managedEndAnchor) endAnchor = Create(OpCodes.Nop);

            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, context.Fields.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueReplacedRef),
                Create(OpCodes.Brfalse_S, endAnchor),
            };
            if (context.Variables.Result != null)
            {
                instructions.AddRange(AssignResultFromContext(context.Fields, context.Variables.Result));
            }
            var onExitEndAnchor = Create(OpCodes.Leave, tailAnchor);
            instructions.AddRange(StateMachineOnExit(rouMethod, moveNextMethodDef, onExitEndAnchor, context.Fields));
            instructions.Add(onExitEndAnchor);

            if (managedEndAnchor) instructions.Add(endAnchor!);

            return instructions;
        }

        private IList<Instruction>? ProxyAsyncSaveReturnValue(RouMethod rouMethod, ProxyAsyncContext context)
        {
            if (context.Variables.Result == null || (rouMethod.Features & (int)(Feature.OnSuccess | Feature.OnExit)) == 0 || (rouMethod.MethodContextOmits & Omit.ReturnValue) != 0) return null;

            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, context.Fields.MethodContext),
                Create(OpCodes.Ldloc, context.Variables.Result)
            };
            if (context.Variables.Result.VariableType.NeedBox())
            {
                instructions.Add(Create(OpCodes.Box, context.Variables.Result.VariableType));
            }
            instructions.Add(Create(OpCodes.Callvirt, _methodMethodContextSetReturnValueRef));

            return instructions;
        }

        private IList<Instruction>? ProxyAsyncSaveReturnValue(RouMethod rouMethod, AsyncFields fields)
        {
            if (fields.Result == null || (rouMethod.Features & (int)(Feature.OnSuccess | Feature.OnExit)) == 0 || (rouMethod.MethodContextOmits & Omit.ReturnValue) != 0) return null;

            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.MethodContext),
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.Result)
            };
            if (fields.Result.FieldType.NeedBox())
            {
                instructions.Add(Create(OpCodes.Box, fields.Result.FieldType));
            }
            instructions.Add(Create(OpCodes.Callvirt, _methodMethodContextSetReturnValueRef));

            return instructions;
        }

        private IList<Instruction>? ProxyAsyncIfSuccessRetry(RouMethod rouMethod, Instruction loopStartAnchor, Instruction? endAnchor, AsyncFields fields)
        {
            return !Feature.SuccessRetry.SubsetOf(rouMethod.Features) ? null : ProxyAsyncIfRetry(rouMethod, loopStartAnchor, endAnchor, fields);
        }

        private IList<Instruction>? ProxyAsyncIfExceptionRetry(RouMethod rouMethod, Instruction loopStartAnchor, Instruction? endAnchor, AsyncFields fields)
        {
            return !Feature.ExceptionRetry.SubsetOf(rouMethod.Features) ? null : ProxyAsyncIfRetry(rouMethod, loopStartAnchor, endAnchor, fields);
        }

        private IList<Instruction> ProxyAsyncIfRetry(RouMethod rouMethod, Instruction loopStartAnchor, Instruction? endAnchor, AsyncFields fields)
        {
            var managedAnchor = endAnchor == null;
            if (managedAnchor) endAnchor = Create(OpCodes.Nop);

            List<Instruction> instructions = [
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetRetryCountRef),
                Create(OpCodes.Ldc_I4_0),
                Create(OpCodes.Ble, endAnchor),
                Create(OpCodes.Leave_S, loopStartAnchor)
            ];

            if (managedAnchor) instructions.Add(endAnchor!);

            return instructions;
        }

        private IList<Instruction>? ProxyAsyncIfSuccessReplacedReturn(RouMethod rouMethod, Instruction? endAnchor, ProxyAsyncContext context)
        {
            if (context.Variables.Result == null || !Feature.SuccessReplace.SubsetOf(rouMethod.Features) || (rouMethod.MethodContextOmits & Omit.ReturnValue) != 0) return EmptyInstructions;

            var managedAnchor = endAnchor == null;
            if (managedAnchor) endAnchor = Create(OpCodes.Nop);

            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, context.Fields.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueReplacedRef),
                Create(OpCodes.Brfalse_S, endAnchor),
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, context.Fields.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueRef),
            };
            if (context.Variables.Result.VariableType.FullName != typeof(object).FullName)
            {
                var castOp = context.Variables.Result.VariableType.NeedBox() ? OpCodes.Unbox_Any : OpCodes.Castclass;
                instructions.Add(Create(castOp, context.Variables.Result.VariableType));
            }
            instructions.Add(Create(OpCodes.Stloc, context.Variables.Result));

            if (managedAnchor) instructions.Add(endAnchor!);

            return instructions;
        }

        private IList<Instruction>? ProxyStateMachineSaveException(RouMethod rouMethod, IStateMachineFields fields, VariableDefinition vException)
        {
            if ((rouMethod.Features & (int)(Feature.OnException | Feature.OnSuccess | Feature.OnExit)) == 0) return null;

            return [
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.MethodContext),
                Create(OpCodes.Ldloc, vException),
                Create(OpCodes.Callvirt, _methodMethodContextSetExceptionRef)
            ];
        }

        private IList<Instruction>? ProxyAsyncSaveExceptionHandledResult(RouMethod rouMethod, AsyncFields fields, VariableDefinition? vResult)
        {
            if (!Feature.ExceptionHandle.SubsetOf(rouMethod.Features) || (rouMethod.MethodContextOmits & Omit.ReturnValue) != 0) return null;

            if (vResult == null && fields.Result == null) return null;

            var endAnchor = Create(OpCodes.Nop);

            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetExceptionHandledRef),
                Create(OpCodes.Brfalse, endAnchor)
            };

            if (vResult == null)
            {
                instructions.AddRange(AssignResultFromContext(fields));
            }
            else
            {
                instructions.AddRange(AssignResultFromContext(fields, vResult));
            }
            instructions.Add(endAnchor);

            return instructions;
        }

        private IList<Instruction> ProxyAsyncCheckExceptionHandled(RouMethod rouMethod, Instruction tailAnchor, IStateMachineFields fields)
        {
            if (!Feature.ExceptionHandle.SubsetOf(rouMethod.Features) || (rouMethod.MethodContextOmits & Omit.ReturnValue) != 0) return [Create(OpCodes.Rethrow)];

            var rethrow = Create(OpCodes.Rethrow);

            return [
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetExceptionHandledRef),
                Create(OpCodes.Brfalse, rethrow),
                Create(OpCodes.Leave, tailAnchor),
                rethrow
            ];
        }

        private IList<Instruction> ProxyAsyncLoadArguments(FieldReference?[] parameters)
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

        private IList<Instruction> AssignResultFromContext(AsyncFields fields, VariableDefinition vResult)
        {
            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueRef)
            };
            if (vResult.VariableType.FullName != typeof(object).FullName)
            {
                var castOp = vResult.VariableType.NeedBox() ? OpCodes.Unbox_Any : OpCodes.Castclass;
                instructions.Add(Create(castOp, vResult.VariableType));
            }
            instructions.Add(Create(OpCodes.Stloc, vResult));

            return instructions;
        }

        private IList<Instruction> AssignResultFromContext(AsyncFields fields)
        {
            var instructions = new List<Instruction>
            {
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.MethodContext),
                Create(OpCodes.Callvirt, _methodMethodContextGetReturnValueRef)
            };
            var resultTypeRef = fields.Result!.FieldType;
            if (resultTypeRef.FullName != typeof(object).FullName)
            {
                var castOp = resultTypeRef.NeedBox() ? OpCodes.Unbox_Any : OpCodes.Castclass;
                instructions.Add(Create(castOp, resultTypeRef));
            }
            instructions.Add(Create(OpCodes.Stfld, fields.Result));

            return instructions;
        }

        private void ProxyAsyncDefaultMoContext(Mono.Collections.Generic.Collection<Instruction> instructions, AsyncFields fields)
        {
            if (fields.MoArray != null)
            {
                // this._mos = null;
                instructions.Add(ProxyAsyncFieldDefault(fields.MoArray));
            }
            else
            {
                foreach (var mo in fields.Mos)
                {
                    // this._mo = null;
                    instructions.Add(ProxyAsyncFieldDefault(mo));
                }
            }

            // this._context = null;
            instructions.Add(ProxyAsyncFieldDefault(fields.MethodContext));
        }

        private IList<Instruction> ProxyAsyncFieldDefault(FieldReference fieldRef)
        {
            var fieldType = fieldRef.FieldType;
            if (fieldType.IsValueType || fieldType.IsGenericParameter)
            {
                return [
                    Create(OpCodes.Ldarg_0),
                    Create(OpCodes.Ldflda, fieldRef),
                    Create(OpCodes.Initobj, fieldType)
                ];
            }

            return [
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldnull),
                Create(OpCodes.Stfld, fieldRef)
            ];
        }
    }
}
