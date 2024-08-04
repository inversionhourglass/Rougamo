using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Rougamo.Fody.Enhances.AsyncIterator;
using Rougamo.Fody.Simulations;
using Rougamo.Fody.Simulations.Operations;
using Rougamo.Fody.Simulations.PlainValues;
using Rougamo.Fody.Simulations.Types;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void WeavingAsyncIteratorMethod(RouMethod rouMethod)
        {
            var stateMachineTypeDef = rouMethod.MethodDef.ResolveStateMachine(Constants.TYPE_AsyncIteratorStateMachineAttribute);
            var actualStateMachineTypeDef = ProxyStateMachineClone(stateMachineTypeDef);
            if (actualStateMachineTypeDef == null) return;

            var actualMethodDef = ProxyStateMachineSetupMethodClone(rouMethod.MethodDef, stateMachineTypeDef, actualStateMachineTypeDef, Constants.TYPE_AsyncIteratorStateMachineAttribute);
            rouMethod.MethodDef.DeclaringType.Methods.Add(actualMethodDef);

            var actualMoveNextDef = actualStateMachineTypeDef.Methods.Single(m => m.Name == Constants.METHOD_MoveNext);
            actualMoveNextDef.DebugInformation.StateMachineKickOffMethod = actualMethodDef;

            _config.MoArrayThreshold = int.MaxValue;
            var fields = AiteratorResolveFields(rouMethod, stateMachineTypeDef);
            ProxyIteratorSetAbsentFields(rouMethod, stateMachineTypeDef, fields, Constants.GenericPrefix(Constants.TYPE_IAsyncEnumerable), Constants.GenericSuffix(Constants.METHOD_GetAsyncEnumerator));
            ProxyFieldCleanup(stateMachineTypeDef, fields);

            var tStateMachine = stateMachineTypeDef.MakeReference().Simulate<TsAsyncIteratorStateMachine>(this).SetFields(fields);
            var mActualMethod = StateMachineResolveActualMethod<TsAsyncEnumerable>(tStateMachine, actualMethodDef);
            AsyncIteratorBuildMoveNext(rouMethod, tStateMachine, mActualMethod);
        }

        private void AsyncIteratorBuildMoveNext(RouMethod rouMethod, TsAsyncIteratorStateMachine tStateMachine, MethodSimulation<TsAsyncEnumerable> mActualMethod)
        {
            var mMoveNext = tStateMachine.M_MoveNext;
            mMoveNext.Def.Clear();

            if (tStateMachine.F_MoArray == null)
            {
                AsyncIteratorBuildMosMoveNext(rouMethod, tStateMachine, mActualMethod);
            }
            else
            {
                AsyncIteratorBuildMoArrayMoveNext(rouMethod, tStateMachine, mActualMethod);
            }

            DebuggerStepThrough(mMoveNext.Def);
            mMoveNext.Def.Body.InitLocals = true;
            mMoveNext.Def.Body.OptimizePlus(EmptyInstructions);
        }

        private void AsyncIteratorBuildMosMoveNext(RouMethod rouMethod, TsAsyncIteratorStateMachine tStateMachine, MethodSimulation<TsAsyncEnumerable> mActualMethod)
        {
            var mMoveNext = tStateMachine.M_MoveNext;
            var instructions = mMoveNext.Def.Body.Instructions;

            var context = new AiteratorContext(rouMethod);

            var vState = mMoveNext.CreateVariable(_typeIntRef);
            var vHasNext = mMoveNext.CreateVariable(_typeBoolRef);
            var vToken = mMoveNext.CreateVariable(_typeCancellationTokenRef);
            var vMoValueTask = mMoveNext.CreateVariable<TsAwaitable>(_typeValueTaskRef);
            var vAwaiter = mMoveNext.CreateVariable<TsAwaiter>(tStateMachine.F_Awaiter.Value);
            var vMoAwaiter = tStateMachine.F_MoAwaiter == tStateMachine.F_Awaiter ? vAwaiter : mMoveNext.CreateVariable<TsAwaiter>(tStateMachine.F_MoAwaiter.Value);
            var vException = mMoveNext.CreateVariable(_typeExceptionRef);
            var vInnerException = rouMethod.Features.HasIntersection(Feature.OnException | Feature.OnExit) ? mMoveNext.CreateVariable(_typeExceptionRef) : null;
            var vPersistentInnerException = vInnerException == null || context.OnExceptionAllInSync && context.OnExitAllInSync ? null : mMoveNext.CreateVariable(_typeExceptionRef);

            Instruction? outerTryStart = null, outerCatchStart = null, outerCatchEnd = null, innerTryStart = null, innerCatchStart = null, innerCatchEnd = Create(OpCodes.Nop);
            var switches = Create(OpCodes.Switch, []);

            // .var state = _state;
            instructions.Add(vState.Assign(tStateMachine.F_State));
            // .try
            {
                // .if (_disposed) goto END;
                outerTryStart = instructions.AddGetFirst(tStateMachine.F_Disposed.If(anchor => [Create(OpCodes.Leave, context.AnchorEnd)]));
                // .switch (state + 4)
                instructions.Add(vState.Load());
                instructions.Add(Create(OpCodes.Ldc_I4_4));
                instructions.Add(Create(OpCodes.Add));
                instructions.Add(switches);
                {
                    /*
                    // goto case default;
                    instructions.Add(Create(OpCodes.Br, context.AnchorSwitchDefault));
                    // .case 0: (state == -4)
                    {
                        // .goto case MOVE_NEXT;
                        instructions.Add(context.AnchorSwitches.AddGet(context.AnchorSwitchM4.Set(OpCodes.Br, context.AnchorSwitchMoveNext)));
                    }
                    */
                    // *if (state == -4) goto case MOVE_NEXT;
                    context.AnchorSwitches.Add(context.AnchorSwitchMoveNext);
                    // *if (state in [-3, -2, -1]) goto case default;
                    context.AnchorSwitches.Add([context.AnchorSwitchDefault, context.AnchorSwitchDefault, context.AnchorSwitchDefault]);
                    // .default:
                    {
                        instructions.Add(context.AnchorSwitchDefault);
                        // ._state = -1;
                        // ._items = new List<T>();
                        instructions.Add(IteratorInitItems(rouMethod, tStateMachine));
                        // ._mo = new Mo1Attribute(..);
                        instructions.Add(StateMachineInitMos(rouMethod, tStateMachine));
                        // ._context = new MethodContext(..);
                        instructions.Add(StateMachineInitMethodContext(rouMethod, tStateMachine));
                        // ._mo.OnEntryAsync(context).GetAwaiater(); ...
                        instructions.Add(AsyncMosOn(rouMethod, tStateMachine, vMoValueTask, vMoAwaiter, context, Feature.OnEntry, ForceSync.OnEntry, fMo => fMo.Value.M_OnEntry, fMo => fMo.Value.M_OnEntryAsync));
                        // .if (_context.RewriteArguments) { ... }
                        instructions.Add(StateMachineIfRewriteArguments(rouMethod, tStateMachine));
                        // .token = new CancellationToken();
                        instructions.Add(vToken.AssignDefault());
                        // ._iterator = ActualMethod(..).GetAsyncEnumerator();
                        instructions.Add(tStateMachine.F_Iterator.Assign(target => [
                            .. mActualMethod.Call(tStateMachine.M_MoveNext, tStateMachine.F_Parameters),
                            .. mActualMethod.Result.M_GetAsyncEnumerator.Call(tStateMachine.M_MoveNext, vToken)
                        ]));
                    }
                    // .case MOVE_NEXT:
                    instructions.Add(context.AnchorSwitches.AddGet(context.AnchorSwitchMoveNext));
                    {
                        // .try
                        {
                            // .if (state == MOVE_NEXT)
                            innerTryStart = instructions.AddGetFirst(vState.IsEqual(context.State).If((a1, a2) =>
                            {
                                // .awaiter = _awaiter;
                                return vAwaiter.Assign(tStateMachine.F_Awaiter);
                            }, (a1, a2) =>
                            {
                                var vHasNextTask = tStateMachine.M_MoveNext.CreateVariable<TsAwaitable>(tStateMachine.F_Iterator.Value.M_MoveNextAsync.Result);
                                return [
                                    // .awaiter = _iterator.MoveNextAsync().GetAwaiter();
                                    .. vHasNextTask.Assign(target => tStateMachine.F_Iterator.Value.M_MoveNextAsync.Call(tStateMachine.M_MoveNext)),
                                    .. vAwaiter.Assign(target => vHasNextTask.Value.M_GetAwaiter.Call(tStateMachine.M_MoveNext)),
                                    // .if (!awaiter.IsCompleted) { ... }
                                    .. AsyncIsAwaiterCompleted(tStateMachine, tStateMachine.F_Awaiter, vAwaiter, context)
                                ];
                            }));
                            // ._hasNext = awaiter.GetResult();
                            instructions.Add(vHasNext.Assign(target => vAwaiter.Value.M_GetResult.Call(tStateMachine.M_MoveNext)));
                            if (vInnerException != null)
                            {
                                instructions.Add(Create(OpCodes.Leave, innerCatchEnd));
                            }
                        }
                        // .catch (Exception innerException)
                        if (vInnerException != null)
                        {
                            innerCatchStart = instructions.AddGetFirst(vInnerException.Assign(target => []));
                            if (vPersistentInnerException != null)
                            {
                                // persistentInnerException = innerException;
                                instructions.Add(vPersistentInnerException.Assign(vInnerException));
                                instructions.Add(Create(OpCodes.Leave, innerCatchEnd));
                            }
                            else
                            {
                                // ._context.Exception = innerException;
                                instructions.Add(tStateMachine.F_MethodContext.Value.P_Exception.Assign(vInnerException));
                                // ._mo.OnException(_context);
                                instructions.Add(StateMachineSyncMosNo(rouMethod, tStateMachine, Feature.OnException, mo => mo.M_OnException));
                                // ._mo.OnExit(_context);
                                instructions.Add(StateMachineSyncMosNo(rouMethod, tStateMachine, Feature.OnExit, mo => mo.M_OnExit));
                                // throw;
                                instructions.Add(Create(OpCodes.Rethrow));
                            }

                            instructions.Add(innerCatchEnd);
                        }

                        if (vPersistentInnerException != null)
                        {
                            // .if (persistentInnerException != null)
                            instructions.Add(vPersistentInnerException.IsEqual(new Null(this)).IfNot(anchor =>
                            {
                                return [
                                    // ._context.Exception = persistentInnerException;
                                    .. tStateMachine.F_MethodContext.Value.P_Exception.Assign(vPersistentInnerException),
                                    // .goto ON_EXCEPTION;
                                    Create(OpCodes.Br, context.AnchorOnExceptionAsync)
                                ];
                            }));
                        }
                        // .if (hasNext)
                        instructions.Add(vHasNext.If(anchor => [
                            // ._current = _iterator.Current;
                            .. tStateMachine.F_Current.Assign(tStateMachine.F_Iterator.Value.P_Current),
                            // ._items.Add(_current);
                            .. IteratorSaveItem(rouMethod, tStateMachine),
                            // ._state = -4;
                            .. tStateMachine.F_State.Assign(-4),
                            // goto YIELD;
                            Create(OpCodes.Leave, context.AnchorYield)
                        ]));
                        // ._mo.OnSuccessAsync(_context).GetAwaiter(); ...
                        instructions.Add(AsyncMosOn(rouMethod, tStateMachine, vMoValueTask, vMoAwaiter, context, Feature.OnSuccess, ForceSync.OnSuccess, fMo => fMo.Value.M_OnSuccess, fMo => fMo.Value.M_OnSuccessAsync));
                        // .ON_EXIT
                        instructions.Add(context.AnchorOnExitAsync);
                        // ._mo.OnExitAsync(_context).GetAwaiter(); ...
                        instructions.Add(AsyncMosOn(rouMethod, tStateMachine, vMoValueTask, vMoAwaiter, context, Feature.OnExit, ForceSync.OnExit, fMo => fMo.Value.M_OnExit, fMo => fMo.Value.M_OnExitAsync));

                        if (vPersistentInnerException != null)
                        {
                            // .if (persistentInnerException != null) ExceptionDispatchInfo.Capture(persistentInnerException).Throw();
                            instructions.Add(AsyncIteratorThrowPersistentException(vPersistentInnerException));
                        }
                        // goto END;
                        instructions.Add(Create(OpCodes.Leave, context.AnchorEnd));

                        if (vPersistentInnerException != null)
                        {
                            // .ON_EXCEPTION
                            instructions.Add(context.AnchorOnExceptionAsync);
                            // ._mo.OnExceptionAsync(_context).GetAwaiter(); ...
                            instructions.Add(AsyncMosOn(rouMethod, tStateMachine, vMoValueTask, vMoAwaiter, context, Feature.OnException, ForceSync.OnException, fMo => fMo.Value.M_OnException, fMo => fMo.Value.M_OnExceptionAsync));
                            // .goto ON_EXIT;
                            instructions.Add(Create(OpCodes.Br, context.AnchorOnExitAsync));
                        }
                    }
                }
            }
            // .catch (Exception e)
            {
                outerCatchStart = instructions.AddGetFirst(vException.Assign(target => []));
                // ._state = -2;
                instructions.Add(tStateMachine.F_State.Assign(-2));
                // ._builder.Complete();
                instructions.Add(tStateMachine.F_Builder.Value.M_Complete.Call(tStateMachine.M_MoveNext));
                // ._promise.SetException(e);
                instructions.Add(tStateMachine.F_Promise.Value.M_SetException.Call(tStateMachine.M_MoveNext, vException));
                // .return;
                instructions.Add(Create(OpCodes.Leave, context.Ret));
            }
            // .END:
            instructions.Add(context.AnchorEnd);
            outerCatchEnd = context.AnchorEnd;
            // ._state = -2;
            instructions.Add(tStateMachine.F_State.Assign(-2));
            // ._builder.Complete();
            instructions.Add(tStateMachine.F_Builder.Value.M_Complete.Call(tStateMachine.M_MoveNext));
            // ._promise.SetResult(false);
            instructions.Add(tStateMachine.F_Promise.Value.M_SetResult.Call(tStateMachine.M_MoveNext, new Bool(false, this)));
            instructions.Add(Create(OpCodes.Ret));
            // .YIELD:
            instructions.Add(context.AnchorYield);
            // ._promise.SetResult(true);
            instructions.Add(tStateMachine.F_Promise.Value.M_SetResult.Call(tStateMachine.M_MoveNext, new Bool(true, this)));
            instructions.Add(context.Ret);

            switches.Operand = context.AnchorSwitches.ToArray();

            SetTryCatch(mMoveNext.Def, innerTryStart, innerCatchStart, innerCatchEnd);
            SetTryCatch(mMoveNext.Def, outerTryStart, outerCatchStart, outerCatchEnd);
        }

        private void AsyncIteratorBuildMoArrayMoveNext(RouMethod rouMethod, TsAsyncIteratorStateMachine tStateMachine, MethodSimulation<TsAsyncEnumerable> mActualMethod)
        {
            throw new NotImplementedException($"Currently, async methods are not allowed to use array to save the Mos. Contact the author to get support.");
        }

        private IList<Instruction> AsyncIteratorThrowPersistentException(VariableSimulation vPersistentInnerException)
        {
            // .if (persistentInnerException != null)
            return vPersistentInnerException.IsEqual(new Null(this)).IfNot(anchor => [
                // .ExceptionDispatchInfo.Capture(persistentInnerException).Throw();
                .. vPersistentInnerException.Load(),
                Create(OpCodes.Call, _methodExceptionDispatchInfoCaptureRef),
                Create(OpCodes.Callvirt, _methodExceptionDispatchInfoThrowRef)
            ]);
        }

        private AiteratorFields AiteratorResolveFields(RouMethod rouMethod, TypeDefinition stateMachineTypeDef)
        {
            var getEnumeratorMethodDef = stateMachineTypeDef.Methods.Single(x => x.Name.StartsWith(Constants.GenericPrefix(Constants.TYPE_IAsyncEnumerable)) && x.Name.EndsWith(Constants.GenericSuffix(Constants.METHOD_GetAsyncEnumerator)));

            FieldDefinition? moArray = null;
            var mos = new FieldDefinition[0];
            if (rouMethod.Mos.Length >= _config.MoArrayThreshold)
            {
                moArray = new FieldDefinition(Constants.FIELD_RougamoMos, FieldAttributes.Public, _typeIMoArrayRef);
                stateMachineTypeDef.AddUniqueField(moArray);
            }
            else
            {
                mos = new FieldDefinition[rouMethod.Mos.Length];
                for (int i = 0; i < rouMethod.Mos.Length; i++)
                {
                    mos[i] = new FieldDefinition(Constants.FIELD_RougamoMo_Prefix + i, FieldAttributes.Public, rouMethod.Mos[i].MoTypeRef.ImportInto(ModuleDefinition));
                    stateMachineTypeDef.AddUniqueField(mos[i]);
                }
            }
            var methodContext = new FieldDefinition(Constants.FIELD_RougamoContext, FieldAttributes.Public, _typeMethodContextRef);
            var state = stateMachineTypeDef.Fields.Single(x => x.Name == Constants.FIELD_State);
            var current = stateMachineTypeDef.Fields.Single(x => x.Name.EndsWith(Constants.FIELD_Current_Suffix));
            var initialThreadId = stateMachineTypeDef.Fields.Single(x => x.Name == Constants.FIELD_InitialThreadId);
            var disposed = stateMachineTypeDef.Fields.Single(x => x.Name == Constants.FIELD_Disposed);
            var builder = stateMachineTypeDef.Fields.Single(x => x.Name == Constants.FIELD_Builder);
            var promise = stateMachineTypeDef.Fields.Single(x => x.Name == Constants.FIELD_Promise);
            var declaringThis = stateMachineTypeDef.Fields.SingleOrDefault(x => x.FieldType.Resolve() == stateMachineTypeDef.DeclaringType);
            FieldDefinition? recordedReturn = null;
            var transitParameters = StateMachineParameterFields(rouMethod);
            var parameters = IteratorGetPrivateParameterFields(getEnumeratorMethodDef, transitParameters);
            if (_config.RecordingIteratorReturns && (rouMethod.MethodContextOmits & Omit.ReturnValue) == 0)
            {
                var listReturnsRef = new GenericInstanceType(_typeListRef);
                listReturnsRef.GenericArguments.Add(((GenericInstanceType)rouMethod.MethodDef.ReturnType).GenericArguments[0]);
                recordedReturn = new FieldDefinition(Constants.FIELD_IteratorReturnList, FieldAttributes.Public, listReturnsRef);
                stateMachineTypeDef.AddUniqueField(recordedReturn);
            }

            var enumerableTypeRef = rouMethod.MethodDef.ReturnType;
            var enumeratorTypeRef = enumerableTypeRef.GetMethod(Constants.METHOD_GetAsyncEnumerator, false).ReturnType;
            enumeratorTypeRef = stateMachineTypeDef.Import(enumerableTypeRef, enumeratorTypeRef);
            var taskTypeRef = enumeratorTypeRef.GetMethod(Constants.METHOD_MoveNextAsync, false).ReturnType;
            var awaiterTypeRef = taskTypeRef.GetMethod(Constants.METHOD_GetAwaiter, false).ReturnType;
            awaiterTypeRef = enumerableTypeRef.Import(taskTypeRef, awaiterTypeRef);
            var iterator = new FieldDefinition(Constants.FIELD_Iterator, FieldAttributes.Private, enumeratorTypeRef);
            var awaiter = new FieldDefinition(Constants.FIELD_Awaiter, FieldAttributes.Private, awaiterTypeRef);
            var moAwaiter = new FieldDefinition(Constants.FIELD_MoAwaiter, FieldAttributes.Private, _typeValueTaskAwaiterRef);

            stateMachineTypeDef
                .AddUniqueField(methodContext)
                .AddUniqueField(iterator)
                .AddUniqueField(awaiter)
                .AddUniqueField(moAwaiter);

            return new AiteratorFields(stateMachineTypeDef, moArray, mos, methodContext, state, current, initialThreadId, disposed, builder, promise, recordedReturn, declaringThis, iterator, awaiter, moAwaiter, transitParameters, parameters);
        }
    }
}
