using Fody;
using Fody.Simulations;
using Fody.Simulations.Operations;
using Fody.Simulations.PlainValues;
using Fody.Simulations.Types;
using Mono.Cecil.Cil;
using Mono.Cecil;
using Rougamo.Fody.Contexts;
using Rougamo.Fody.Simulations.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using static Mono.Cecil.Cil.Instruction;
using Mono.Cecil.Rocks;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private readonly Dictionary<TypeDefinition, Dictionary<MethodDefinition, object?>> _stateMachineComputeMap = [];

        private void WeavingAsyncTaskMethod(RouMethod rouMethod)
        {
            var methodName = $"$Rougamo_{rouMethod.MethodDef.Name}";
            var typeName = $"<{methodName}>d__{rouMethod.MethodDef.DeclaringType.Methods.Count}";
            var buildSetStateMachine = false;
            MethodDefinition actualMethodDef;
            if (rouMethod.MethodDef.TryResolveStateMachine(Constants.TYPE_AsyncStateMachineAttribute, out var stateMachineTypeDef))
            {
                var actualStateMachineTypeDef = StateMachineClone(stateMachineTypeDef, typeName);
                if (actualStateMachineTypeDef == null) return;
                
                actualMethodDef = StateMachineSetupMethodClone(rouMethod.MethodDef, stateMachineTypeDef, actualStateMachineTypeDef, methodName, Constants.TYPE_AsyncStateMachineAttribute);
                rouMethod.MethodDef.DeclaringType.Methods.Add(actualMethodDef);

                var actualMoveNextDef = actualStateMachineTypeDef.Methods.Single(m => m.Name == Constants.METHOD_MoveNext);
                actualMoveNextDef.DebugInformation.StateMachineKickOffMethod = actualMethodDef;
            }
            else
            {
                actualMethodDef = rouMethod.MethodDef.Clone(methodName);
                rouMethod.MethodDef.DeclaringType.Methods.Add(actualMethodDef);
                actualMethodDef.CustomAttributes.Clear();

                stateMachineTypeDef = AsyncBuildStateMachine(rouMethod.MethodDef);
                AsyncBuildSetupMethod(rouMethod.MethodDef, stateMachineTypeDef);
                buildSetStateMachine = true;
            }

            using var _ = _config.ChangeMoArrayThresholdTemporarily(int.MaxValue);
            var fields = AsyncResolveFields(rouMethod, stateMachineTypeDef);
            AsyncSetAbsentFields(rouMethod, stateMachineTypeDef, fields);
            StateMachineFieldCleanup(stateMachineTypeDef, fields);

            var tStateMachine = stateMachineTypeDef.MakeReference().Simulate<TsAsyncStateMachine>(this).SetFields(fields);
            var mActualMethod = StateMachineResolveActualMethod<TsAwaitable>(tStateMachine, actualMethodDef);
            if (buildSetStateMachine) AsyncBuildSetStateMachine(tStateMachine);
            AsyncBuildMoveNext(rouMethod, tStateMachine, mActualMethod);
        }

        private void AsyncBuildMoveNext(RouMethod rouMethod, TsAsyncStateMachine tStateMachine, MethodSimulation<TsAwaitable> mActualMethod)
        {
            var mMoveNext = tStateMachine.M_MoveNext;
            mMoveNext.Def.Clear();

            if (tStateMachine.F_MoArray == null)
            {
                AsyncBuildMosMoveNext(rouMethod, tStateMachine, mActualMethod);
            }
            else
            {
                AsyncBuildMoArrayMoveNext(rouMethod, tStateMachine, mActualMethod);
            }

            StackTraceHidden(mMoveNext.Def);
            DebuggerStepThrough(mMoveNext.Def);
            mMoveNext.Def.Body.InitLocals = true;
            mMoveNext.Def.Body.OptimizePlus();
        }

        private void AsyncBuildMosMoveNext(RouMethod rouMethod, TsAsyncStateMachine tStateMachine, MethodSimulation<TsAwaitable> mActualMethod)
        {
            var mMoveNext = tStateMachine.M_MoveNext;
            var instructions = mMoveNext.Def.Body.Instructions;

            var context = new AsyncContext(rouMethod);

            VariableSimulation? vExceptionHandled = null;
            var vState = mMoveNext.CreateVariable(_tInt32Ref);
            var vMoValueTask = mMoveNext.CreateVariable<TsAwaitable>(_tValueTaskRef);
            var vAwaiter = mMoveNext.CreateVariable<TsAwaiter>(tStateMachine.F_Awaiter.Value);
            var vMoAwaiter = tStateMachine.F_MoAwaiter == tStateMachine.F_Awaiter ? vAwaiter : mMoveNext.CreateVariable<TsAwaiter>(tStateMachine.F_MoAwaiter.Value);
            var vException = mMoveNext.CreateVariable(_tExceptionRef);
            var vInnerException = rouMethod.Features.HasIntersection(Feature.OnException | Feature.OnExit) ? mMoveNext.CreateVariable(_tExceptionRef) : null;
            var vPersistentInnerException = vInnerException == null || context.OnExceptionAllInSync && context.OnExitAllInSync ? null : mMoveNext.CreateVariable(_tExceptionRef);

            Instruction? outerTryStart = null, outerCatchStart = null, outerCatchEnd = null, innerTryStart = null, innerCatchStart = null, innerCatchEnd = Create(OpCodes.Nop);
            Instruction poolFinallyStart = Create(OpCodes.Nop), poolFinallyEnd = Create(OpCodes.Nop);
            var switches = Create(OpCodes.Switch, []);

            // .var state = _state;
            instructions.Add(vState.Assign(tStateMachine.F_State));

            // .try
            {
                // .try
                {
                    // .switch (state)
                    outerTryStart = instructions.AddGetFirst(vState.Load());
                    instructions.Add(switches);
                    {
                        // ._mo = new Mo1Attribute(..);
                        instructions.Add(StateMachineInitMos(rouMethod, tStateMachine));
                        // ._context = new MethodContext(..);
                        instructions.Add(StateMachineInitMethodContext(rouMethod, tStateMachine));
                        // ._mo.OnEntry(_context); <--> _mo.OnEntryAsync(_context).GetAwaiter()...
                        instructions.Add(AsyncMosOn(rouMethod, tStateMachine, vMoValueTask, vMoAwaiter, vState, context, Feature.OnEntry, ForceSync.OnEntry, fMo => fMo.Value.M_OnEntry, fMo => fMo.Value.M_OnEntryAsync));

                        // .if (_context.ReturnValueReplaced) { ... }
                        instructions.Add(AsyncIfOnEntryReplacedReturn(rouMethod, tStateMachine, context));
                        // .if (_context.RewriteArguments) { ... }
                        instructions.Add(StateMachineIfRewriteArguments(rouMethod, tStateMachine));
                        // .RETRY:
                        instructions.Add(context.AnchorProxyCallCase);
                        context.AnchorSwitches.Add(context.AnchorProxyCallCase);
                        if (vPersistentInnerException != null)
                        {
                            instructions.Add(vPersistentInnerException.AssignDefault());
                        }
                        // .try
                        {
                            // .ActualMethod(...).GetAwaiter()...
                            innerTryStart = instructions.AddGetFirst(AsyncProxyCall(rouMethod, tStateMachine, vState, vAwaiter, mActualMethod, context));
                            // ._result = awaiter.GetResult(); _context.ReturnValue = _result;
                            instructions.Add(AsyncSaveResult(rouMethod, tStateMachine, vAwaiter));
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
                                // // .if (_context.RetryCount > 0) goto RETRY;
                                instructions.Add(AsynCheckRetry(rouMethod, tStateMachine, context, Feature.OnException, vState, true));
                                // .if (exceptionHandled) _result = _context.ReturnValue;
                                instructions.Add(AsyncSaveResultIfExceptionHandled(rouMethod, tStateMachine, ref vExceptionHandled));
                                // ._mo.OnExit(_context);
                                instructions.Add(StateMachineSyncMosNo(rouMethod, tStateMachine, Feature.OnExit, mo => mo.M_OnExit));
                                // .if (exceptionHandled) goto END;
                                instructions.Add(AsyncMosReturnIfExceptionHandled(rouMethod, tStateMachine, context, vExceptionHandled));
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
                        // ._mo.OnSuccessAsync(_context).GetAwaiter(); ...
                        instructions.Add(AsyncMosOn(rouMethod, tStateMachine, vMoValueTask, vMoAwaiter, vState, context, Feature.OnSuccess, ForceSync.OnSuccess, fMo => fMo.Value.M_OnSuccess, fMo => fMo.Value.M_OnSuccessAsync));
                        // .if (_context.RetryCount > 0) continue;
                        instructions.Add(AsynCheckRetry(rouMethod, tStateMachine, context, Feature.OnSuccess, vState, false));
                        // .if (_context.ReturnValueReplaced) _result = _context.ReturnValue;
                        instructions.Add(AsyncSaveResultIfReplaced(rouMethod, tStateMachine));
                        // .ON_EXIT
                        instructions.Add(context.AnchorOnExitAsync);
                        // ._mo.OnExitAsync(_context).GetAwaiter(); ...
                        instructions.Add(AsyncMosOn(rouMethod, tStateMachine, vMoValueTask, vMoAwaiter, vState, context, Feature.OnExit, ForceSync.OnExit, fMo => fMo.Value.M_OnExit, fMo => fMo.Value.M_OnExitAsync));

                        if (vPersistentInnerException != null)
                        {
                            // .if (!exceptionHandled) ExceptionDispatchInfo.Capture(persistentInnerException).Throw();
                            instructions.Add(AsyncThrowIfExceptionNotHandled(rouMethod, tStateMachine, vPersistentInnerException, ref vExceptionHandled));
                        }
                        // goto END;
                        instructions.Add(Create(OpCodes.Leave, context.AnchorSetResult));

                        if (vPersistentInnerException != null)
                        {
                            // .ON_EXCEPTION
                            instructions.Add(context.AnchorOnExceptionAsync);
                            // ._mo.OnExceptionAsync(_context).GetAwaiter(); ...
                            instructions.Add(AsyncMosOn(rouMethod, tStateMachine, vMoValueTask, vMoAwaiter, vState, context, Feature.OnException, ForceSync.OnException, fMo => fMo.Value.M_OnException, fMo => fMo.Value.M_OnExceptionAsync));
                            // .if (_context.RetryCount > 0) continue;
                            instructions.Add(AsynCheckRetry(rouMethod, tStateMachine, context, Feature.OnException, vState, false));
                            // .if (exceptionHandled) _result = _context.ReturnValue;
                            instructions.Add(AsyncSaveResultIfExceptionHandled(rouMethod, tStateMachine, ref vExceptionHandled));
                            // .goto ON_EXIT;
                            instructions.Add(Create(OpCodes.Br, context.AnchorOnExitAsync));
                        }
                    }
                }
                // .finally
                {
                    instructions.Add(poolFinallyStart);

                    // .if (state < 0)
                    instructions.Add(vState.Lt(0).If(_ =>
                    {
                        // .if (_context != null)
                        return tStateMachine.F_MethodContext.Value.IsNull().IfNot(_ => [
                            // .RougamoPool<MethodContext>.Return(_context);
                            .. StateMachineReturnToPool(tStateMachine.F_MethodContext, tStateMachine.M_MoveNext),
                            // ._context = null;
                            .. tStateMachine.F_MethodContext.AssignDefault()
                        ]);
                    }));

                    instructions.Add(Create(OpCodes.Endfinally));
                    instructions.Add(poolFinallyEnd);
                }
                instructions.Add(Create(OpCodes.Leave, context.AnchorSetResult));
            }
            // .catch (Exception exception)
            {
                outerCatchStart = instructions.AddGetFirst(vException.Assign(target => []));
                // ._state = -2;
                instructions.Add(tStateMachine.F_State.Assign(-2));
                // ._builder.SetException(exception);
                instructions.Add(tStateMachine.F_Builder.Value.M_SetException.Call(mMoveNext, vException));
                // .return;
                instructions.Add(Create(OpCodes.Leave, context.Ret));
            }
            // .END
            instructions.Add(context.AnchorSetResult);
            outerCatchEnd = context.AnchorSetResult;
            // ._state = -2;
            instructions.Add(tStateMachine.F_State.Assign(-2));
            // ._builder.SetResult(_result);
            IParameterSimulation[] setResultArgs = tStateMachine.F_Result == null ? [] : [tStateMachine.F_Result];
            instructions.Add(tStateMachine.F_Builder.Value.M_SetResult.Call(mMoveNext, setResultArgs));
            // .return;
            instructions.Add(context.Ret);

            switches.Operand = context.AnchorSwitches.ToArray();

            SetTryCatch(mMoveNext.Def, innerTryStart, innerCatchStart, innerCatchEnd);
            SetTryFinally(mMoveNext.Def, outerTryStart, poolFinallyStart, poolFinallyEnd);
            SetTryCatch(mMoveNext.Def, outerTryStart, outerCatchStart, outerCatchEnd);
        }

        private void AsyncBuildMoArrayMoveNext(RouMethod rouMethod, TsAsyncStateMachine tStateMachine, MethodSimulation<TsAwaitable> mActualMethod)
        {
            throw new NotImplementedException($"Currently, async methods are not allowed to use array to save the Mos. Contact the author to get support.");
        }

        private MethodSimulation<T> StateMachineResolveActualMethod<T>(TsStateMachine tStateMachine, MethodDefinition actualMethodDef) where T : TypeSimulation
        {
            var tDeclaring = StateMachineResolveDeclaringType(tStateMachine);
            var mActualMethod = actualMethodDef.Simulate<T>(tDeclaring);

            if (actualMethodDef.HasGenericParameters)
            {
                var generics = tStateMachine.Def.GetSelfGenerics();
                var tGenerics = mActualMethod.Def.GenericParameters.Select(x => generics.Single(y => y.Name == x.Name).Simulate(this)).ToArray();
                mActualMethod = mActualMethod.MakeGenericMethod(tGenerics);
            }

            return mActualMethod;
        }

        private TypeSimulation StateMachineResolveDeclaringType(TsStateMachine tStateMachine)
        {
            if (tStateMachine.F_DeclaringThis != null) return tStateMachine.F_DeclaringThis.Value;

            var stateMachineRef = tStateMachine.M_MoveNext.DeclaringType.Ref;
            var declaringTypeDef = stateMachineRef.DeclaringType.Resolve();
            if (stateMachineRef is not GenericInstanceType || !declaringTypeDef.HasGenericParameters) return new TsWeavingTarget(declaringTypeDef, null, this);

            var declaringTypeRef = declaringTypeDef.ReplaceGenericArgs(stateMachineRef.GetGenericMap());

            return new TsWeavingTarget(declaringTypeRef, null, this);
        }

        private IList<Instruction> StateMachineInitMos(RouMethod rouMethod, TsStateMachine tStateMachine)
        {
            var instructions = new List<Instruction>();

            for (int i = 0; i < rouMethod.Mos.Length; i++)
            {
                var mo = rouMethod.Mos[i];
                var fMo = tStateMachine.F_Mos![i];
                instructions.Add(fMo.Assign(target => fMo.Value.New(tStateMachine.M_MoveNext, mo)));
            }

            return instructions;
        }

        private IList<Instruction> StateMachineInitMethodContext(RouMethod rouMethod, TsStateMachine tStateMachine)
        {
            var instructions = new List<Instruction>();

            // ._context = RougamoPool<MethodContext>.Get();
            instructions.AddRange(StateMachineAssignByPool(tStateMachine.F_MethodContext, tStateMachine.M_MoveNext));
            // ._context.Mos = new Mo[] { ... };
            if (!rouMethod.MethodContextOmits.Contains(Omit.Mos))
            {
                var tMoArray = _tIMoArrayRef.Simulate<TsArray>(this);
                instructions.AddRange(tStateMachine.F_MethodContext.Value.P_Mos.Assign(tMoArray.NewAsPlainValue(tStateMachine.F_Mos!)));
            }
            // ._context.Target = <>4__this;
            if (tStateMachine.F_DeclaringThis != null)
            {
                instructions.AddRange(tStateMachine.F_MethodContext.Value.P_Target.Assign(tStateMachine.F_DeclaringThis.Typed(_simulations.Object)));
            }
            // ._context.TargetType = typeof(TARGET_TYPE);
            instructions.AddRange(tStateMachine.F_MethodContext.Value.P_TargetType.Assign(new SystemType(rouMethod.MethodDef.DeclaringType, this)));
            // ._context.Method = methodof(TARGET_METHOD);
            instructions.AddRange(tStateMachine.F_MethodContext.Value.P_Method.Assign(new SystemMethodBase(rouMethod.MethodDef, this)));
            // ._context.Arguments = new object[] { ... };
            if (!rouMethod.MethodContextOmits.Contains(Omit.Arguments))
            {
                var tObjectArray = _tObjectArrayRef.Simulate<TsArray>(this);
                instructions.AddRange(tStateMachine.F_MethodContext.Value.P_Arguments.Assign(tObjectArray.NewAsPlainValue(tStateMachine.F_Parameters)));
            }
            
            return instructions;
        }

        private IList<Instruction> AsyncAwaitMoAwaiterIfNeed(RouMethod rouMethod, IAsyncStateMachine tStateMachine, VariableSimulation<TsAwaiter> vMoAwaiter, VariableSimulation vState, IAsyncContext context)
        {
            if (!context.AwaitFirst) return [];

            // .moAwaiter = _moAwaiter;
            var assignAwaiterInsts = vMoAwaiter.Assign(tStateMachine.F_MoAwaiter);

            Instruction[] instructions = [
                .. assignAwaiterInsts,
                // .state = -1;
                .. vState.Assign(-1),
                context.AnchorStateReady,
                // .moAwaiter.GetResult();
                .. vMoAwaiter.Value.M_GetResult.Call(tStateMachine.M_MoveNext)
            ];

            context.AnchorSwitches.Add(assignAwaiterInsts.First());
            context.AwaitFirst = false;

            return instructions;
        }

        private IList<Instruction>? AsyncIfOnEntryReplacedReturn(RouMethod rouMethod, TsAsyncStateMachine tStateMachine, AsyncContext context)
        {
            if (!rouMethod.Features.Contains(Feature.EntryReplace) || rouMethod.MethodContextOmits.Contains(Omit.ReturnValue)) return null;

            // .if (_context.ReturnValueReplaced)
            return tStateMachine.F_MethodContext.Value.P_ReturnValueReplaced.If(anchor =>
            {
                var instructions = new List<Instruction>();

                if (tStateMachine.F_Result != null)
                {
                    // ._result = _context.ReturnValue;
                    instructions.Add(tStateMachine.F_Result.Assign(tStateMachine.F_MethodContext.Value.P_ReturnValue));
                }
                if (context.OnExitAllInSync)
                {
                    // ._mo.OnExit(_contexxt);
                    instructions.Add(StateMachineSyncMosNo(rouMethod, tStateMachine, Feature.OnExit, mo => mo.M_OnExit));
                    // .goto END;
                    instructions.Add(Create(OpCodes.Leave, context.AnchorSetResult));
                }
                else
                {
                    // goto ON_EXIT;
                    instructions.Add(Create(OpCodes.Br, context.AnchorOnExitAsync));
                }

                return instructions;
            });
        }

        private IList<Instruction> StateMachineIfRewriteArguments(RouMethod rouMethod, TsStateMachine tStateMachine)
        {
            if (rouMethod.MethodDef.Parameters.Count == 0 || !rouMethod.Features.Contains(Feature.RewriteArgs) || rouMethod.MethodContextOmits.Contains(Omit.Arguments)) return [];

            return tStateMachine.F_MethodContext.Value.P_RewriteArguments.If(anchor =>
            {
                var instructions = new List<Instruction>();
                var vArguments = tStateMachine.M_MoveNext.CreateVariable<TsArray>(_tObjectArrayRef);

                // .var args = _context.Arguments;
                instructions.Add(vArguments.Assign(tStateMachine.F_MethodContext.Value.P_Arguments));
                for (var i = 0; i < tStateMachine.F_Parameters.Length; i++)
                {
                    // .if (args[i] == null)
                    instructions.Add(vArguments.Value[i].IsNull().If((a1, a2) =>
                    {
                        // ._parameters[i] = default;
                        return tStateMachine.F_Parameters[i].AssignDefault();
                    }, (a1, a2) =>
                    {// .else (args[i] != null)
                        // ._parameters[i] = args[i];
                        return tStateMachine.F_Parameters[i].Assign(vArguments.Value[i]);
                    }));
                }

                return instructions;
            });
        }

        private IList<Instruction> AsyncProxyCall(RouMethod rouMethod, TsAsyncStateMachine tStateMachine, VariableSimulation vState, VariableSimulation<TsAwaiter> vAwaiter, MethodSimulation<TsAwaitable> mActualMethod, AsyncContext context)
        {
            // .if (state == STATE)
            return vState.IsEqual(new Int32Value(context.State, this)).If((a1, a2) =>
            {
                return [
                    // .awaiter = _awaiter;
                    .. vAwaiter.Assign(tStateMachine.F_Awaiter),
                    // .state = -1;
                    .. vState.Assign(-1)
                ];
            }, (a1, a2) =>
            {// .else (state != STATE)
                Instruction[] callAssignAwaiter;
                if (mActualMethod.Result.IsValueType)
                {
                    var vResultTask = tStateMachine.M_MoveNext.CreateVariable<TsAwaitable>(mActualMethod.Result);
                    callAssignAwaiter = [
                        // .resultTask = ActualMethod(...);
                        .. vResultTask.Assign(target => mActualMethod.Call(tStateMachine.M_MoveNext, tStateMachine.F_Parameters)),
                        // .awaiter = resultTask.GetAwaiter();
                        .. vAwaiter.Assign(target => vResultTask.Value.M_GetAwaiter.Call(tStateMachine.M_MoveNext))
                    ];
                }
                else
                {
                    callAssignAwaiter = [
                        // .awaiter = ActualMethod(...).GetAwaiter();
                        .. mActualMethod.Call(tStateMachine.M_MoveNext, tStateMachine.F_Parameters),
                        .. vAwaiter.Assign(target => mActualMethod.Result.M_GetAwaiter.Call(tStateMachine.M_MoveNext)),
                    ];
                }
                return [
                    .. callAssignAwaiter,
                    // .if (!awaiter.IsCompleted) { ... }
                    .. AsyncIsAwaiterCompleted(tStateMachine, tStateMachine.F_Awaiter, vAwaiter, vState, context)
                ];
            });
        }

        private IList<Instruction> AsyncIsAwaiterCompleted(IAsyncStateMachine tStateMachine, FieldSimulation<TsAwaiter> fAwaiter, VariableSimulation<TsAwaiter> vAwaiter, VariableSimulation vState, IAsyncContext context)
        {
            // .if (!awaiter.IsCompleted)
            return vAwaiter.Value.P_IsCompleted.IfNot(anchor => [
                // .state = STATE;
                .. vState.Assign(context.State),
                // ._state = STATE++;
                .. tStateMachine.F_State.Assign(context.State++),
                // ._awaiter = awaiter;
                .. fAwaiter.Assign(vAwaiter),
                // ._builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
                .. tStateMachine.FV_Builder.M_AwaitUnsafeOnCompleted.Call(tStateMachine.M_MoveNext, vAwaiter, tStateMachine),
                // .return;
                Create(OpCodes.Leave, context.Ret)
            ]);
        }

        private IList<Instruction> AsyncSaveResult(RouMethod rouMethod, TsAsyncStateMachine tStateMachine, VariableSimulation<TsAwaiter> vAwaiter)
        {
            IList<Instruction> getResultInsts;
            if (tStateMachine.F_Result == null)
            {
                // .awaiter.GetResult();
                getResultInsts = vAwaiter.Value.M_GetResult.Call(tStateMachine.M_MoveNext);
            }
            else
            {
                // ._result = awaiter.GetResult();
                getResultInsts = tStateMachine.F_Result.Assign(target => vAwaiter.Value.M_GetResult.Call(tStateMachine.M_MoveNext));
            }
            if (tStateMachine.F_Result == null || !rouMethod.Features.HasIntersection(Feature.OnSuccess | Feature.OnExit) || rouMethod.MethodContextOmits.Contains(Omit.ReturnValue))
            {
                return getResultInsts;
            }

            return [
                // ._result = awaiter.GetResult();
                .. getResultInsts,
                // ._context.ReturnValue  = _result;
                .. tStateMachine.F_MethodContext.Value.P_ReturnValue.Assign(tStateMachine.F_Result)
            ];
        }

        private IList<Instruction> AsynCheckRetry(RouMethod rouMethod, TsAsyncStateMachine tStateMachine, AsyncContext context, Feature action, VariableSimulation vState, bool insideBlock)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            if (!rouMethod.Features.Contains(Feature.RetryAny | action)) return [];
#pragma warning restore CS0618 // Type or member is obsolete

            // .if (_context.RetryCount > 0)
            return tStateMachine.F_MethodContext.Value.P_RetryCount.Gt(0).If(anchor =>
            {
                return [
                    // .state = -1;
                    .. vState.Assign(-1),
                    // .goto RETRY;
                    Create(insideBlock ? OpCodes.Leave : OpCodes.Br, context.AnchorProxyCallCase)
                ];
            });
        }

        private IList<Instruction> AsyncSaveResultIfExceptionHandled(RouMethod rouMethod, TsAsyncStateMachine tStateMachine, ref VariableSimulation? vExceptionHandled)
        {
            if (!rouMethod.Features.Contains(Feature.ExceptionHandle) || rouMethod.MethodContextOmits.Contains(Omit.ReturnValue)) return [];

            var instructions = new List<Instruction>();

            vExceptionHandled ??= tStateMachine.M_MoveNext.CreateVariable(_tBooleanRef);

            // .exceptionHandled = _context.ExceptionHandled;
            instructions.Add(vExceptionHandled.Assign(tStateMachine.F_MethodContext.Value.P_ExceptionHandled));

            if (tStateMachine.F_Result != null)
            {
                // .if (exceptionHandled)
                instructions.Add(vExceptionHandled.If(anchor =>
                {
                    // ._result = _context.ReturnValue;
                    return tStateMachine.F_Result.Assign(tStateMachine.F_MethodContext.Value.P_ReturnValue);
                }));
            }

            return instructions;
        }

        private IList<Instruction> AsyncMosReturnIfExceptionHandled(RouMethod rouMethod, TsAsyncStateMachine tStateMachine, AsyncContext context, VariableSimulation? vExceptionHandled)
        {
            if (vExceptionHandled == null) return [];

            // .if (exceptionHandled)
            return vExceptionHandled.If(anchor =>
            {
                // .goto END;
                return [Create(OpCodes.Leave, context.AnchorSetResult)];
            });
        }

        private IList<Instruction> AsyncThrowIfExceptionNotHandled(RouMethod rouMethod, TsAsyncStateMachine tStateMachine, VariableSimulation vPersistentInnerException, ref VariableSimulation? vExceptionHandled)
        {
            // .ExceptionDispatchInfo.Capture(persistentInnerException).Throw();
            IList<Instruction> instructions = [
                .. vPersistentInnerException.Load(),
                Create(OpCodes.Call, _mExceptionDispatchInfoCaptureRef),
                Create(OpCodes.Callvirt, _mExceptionDispatchInfoThrowRef)
            ];

            if (rouMethod.Features.Contains(Feature.ExceptionHandle) && !rouMethod.MethodContextOmits.Contains(Omit.ReturnValue))
            {
                vExceptionHandled = tStateMachine.M_MoveNext.CreateVariable(_tBooleanRef);
                // .if (exceptionHandled) { ... }
                instructions = vExceptionHandled.IfNot(anchor => instructions);
            }

            // .if (persistentInnerException != null) { ... }
            return vPersistentInnerException.IsEqual(new Null(this)).IfNot(anchor => instructions);
        }

        private IList<Instruction> AsyncSaveResultIfReplaced(RouMethod rouMethod, TsAsyncStateMachine tStateMachine)
        {
            if (tStateMachine.F_Result == null || !rouMethod.Features.Contains(Feature.SuccessReplace) || rouMethod.MethodContextOmits.Contains(Omit.ReturnValue)) return [];

            // .if (_context.ReturnValueReplaced)
            return tStateMachine.F_MethodContext.Value.P_ReturnValueReplaced.If(anchor =>
            {
                // ._result = _context.ReturnValue;
                return tStateMachine.F_Result.Assign(tStateMachine.F_MethodContext.Value.P_ReturnValue);
            });
        }

        private IList<Instruction> AsyncMosOn(RouMethod rouMethod, IAsyncStateMachine tStateMachine, VariableSimulation<TsAwaitable> vMoValueTask, VariableSimulation<TsAwaiter> vMoAwaiter, VariableSimulation vState, IAsyncContext context, Feature feature, ForceSync forceSync, Func<FieldSimulation<TsMo>, MethodSimulation> syncMethodFactory, Func<FieldSimulation<TsMo>, MethodSimulation<TsAwaitable>> asyncMethodFactory)
        {
            if (!rouMethod.Features.Contains(feature)) return [];

            var reverseCall = feature != Feature.OnEntry && _config.ReverseCallNonEntry;

            var instructions = new List<Instruction>();

            for (var i = 0; i < rouMethod.Mos.Length; i++)
            {
                var j = reverseCall ? rouMethod.Mos.Length - i - 1 : i;

                var mo = rouMethod.Mos[j];
                if (!mo.Features.Contains(feature)) continue;

                instructions.Add(AsyncAwaitMoAwaiterIfNeed(rouMethod, tStateMachine, vMoAwaiter, vState, context));

                var fMo = tStateMachine.F_Mos![j];
                if (mo.ForceSync.Contains(forceSync))
                {
                    // ._mo.OnXxx(_context);
                    instructions.Add(syncMethodFactory(fMo).Call(tStateMachine.M_MoveNext, tStateMachine.F_MethodContext));
                }
                else
                {
                    // .moValueTask = _mo.OnXxxAsync(_context);
                    var method = asyncMethodFactory(fMo);
                    instructions.Add(vMoValueTask.Assign(target => method.Call(tStateMachine.M_MoveNext, tStateMachine.F_MethodContext)));
                    // .moAwaiter = moValueTask.GetAwaiter();
                    instructions.Add(vMoAwaiter.Assign(target => vMoValueTask.Value.M_GetAwaiter.Call(tStateMachine.M_MoveNext)));
                    // .if (!moAwaiter.IsCompleted) { ... }
                    instructions.Add(AsyncIsAwaiterCompleted(tStateMachine, tStateMachine.F_MoAwaiter, vMoAwaiter, vState, context));
                    // goto NEXT_STATE;
                    instructions.Add(Create(OpCodes.Br, context.GetNextStateReadyAnchor()));
                    context.AwaitFirst = true;
                }
            }

            instructions.Add(AsyncAwaitMoAwaiterIfNeed(rouMethod, tStateMachine, vMoAwaiter, vState, context));

            return instructions;
        }

        private IList<Instruction> StateMachineSyncMosNo(RouMethod rouMethod, TsStateMachine tStateMachine, Feature feature, Func<TsMo, MethodSimulation> methodFactory)
        {
            return SyncMosOn(rouMethod, tStateMachine.M_MoveNext, tStateMachine.F_MethodContext, tStateMachine.F_Mos?.Select(x => x.Value).ToArray(), tStateMachine.F_MoArray?.Value, feature, methodFactory);
        }

        private IList<Instruction> StateMachineAssignByPool(FieldSimulation field, MethodSimulation executionMethod)
        {
            var tPool = _tPoolRef.MakeGenericInstanceType(field.Type).Simulate(this);
            var mGet = _mPoolGetRef.Simulate(tPool);
            return field.Assign(target => mGet.Call(executionMethod));
        }

        private IList<Instruction> StateMachineReturnToPool(FieldSimulation field, MethodSimulation executionMethod)
        {
            var tPool = _tPoolRef.MakeGenericInstanceType(field.Type).Simulate(this);
            var mReturn = _mPoolReturnRef.Simulate(tPool);
            return mReturn.Call(executionMethod, field);
        }

        private TypeDefinition AsyncBuildStateMachine(MethodDefinition methodDef)
        {
            var typeDef = methodDef.DeclaringType;
            if (!_stateMachineComputeMap.TryGetValue(typeDef, out var map))
            {
                map = [];
                _stateMachineComputeMap[typeDef] = map;
            }
            if (map.ContainsKey(methodDef)) throw new FodyWeavingException($"Try build a duplicate state machine type for {methodDef}", methodDef);
            map[methodDef] = null;

            var attribute = TypeAttributes.NestedPrivate | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit;
            var baseTypeRef = _debugMode ? _tObjectRef : _tValueTypeRef;
            // #77. 这里的类名由于本身不需要调试所以暂时不按官方的命名来，如果后续需要，将类名改为 $"<{methodDef.Name}>d__{methodDef.DeclaringType.Methods.Count - 1}" 即可
            var stateMachineTypeDef = new TypeDefinition(methodDef.DeclaringType.Namespace, $"<{methodDef.Name}>r__{map.Count}", attribute, baseTypeRef);
            stateMachineTypeDef.Interfaces.Add(new(_tIAsyncStateMachineRef));
            stateMachineTypeDef.DeclaringType = methodDef.DeclaringType;
            methodDef.DeclaringType.NestedTypes.Add(stateMachineTypeDef);
            stateMachineTypeDef.CustomAttributes.Add(new(_ctorCompilerGeneratedAttributeRef));

            if (methodDef.DeclaringType.HasGenericParameters)
            {
                stateMachineTypeDef.GenericParameters.Add(methodDef.DeclaringType.GenericParameters.Select(x => x.Clone(stateMachineTypeDef)).ToArray());
            }
            if (methodDef.HasGenericParameters)
            {
                stateMachineTypeDef.GenericParameters.Add(methodDef.GenericParameters.Select(x => x.Clone(stateMachineTypeDef)).ToArray());
            }

            var builderTypeRef = AsyncGetBuilderType(methodDef, stateMachineTypeDef);
            var fBuilder = new FieldDefinition(Constants.FIELD_Builder, FieldAttributes.Public, builderTypeRef);
            var fState = new FieldDefinition(Constants.FIELD_State, FieldAttributes.Public, _tInt32Ref);
            stateMachineTypeDef.AddUniqueField(fBuilder).AddUniqueField(fState);

            if (_debugMode) stateMachineTypeDef.Methods.Add(AsyncBuildStateMachineCtor());
            stateMachineTypeDef.Methods.Add(AsyncBuildStateMachineMethodSetStateMachine());
            stateMachineTypeDef.Methods.Add(AsyncBuildStateMachineMethodMoveNext(methodDef));

            return stateMachineTypeDef;
        }

        private MethodDefinition AsyncBuildStateMachineCtor()
        {
            var attribute = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var methodDef = new MethodDefinition(Constants.METHOD_Ctor, attribute, _tVoidRef);
            methodDef.HasThis = true;
            methodDef.ExplicitThis = false;
            methodDef.CallingConvention = MethodCallingConvention.Default;

            methodDef.Body.InitLocals = true;
            methodDef.Body.Instructions.Add([
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Call, _ctorObjectRef),
                Create(OpCodes.Ret)
            ]);

            return methodDef;
        }

        private MethodDefinition AsyncBuildStateMachineMethodMoveNext(MethodDefinition kickOffMethodDef)
        {
            var attribute = MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
            var methodDef = new MethodDefinition(Constants.METHOD_MoveNext, attribute, _tVoidRef);
            methodDef.HasThis = true;
            methodDef.ExplicitThis = false;
            methodDef.CallingConvention = MethodCallingConvention.Default;
            methodDef.Overrides.Add(_mIAsyncStateMachineMoveNextRef);
            methodDef.DebugInformation.StateMachineKickOffMethod = kickOffMethodDef;
            methodDef.Body.InitLocals = true;

            return methodDef;
        }

        private MethodDefinition AsyncBuildStateMachineMethodSetStateMachine()
        {
            var attribute = MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
            var methodDef = new MethodDefinition(Constants.METHOD_SetStateMachine, attribute, _tVoidRef);
            methodDef.HasThis = true;
            methodDef.ExplicitThis = false;
            methodDef.CallingConvention = MethodCallingConvention.Default;
            methodDef.CustomAttributes.Add(new(_ctorDebuggerHiddenAttributeRef));
            methodDef.Overrides.Add(_mIAsyncStateMachineSetStateMachineRef);
            methodDef.Parameters.Add(new("stateMachine", ParameterAttributes.None, _tIAsyncStateMachineRef));
            methodDef.Body.InitLocals = true;

            return methodDef;
        }

        private void AsyncBuildSetStateMachine(TsAsyncStateMachine tStateMachine)
        {
            var instructions = tStateMachine.M_SetStateMachine.Def.Body.Instructions;

            if (!_debugMode)
            {
                var args = tStateMachine.M_SetStateMachine.Def.Parameters.Select(x => x.Simulate(this)).ToArray();
                // ._builder.SetStateMachine(stateMachine);
                instructions.Add(tStateMachine.F_Builder.Value.M_SetStateMachine.Call(tStateMachine.M_SetStateMachine, args));
            }
            instructions.Add(Create(OpCodes.Ret));
        }

        private void AsyncBuildSetupMethod(MethodDefinition methodDef, TypeDefinition stateMachineTypeDef)
        {
            methodDef.Clear();
            methodDef.Body.InitLocals = true;
            var stateMachineAttribute = new CustomAttribute(_ctorAsyncStateMachineAttributeRef);
            stateMachineAttribute.ConstructorArguments.Add(new(_tTypeRef, stateMachineTypeDef));
            methodDef.CustomAttributes.Add(stateMachineAttribute);

            var stateMachineTypeRef = stateMachineTypeDef.ReplaceGenericArgs(methodDef.GetGenericMap());
            var builderTypeRef = AsyncGetBuilderType(methodDef, null);

            var instructions = methodDef.Body.Instructions;

            var method = methodDef.Simulate<TsAwaitable>(methodDef.DeclaringType.Simulate(this));

            var vStateMachine = method.CreateVariable<TsAsyncStateMachine>(stateMachineTypeRef);
            var mCreate = builderTypeRef.GetMethod(true, x => x.IsStatic && x.Name == Constants.METHOD_Create)!.Simulate(builderTypeRef.Simulate(this));

            // .stateMachine = new();
            if (!vStateMachine.VariableDef.VariableType.IsValueType)
            {
                instructions.Add(vStateMachine.AssignNew());
            }
            // .stateMachine._builder = AsyncTaskMethodBuilder.Create();
            instructions.Add(vStateMachine.Value.F_Builder.Assign(target => mCreate.Call(method)));
            // .stateMachine._state = -1;
            instructions.Add(vStateMachine.Value.F_State.Assign(-1));
            // .stateMachine._builder.Start(ref stateMachine);
            instructions.Add(vStateMachine.Value.F_Builder.Value.M_Start.Call(method, vStateMachine));
            // .return stateMachine._builder.Task;
            instructions.Add(vStateMachine.Value.F_Builder.Value.P_Task.Load());
            instructions.Add(Create(OpCodes.Ret));
        }

        private TypeReference AsyncGetBuilderType(MethodDefinition methodDef, TypeDefinition? stateMachineDef)
        {
            var returnTypeRef = methodDef.ReturnType;
            if (returnTypeRef.IsTask()) return _tAsyncTaskMethodBuilderRef;
            if (returnTypeRef.IsGenericTask()) return GetGenericBuilderType(stateMachineDef, returnTypeRef, _tAsyncTaskMethodBuilder1Ref);

            var builderAttribute = methodDef.ReturnType.Resolve().CustomAttributes.Single(x => x.Is(Constants.TYPE_AsyncMethodBuilderAttribute));
            var builderTypeRef = (TypeReference)builderAttribute.ConstructorArguments[0].Value;
            if (builderTypeRef is GenericInstanceType) return this.Import(builderTypeRef);
            if (builderTypeRef is not TypeDefinition typeDef) throw new FodyWeavingException($"Unrecognized async method builder type {builderTypeRef}", methodDef);
            if (!typeDef.HasGenericParameters) return this.Import(typeDef);

            return GetGenericBuilderType(stateMachineDef, returnTypeRef, this.Import(typeDef));

            TypeReference GetGenericBuilderType(TypeDefinition? stateMachineDef, TypeReference returnTypeRef, TypeReference builderTypeRef)
            {
                var elementTypeRef = ((GenericInstanceType)returnTypeRef).GenericArguments[0];
                if (stateMachineDef == null)
                {
                    elementTypeRef = this.Import(elementTypeRef);
                }
                else
                {
                    elementTypeRef = this.Import(elementTypeRef.ReplaceGenericArgs(stateMachineDef.GetGenericMap()));
                }

                var git = new GenericInstanceType(builderTypeRef);
                git.GenericArguments.Add(elementTypeRef);
                return git;
            }
        }

        private AsyncFields AsyncResolveFields(RouMethod rouMethod, TypeDefinition stateMachineTypeDef)
        {
            var asyncableTypeRef = rouMethod.MethodDef.ReturnType;
            var awaiterTypeRef = asyncableTypeRef.GetMethod(Constants.METHOD_GetAwaiter, false).ReturnType;
            awaiterTypeRef = stateMachineTypeDef.Import(asyncableTypeRef, awaiterTypeRef);
            var resultTypeRef = awaiterTypeRef.GetMethod(Constants.METHOD_GetResult, false).ReturnType;
            resultTypeRef = awaiterTypeRef.Import(resultTypeRef);

            FieldDefinition? moArray = null;
            var mos = new FieldDefinition[0];
            if (rouMethod.Mos.Length >= _config.MoArrayThreshold)
            {
                moArray = new FieldDefinition(Constants.FIELD_RougamoMos, FieldAttributes.Public, _tIMoArrayRef);
                stateMachineTypeDef.AddUniqueField(moArray);
            }
            else
            {
                mos = new FieldDefinition[rouMethod.Mos.Length];
                for (int i = 0; i < rouMethod.Mos.Length; i++)
                {
                    mos[i] = new FieldDefinition(Constants.FIELD_RougamoMo_Prefix + i, FieldAttributes.Public, this.Import(rouMethod.Mos[i].MoTypeRef));
                    stateMachineTypeDef.AddUniqueField(mos[i]);
                }
            }
            var methodContext = new FieldDefinition(Constants.FIELD_RougamoContext, FieldAttributes.Public, _tMethodContextRef);
            var awaiter = new FieldDefinition(Constants.FIELD_Awaiter, FieldAttributes.Private, this.Import(awaiterTypeRef));
            var moAwaiter = asyncableTypeRef.Is(Constants.TYPE_ValueTask) ? null : new FieldDefinition(Constants.FIELD_MoAwaiter, FieldAttributes.Private, _tValueTaskAwaiterRef);
            var result = resultTypeRef.Is(Constants.TYPE_Void) ? null : new FieldDefinition(Constants.FIELD_Result, FieldAttributes.Private, this.Import(resultTypeRef));
            var builder = stateMachineTypeDef.Fields.Single(x => x.Name == Constants.FIELD_Builder);
            var state = stateMachineTypeDef.Fields.Single(x => x.Name == Constants.FIELD_State);
            var declaringThis = stateMachineTypeDef.Fields.SingleOrDefault(x => x.FieldType.Resolve() == stateMachineTypeDef.DeclaringType);
            var parameters = StateMachineParameterFields(rouMethod);

            stateMachineTypeDef
                .AddUniqueField(methodContext)
                .AddUniqueField(awaiter)
                .AddUniqueField(moAwaiter)
                .AddUniqueField(result);

            return new AsyncFields(moArray, mos, methodContext, state, builder, declaringThis, awaiter, moAwaiter, result, parameters);
        }

        private FieldDefinition?[] StateMachineParameterFields(RouMethod rouMethod)
        {
            if (rouMethod.MethodDef.Parameters.Count == 0) return new FieldDefinition?[0];

            var isStaticMethod = rouMethod.MethodDef.IsStatic;
            var parameterFieldDefs = new FieldDefinition?[rouMethod.MethodDef.Parameters.Count];
            foreach (var instruction in rouMethod.MethodDef.Body.Instructions)
            {
                var index = -1;
                var code = instruction.OpCode.Code;
                if (isStaticMethod && code == Code.Ldarg_0 || !isStaticMethod && code == Code.Ldarg_1)
                {
                    index = 0;
                }
                else if (isStaticMethod && code == Code.Ldarg_1 || !isStaticMethod && code == Code.Ldarg_2)
                {
                    index = 1;
                }
                else if (isStaticMethod && code == Code.Ldarg_2 || !isStaticMethod && code == Code.Ldarg_3)
                {
                    index = 2;
                }
                else if (isStaticMethod && code == Code.Ldarg_3)
                {
                    index = 3;
                }
                else if (code == Code.Ldarg || code == Code.Ldarg_S)
                {
                    index = rouMethod.MethodDef.Parameters.IndexOf((ParameterDefinition)instruction.Operand);
                    if (index == -1) throw new FodyWeavingException($"{rouMethod.MethodDef.FullName} can not locate the index of parameter {((ParameterDefinition)instruction.Operand).Name}");
                }
                if (index != -1)
                {
                    parameterFieldDefs[index] = ((FieldReference)instruction.Next.Operand).Resolve();
                }
            }

            return parameterFieldDefs;
        }

        private TypeDefinition? StateMachineClone(TypeDefinition stateMachineTypeDef, string typeName)
        {
            if (stateMachineTypeDef.DeclaringType.NestedTypes.Any(x => x.Name == typeName)) return null;

            var actualTypeDef = stateMachineTypeDef.Clone(typeName);
            actualTypeDef.DeclaringType.NestedTypes.Add(actualTypeDef);

            return actualTypeDef;
        }

        private MethodDefinition StateMachineSetupMethodClone(MethodDefinition methodDef, TypeDefinition stateMachineTypeDef, TypeDefinition clonedStateMachineTypeDef, string methodName, string stateMachineAttributeTypeName)
        {
            var clonedMethodDef = methodDef.Clone(methodName);

            StateMachineAttributeClone(clonedMethodDef, clonedStateMachineTypeDef, stateMachineAttributeTypeName);

            var genericMap = methodDef.DeclaringType.GenericParameters.ToDictionary(x => x.Name, x => (TypeReference)x);
            genericMap.AddRange(clonedMethodDef.GenericParameters.ToDictionary(x => x.Name, x => (TypeReference)x));
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

        private void StateMachineAttributeClone(MethodDefinition clonedMethodDef, TypeDefinition clonedStateMachineTypeDef, string stateMachineAttributeTypeName)
        {
            var stateMachineAttribute = clonedMethodDef.CustomAttributes.Single(x => x.Is(stateMachineAttributeTypeName));
            clonedMethodDef.CustomAttributes.Clear();

            if (!_stateMachineCtorRefs.TryGetValue(stateMachineAttributeTypeName, out var ctor))
            {
                ctor = stateMachineAttribute.AttributeType.Resolve().Methods.Single(x => x.IsConstructor && !x.IsStatic && x.Parameters.Single().ParameterType.Is(Constants.TYPE_Type)).ImportInto(ModuleDefinition);
                _stateMachineCtorRefs[stateMachineAttributeTypeName] = ctor;
            }
            stateMachineAttribute = new CustomAttribute(ctor);
            stateMachineAttribute.ConstructorArguments.Add(new CustomAttributeArgument(_tTypeRef, clonedStateMachineTypeDef));
            clonedMethodDef.CustomAttributes.Add(stateMachineAttribute);
        }

        private void StateMachineFieldCleanup(TypeDefinition stateMachineTypeDef, IStateMachineFields fields)
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

        private void AsyncSetAbsentFields(RouMethod rouMethod, TypeDefinition stateMachineTypeDef, AsyncFields fields)
        {
            var instructions = rouMethod.MethodDef.Body.Instructions;
            var setState = instructions.Single(x => (x.OpCode.Code == Code.Ldc_I4_M1 || x.OpCode.Code == Code.Ldc_I4 && x.Operand is int v && v == -1) && x.Next.OpCode.Code == Code.Stfld && x.Next.Operand is FieldReference fr && fr.Resolve() == fields.State.Resolve());
            var vStateMachine = rouMethod.MethodDef.Body.Variables.Single(x => x.VariableType.Resolve() == stateMachineTypeDef);
            var genericMap = stateMachineTypeDef.GenericParameters.ToDictionary(x => x.Name, x => (TypeReference)x);

            stateMachineTypeDef.AddFields(StateMachineSetAbsentFieldThis(rouMethod, fields, vStateMachine.VariableType, vStateMachine.LdlocAny(), setState, genericMap));

            stateMachineTypeDef.AddFields(StateMachineSetAbsentFieldParameters(rouMethod, fields, vStateMachine.VariableType, vStateMachine.LdlocAny(), setState, genericMap));
        }

        private IEnumerable<FieldDefinition> StateMachineSetAbsentFieldThis(RouMethod rouMethod, IStateMachineFields fields, TypeReference stateMachineTypeRef, Instruction loadStateMachine, Instruction anchor, Dictionary<string, TypeReference> genericMap)
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

        private IEnumerable<FieldDefinition> StateMachineSetAbsentFieldParameters(RouMethod rouMethod, IStateMachineFields fields, TypeReference stateMachineTypeRef, Instruction loadStateMachine, Instruction anchor, Dictionary<string, TypeReference> genericMap)
        {
            if (fields.Parameters.All(x => x != null)) yield break;

            var instructions = rouMethod.MethodDef.Body.Instructions;
            var parameters = rouMethod.MethodDef.Parameters;

            for (var i = 0; i < fields.Parameters.Length; i++)
            {
                var parameterFieldDef = fields.Parameters[i];
                if (parameterFieldDef != null) continue;

                var parameter = parameters[i];
                var fieldTypeRef = parameter.ParameterType.ReplaceGenericArgs(genericMap);
                parameterFieldDef = new FieldDefinition(parameter.Name, FieldAttributes.Public, fieldTypeRef);
                var parameterFieldRef = new FieldReference(parameterFieldDef.Name, parameterFieldDef.FieldType, stateMachineTypeRef);

                fields.SetParameter(i, parameterFieldDef);

                instructions.InsertBefore(anchor, [
                    loadStateMachine.Clone(),
                    Create(OpCodes.Ldarg, parameter),
                    Create(OpCodes.Stfld, parameterFieldRef)
                ]);

                yield return parameterFieldDef;
            }
        }
    }
}
