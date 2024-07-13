using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;
using Mono.Cecil;
using Rougamo.Fody.Enhances.Async;
using Rougamo.Fody.Simulations;
using Rougamo.Fody.Simulations.Asserters;
using Rougamo.Fody.Simulations.PlainValues;
using Rougamo.Fody.Simulations.Types;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void WeavingAsyncTaskMethod(RouMethod rouMethod)
        {
            var stateMachineTypeDef = rouMethod.MethodDef.ResolveStateMachine(Constants.TYPE_AsyncStateMachineAttribute);
            var actualStateMachineTypeDef = ProxyStateMachineClone(stateMachineTypeDef);
            if (actualStateMachineTypeDef == null) return;

            var actualMethodDef = ProxyStateMachineSetupMethodClone(rouMethod.MethodDef, stateMachineTypeDef, actualStateMachineTypeDef, Constants.TYPE_AsyncStateMachineAttribute);
            rouMethod.MethodDef.DeclaringType.Methods.Add(actualMethodDef);

            var actualMoveNextDef = actualStateMachineTypeDef.Methods.Single(m => m.Name == Constants.METHOD_MoveNext);
            actualMoveNextDef.DebugInformation.StateMachineKickOffMethod = actualMethodDef;

            var fields = AsyncResolveFields(rouMethod, stateMachineTypeDef);
            ProxyAsyncSetAbsentFields(rouMethod, stateMachineTypeDef, fields);
            ProxyFieldCleanup(stateMachineTypeDef, fields);

            var tStateMachine = stateMachineTypeDef.Simulate<TsAsyncStateMachine>(ModuleDefinition).SetFields(fields);
            AsyncBuildMoveNext(rouMethod, tStateMachine);
        }

        private void AsyncBuildMoveNext(RouMethod rouMethod, TsAsyncStateMachine tStateMachine)
        {
            if (tStateMachine.F_MoArray == null)
            {
                AsyncBuildMosMoveNext(rouMethod, tStateMachine);
            }
            else
            {
                AsyncBuildMoArrayMoveNext(rouMethod, tStateMachine);
            }
        }

        private void AsyncBuildMosMoveNext(RouMethod rouMethod, TsAsyncStateMachine tStateMachine)
        {
            var mMoveNext = tStateMachine.M_MoveNext;
            var mActualMethod = rouMethod.MethodDef.Simulate<TsAsyncable>(tStateMachine.F_DeclaringThis?.Value ?? new TsWeaveingTarget(rouMethod.MethodDef.DeclaringType, null, ModuleDefinition));
            var instructions = mMoveNext.Def.Body.Instructions;

            var context = new AsyncContext(rouMethod);

            var vState = mMoveNext.CreateVariable(_typeIntRef);
            var vAwaiter = mMoveNext.CreateVariable<TsAwaiter>(tStateMachine.F_Awaiter.Value);
            var vMoAwaiter = tStateMachine.F_MoAwaiter == tStateMachine.F_Awaiter ? vAwaiter : mMoveNext.CreateVariable<TsAwaiter>(tStateMachine.F_MoAwaiter.Value);
            var vException = mMoveNext.CreateVariable(_typeExceptionRef);
            var vInnerException = rouMethod.Features.HasIntersection(Feature.OnException | Feature.OnExit) ? mMoveNext.CreateVariable(_typeExceptionRef) : null;
            var vPersistentInnerException = context.OnExceptionAllInSync && context.OnExitAllInSync ? null : mMoveNext.CreateVariable(_typeExceptionRef);

            Instruction? outerTryStart, outerCatchStart, outerCatchEnd, innerTryStart, innerCatchStart, innerCatchEnd = null;
            var switches = Create(OpCodes.Switch, []);

            // .var state = _state;
            instructions.Add(vState.Assign(tStateMachine.F_State));

            // .try
            {
                instructions.Add(switches);
                outerTryStart = switches;
                // .switch (state)
                {
                    // ._mo = new Mo1Attribute(..);
                    instructions.Add(AsyncStateMachineInitMos(rouMethod, tStateMachine));
                    // ._context = new MethodContext(..);
                    instructions.Add(AsyncStateMachineInitMethodContext(rouMethod, tStateMachine));
                    // ._mo.OnEntry(_context); <--> _mo.OnEntryAsync(_context).GetAwaiter()...
                    instructions.Add(AsyncMosOn(rouMethod, tStateMachine, vMoAwaiter, context, Feature.OnEntry, ForceSync.OnEntry, fMo => fMo.Value.M_OnEntry, fMo => fMo.Value.M_OnEntryAsync));

                    // .if (_context.ReturnValueReplaced) { ... }
                    instructions.Add(AsyncIfOnEntryReplacedReturn(rouMethod, tStateMachine, context));
                    // .if (_context.RewriteArguments) { ... }
                    instructions.Add(AsyncIfRewriteArguments(rouMethod, tStateMachine, context));
                    // .PROXY CALL
                    instructions.Add(context.AnchorProxyCallCase);
                    context.AnchorSwitches.Add(context.AnchorProxyCallCase);
                    // .try
                    {
                        // .ActualMethod(...).GetAwaiter()...
                        innerTryStart = instructions.AddGetFirst(AsyncProxyCall(rouMethod, tStateMachine, vState, vAwaiter, mActualMethod, context));
                        // ._result = awaiter.GetResult(); _context.ReturnValue = _result;
                        instructions.Add(AsyncSaveResult(rouMethod, tStateMachine, vAwaiter));
                    }
                    // .catch (Exception innerException)
                    if (vInnerException != null)
                    {
                        innerCatchStart = instructions.AddGetFirst(vInnerException.Assign(target => []));
                        if (vPersistentInnerException != null)
                        {
                            // persistentInnerException = innerException;
                            instructions.Add(vPersistentInnerException.Assign(vInnerException));
                        }
                        else
                        {
                            // ._context.Exception = innerException;
                            instructions.Add(tStateMachine.F_MethodContext.Value.P_Exception.Assign(vInnerException));
                            // ._mo.OnException(_context); ...
                            instructions.Add(AsyncMosSyncOnException(rouMethod, tStateMachine));
                            instructions.Add(AsynCheckRetry(rouMethod, tStateMachine, context, true));
                            instructions.Add(AsyncSaveResultIfExceptionHandled(rouMethod, tStateMachine));
                            instructions.Add(AsyncMosSyncOnExit(rouMethod, tStateMachine));
                            instructions.Add(AsyncMosReturnIfExceptionHandled(rouMethod, tStateMachine, context));
                            instructions.Add(Create(OpCodes.Rethrow));
                        }
                    }
                    if (vPersistentInnerException != null)
                    {
                        // .if (persistentInnerException != null)
                        innerCatchEnd = instructions.AddGetFirst(vPersistentInnerException.IsEqual(new Null()).IfNot(anchor =>
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
                    innerCatchEnd ??= instructions.AddGetFirst(AsyncMosOn(rouMethod, tStateMachine, vMoAwaiter, context, Feature.OnSuccess, ForceSync.OnSuccess, fMo => fMo.Value.M_OnSuccess, fMo => fMo.Value.M_OnSuccessAsync));
                    // .if (_context.RetryCount > 0) continue;
                    instructions.Add(AsynCheckRetry(rouMethod, tStateMachine, context, false));
                    // .if (_context.ReturnValueReplaced) _result = _context.ReturnValue;
                    instructions.Add(AsyncSaveResultIfReplaced(rouMethod, tStateMachine));
                    // .ON_EXIT
                    instructions.Add(context.AnchorOnExitAsync);
                    // ._mo.OnExitAsync(_context).GetAwaiter(); ...
                    instructions.Add(AsyncMosOn(rouMethod, tStateMachine, vMoAwaiter, context, Feature.OnExit, ForceSync.OnExit, fMo => fMo.Value.M_OnExit, fMo => fMo.Value.M_OnExitAsync));

                    if (vPersistentInnerException != null)
                    {
                        instructions.Add(tStateMachine.F_MethodContext.Value.P_Exception.IsEqual(new Null()).IfNot(anchor =>
                        {
                            // .ExceptionDispatchInfo.Capture(persistentInnerException).Throw();
                            return [
                                .. vPersistentInnerException.Load(),
                                Create(OpCodes.Call, _methodExceptionDispatchInfoCaptureRef),
                                Create(OpCodes.Callvirt, _methodExceptionDispatchInfoThrowRef)
                            ];
                        }));
                    }
                    // goto END;
                    instructions.Add(Create(OpCodes.Leave, context.AnchorSetResult));

                    if (vPersistentInnerException != null)
                    {
                        // .ON_EXCEPTION
                        instructions.Add(context.AnchorOnExceptionAsync);
                        // ._mo.OnExceptionAsync(_context).GetAwaiter(); ...
                        instructions.Add(AsyncMosOn(rouMethod, tStateMachine, vMoAwaiter, context, Feature.OnException, ForceSync.OnException, fMo => fMo.Value.M_OnException, fMo => fMo.Value.M_OnExceptionAsync));
                        // .if (_context.RetryCount > 0) continue;
                        instructions.Add(AsynCheckRetry(rouMethod, tStateMachine, context, false));
                        // .if (_context.ExceptionHandled) _result = _context.ReturnValue;
                        instructions.Add(AsyncSaveResultIfExceptionHandled(rouMethod, tStateMachine));
                        // .goto ON_EXIT;
                        instructions.Add(Create(OpCodes.Br, context.AnchorOnExitAsync));
                    }
                }
            }
            // .catch (Exception exception)
            {
                outerCatchStart = instructions.AddGetFirst(vException.Assign(target => []));
                // ._state = -2;
                instructions.Add(tStateMachine.F_State.Assign(-2));
                // ._builder.SetException(exception);
                instructions.Add(tStateMachine.F_Builder.Value.M_SetException.Call(vException));
                // .return;
                instructions.Add(Create(OpCodes.Ret));
            }
            // .END
            instructions.Add(context.AnchorSetResult);
            outerCatchEnd = context.AnchorSetResult;
            // ._state = -2;
            instructions.Add(tStateMachine.F_State.Assign(-2));
            // ._builder.SetResult(_result);
            IParameterSimulation[] setResultArgs = tStateMachine.F_Result == null ? [] : [tStateMachine.F_Result];
            instructions.Add(tStateMachine.F_Builder.Value.M_SetResult.Call(setResultArgs));

            switches.Operand = context.AnchorSwitches.ToArray();

            mMoveNext.Def.DebuggerStepThrough(_methodDebuggerStepThroughCtorRef);
            mMoveNext.Def.Body.OptimizePlus(EmptyInstructions);
        }

        private void AsyncBuildMoArrayMoveNext(RouMethod rouMethod, TsAsyncStateMachine tStateMachine)
        {

        }

        private IList<Instruction> AsyncStateMachineInitMos(RouMethod rouMethod, TsAsyncStateMachine tStateMachine)
        {
            var instructions = new List<Instruction>();

            for (int i = 0; i < rouMethod.Mos.Length; i++)
            {
                var mo = rouMethod.Mos[i];
                var fMo = tStateMachine.F_Mos![i];
                instructions.Add(fMo.Assign(target => fMo.Value.New(mo, tStateMachine.M_MoveNext)));
            }

            return instructions;
        }

        private IList<Instruction> AsyncStateMachineInitMethodContext(RouMethod rouMethod, TsAsyncStateMachine tStateMachine)
        {
            IParameterSimulation?[] arguments = [
                tStateMachine.F_DeclaringThis,
                new SystemType(rouMethod.MethodDef.DeclaringType),
                new SystemMethodBase(rouMethod.MethodDef),
                new Bool(rouMethod.IsAsyncTaskOrValueTask || rouMethod.IsAsyncIterator),
                new Bool(rouMethod.IsIterator || rouMethod.IsAsyncIterator),
                new Bool(!_config.ReverseCallNonEntry),
                rouMethod.MethodContextOmits.Contains(Omit.Mos) ? null : _typeIMoRef.Simulate<TsArray>(ModuleDefinition).NewAsPlainValue(tStateMachine.F_Mos!),
                rouMethod.MethodContextOmits.Contains(Omit.Arguments) ? null : _typeObjectRef.Simulate<TsArray>(ModuleDefinition).NewAsPlainValue(tStateMachine.F_Parameters)
            ];
            return tStateMachine.F_MethodContext.AssignNew(arguments);
        }

        private IList<Instruction>? AsyncStateMachineMosOnEntry(RouMethod rouMethod, TsAsyncStateMachine tStateMachine, VariableSimulation<TsAwaiter> vMoAwaiter, AsyncContext context)
        {
            if (!rouMethod.Features.Contains(Feature.OnEntry)) return null;

            var instructions = new List<Instruction>();

            for (var i = 0; i < rouMethod.Mos.Length; i++)
            {
                var mo = rouMethod.Mos[i];
                if (!mo.Features.Contains(Feature.OnEntry)) continue;

                instructions.Add(AsyncAwaitMoAwaiterIfNeed(rouMethod, tStateMachine, vMoAwaiter, context));

                var fMo = tStateMachine.F_Mos![i];
                if (mo.ForceSync.Contains(ForceSync.OnEntry))
                {
                    // ._mo.OnEntry(_context);
                    instructions.Add(fMo.Value.M_OnEntry.Call(tStateMachine.F_MethodContext));
                }
                else
                {
                    // .moAwaiter = _mo.OnEntryAsync(_context).GetAwaiter();
                    instructions.Add(fMo.Value.M_OnEntryAsync.Call(tStateMachine.F_MethodContext));
                    instructions.Add(vMoAwaiter.Assign(target => fMo.Value.M_OnEntryAsync.Result.M_GetAwaiter.Call()));
                    // .if (!moAwaiter.IsCompleted)
                    instructions.Add(vMoAwaiter.Value.P_IsCompleted.IfNot(anchor =>
                    {
                        var insts = new List<Instruction>();

                        // ._state = STATE++;
                        tStateMachine.F_State.Assign(context.State++);
                        // ._moAwaiter = moAwaiter
                        tStateMachine.F_MoAwaiter.Assign(vMoAwaiter);
                        // ._builder.AwaitUnsafeOnCompleted(ref moAwaiter, ref this);
                        tStateMachine.F_Builder.Value.M_AwaitUnsafeOnCompleted.Call(vMoAwaiter, tStateMachine);
                        // .return
                        insts.Add(Create(OpCodes.Ret));

                        return insts;
                    }));
                    // goto NEXT_STATE;
                    instructions.Add(Create(OpCodes.Br, context.GetNextStateReadyAnchor()));
                    context.AwaitFirst = true;
                }
            }

            instructions.Add(AsyncAwaitMoAwaiterIfNeed(rouMethod, tStateMachine, vMoAwaiter, context));

            return instructions;
        }

        private IList<Instruction> AsyncAwaitMoAwaiterIfNeed(RouMethod rouMethod, TsAsyncStateMachine tStateMachine, VariableSimulation<TsAwaiter> vMoAwaiter, AsyncContext context)
        {
            if (!context.AwaitFirst) return [];

            // .moAwaiter = _moAwaiter;
            var assignAwaiterInsts = vMoAwaiter.Assign(tStateMachine.F_MoAwaiter);

            Instruction[] instructions = [
                .. assignAwaiterInsts,
                context.AnchorStateReady,
                // .moAwaiter.GetResult();
                .. vMoAwaiter.Value.M_GetResult.Call()
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
                    instructions.Add(AsyncMosSyncOnExit(rouMethod, tStateMachine));
                }
                else
                {
                    // goto ON_EXIT;
                    instructions.Add(Create(OpCodes.Br, context.AnchorOnExitAsync));
                }

                return instructions;
            });
        }

        private IList<Instruction> AsyncIfRewriteArguments(RouMethod rouMethod, TsAsyncStateMachine tStateMachine, AsyncContext context)
        {
            if (rouMethod.MethodDef.Parameters.Count == 0 || !rouMethod.Features.Contains(Feature.RewriteArgs) || rouMethod.MethodContextOmits.Contains(Omit.Arguments)) return [];

            return tStateMachine.F_MethodContext.Value.P_RewriteArguments.If(anchor =>
            {
                var instructions = new List<Instruction>();
                var vArguments = tStateMachine.M_MoveNext.CreateVariable<TsArray>(new ArrayType(_typeObjectRef));

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

        private IList<Instruction> AsyncProxyCall(RouMethod rouMethod, TsAsyncStateMachine tStateMachine, VariableSimulation vState, VariableSimulation<TsAwaiter> vAwaiter, MethodSimulation<TsAsyncable> mActualMethod, AsyncContext context)
        {
            // .if (state == STATE)
            return vState.IsEqual(new Int32Value(context.State)).If((a1, a2) =>
            {
                // .awaiter = _awaiter;
                return vAwaiter.Assign(tStateMachine.F_Awaiter);
            }, (a1, a2) =>
            {// .else (state != STATE)
                return [
                    // .awaiter = ActualMethod(...).GetAwaiter();
                    .. mActualMethod.Call(tStateMachine.F_Parameters),
                    .. vAwaiter.Assign(target => mActualMethod.Result.M_GetAwaiter.Call()),
                    // .if (!awaiter.IsCompleted)
                    .. vAwaiter.Value.P_IsCompleted.IfNot(anchor => [
                        // ._state = STATE++;
                        .. tStateMachine.F_State.Assign(context.State++),
                        // ._awaiter = awaiter;
                        .. tStateMachine.F_Awaiter.Assign(vAwaiter),
                        // ._builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
                        .. tStateMachine.F_Builder.Value.M_AwaitUnsafeOnCompleted.Call(vAwaiter, tStateMachine),
                        // .return;
                        Create(OpCodes.Ret)
                    ]),
                ];
            });
        }

        private IList<Instruction>? AsyncSaveResult(RouMethod rouMethod, TsAsyncStateMachine tStateMachine, VariableSimulation<TsAwaiter> vAwaiter)
        {
            if (tStateMachine.F_Result == null || !rouMethod.Features.HasIntersection(Feature.OnSuccess | Feature.OnExit) || rouMethod.MethodContextOmits.Contains(Omit.ReturnValue))
            {
                // .awaiter.GetResult();
                return vAwaiter.Value.M_GetResult.Call();
            }

            return [
                // ._result = awaiter.GetResult();
                .. tStateMachine.F_Result.Assign(target => vAwaiter.Value.M_GetResult.Call()),
                // ._context.ReturnValue  = _result;
                .. tStateMachine.F_MethodContext.Value.P_ReturnValue.Assign(tStateMachine.F_Result)
            ];
        }

        private IList<Instruction> AsynCheckRetry(RouMethod rouMethod, TsAsyncStateMachine tStateMachine, AsyncContext context, bool insideBlock)
        {
            if (!rouMethod.Features.Contains(Feature.ExceptionRetry)) return [];

            return tStateMachine.F_MethodContext.Value.P_RetryCount.Gt(new Int32Value(0)).If(anchor =>
            {
                return [Create(insideBlock ? OpCodes.Leave : OpCodes.Br, context.AnchorProxyCallCase)];
            });
        }

        private IList<Instruction> AsyncSaveResultIfExceptionHandled(RouMethod rouMethod, TsAsyncStateMachine tStateMachine)
        {
            if (tStateMachine.F_Result == null || !rouMethod.Features.Contains(Feature.ExceptionHandle) || rouMethod.MethodContextOmits.Contains(Omit.ReturnValue)) return [];

            // .if (_context.ExceptionHandled)
            return tStateMachine.F_MethodContext.Value.P_ExceptionHandled.If(anchor =>
            {
                // ._result = _context.ReturnValue;
                return tStateMachine.F_Result.Assign(tStateMachine.F_MethodContext.Value.P_ReturnValue);
            });
        }

        private IList<Instruction> AsyncMosReturnIfExceptionHandled(RouMethod rouMethod, TsAsyncStateMachine tStateMachine, AsyncContext context)
        {
            if (!rouMethod.Features.Contains(Feature.ExceptionHandle) || rouMethod.MethodContextOmits.Contains(Omit.ReturnValue)) return [];

            // .if (_context.ExceptionHandled)
            return tStateMachine.F_MethodContext.Value.P_ExceptionHandled.If(anchor =>
            {
                // .goto END;
                return [Create(OpCodes.Leave, context.AnchorSetResult)];
            });
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

        private IList<Instruction> AsyncMosSyncOnException(RouMethod rouMethod, TsAsyncStateMachine tStateMachine)
        {
            if (!rouMethod.Features.Contains(Feature.OnException)) return [];

            var instructions = new List<Instruction>();

            foreach (var mo in tStateMachine.F_Mos!)
            {
                instructions.Add(mo.Value.M_OnException.Call(tStateMachine.F_MethodContext));
            }

            return instructions;
        }

        private IList<Instruction> AsyncMosSyncOnExit(RouMethod rouMethod, TsAsyncStateMachine tStateMachine)
        {
            if (!rouMethod.Features.Contains(Feature.OnExit)) return [];

            var instructions = new List<Instruction>();

            foreach (var mo in tStateMachine.F_Mos!)
            {
                instructions.Add(mo.Value.M_OnExit.Call(tStateMachine.F_MethodContext));
            }

            return instructions;
        }

        private IList<Instruction> AsyncMosOn(RouMethod rouMethod, TsAsyncStateMachine tStateMachine, VariableSimulation<TsAwaiter> vMoAwaiter, AsyncContext context, Feature feature, ForceSync forceSync, Func<FieldSimulation<TsMo>, MethodSimulation> syncMethodFactory, Func<FieldSimulation<TsMo>, MethodSimulation<TsAsyncable>> asyncMethodFactory)
        {
            if (!rouMethod.Features.Contains(feature)) return [];

            var instructions = new List<Instruction>();

            for (var i = 0; i < rouMethod.Mos.Length; i++)
            {
                var mo = rouMethod.Mos[i];
                if (!mo.Features.Contains(feature)) continue;

                instructions.Add(AsyncAwaitMoAwaiterIfNeed(rouMethod, tStateMachine, vMoAwaiter, context));

                var fMo = tStateMachine.F_Mos![i];
                if (mo.ForceSync.Contains(forceSync))
                {
                    // ._mo.OnXxx(_context);
                    instructions.Add(syncMethodFactory(fMo).Call(tStateMachine.F_MethodContext));
                }
                else
                {
                    // .moAwaiter = _mo.OnXxxAsync(_context).GetAwaiter();
                    var method = asyncMethodFactory(fMo);
                    instructions.Add(method.Call(tStateMachine.F_MethodContext));
                    instructions.Add(vMoAwaiter.Assign(target => method.Result.M_GetAwaiter.Call()));
                    // .if (!moAwaiter.IsCompleted)
                    instructions.Add(vMoAwaiter.Value.P_IsCompleted.IfNot(anchor =>
                    {
                        var insts = new List<Instruction>();

                        // ._state = STATE++;
                        tStateMachine.F_State.Assign(context.State++);
                        // ._moAwaiter = moAwaiter
                        tStateMachine.F_MoAwaiter.Assign(vMoAwaiter);
                        // ._builder.AwaitUnsafeOnCompleted(ref moAwaiter, ref this);
                        tStateMachine.F_Builder.Value.M_AwaitUnsafeOnCompleted.Call(vMoAwaiter, tStateMachine);
                        // .return
                        insts.Add(Create(OpCodes.Ret));

                        return insts;
                    }));
                    // goto NEXT_STATE;
                    instructions.Add(Create(OpCodes.Br, context.GetNextStateReadyAnchor()));
                    context.AwaitFirst = true;
                }
            }

            instructions.Add(AsyncAwaitMoAwaiterIfNeed(rouMethod, tStateMachine, vMoAwaiter, context));

            return instructions;
        }
    }
}
