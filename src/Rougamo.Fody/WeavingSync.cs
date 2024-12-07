using Fody;
using Fody.Simulations;
using Fody.Simulations.Operations;
using Fody.Simulations.PlainValues;
using Fody.Simulations.Types;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Rougamo.Context;
using Rougamo.Fody.Contexts;
using Rougamo.Fody.Simulations.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void WeavingSyncMethod(RouMethod rouMethod)
        {
            var actualMethodDef = rouMethod.MethodDef.Clone($"$Rougamo_{rouMethod.MethodDef.Name}");
            rouMethod.MethodDef.DeclaringType.Methods.Add(actualMethodDef);
            var asyncStateMachineAttribute = SyncClearCustomAttributes(actualMethodDef);
            if (asyncStateMachineAttribute != null)
            {
                rouMethod.MethodDef.CustomAttributes.Remove(asyncStateMachineAttribute);
                actualMethodDef.CustomAttributes.Add(asyncStateMachineAttribute);
            }

            var tWeavingTarget = rouMethod.MethodDef.DeclaringType.Simulate<TsWeavingTarget>(this).SetMethods(actualMethodDef, rouMethod.MethodDef);
            SyncBuildProxyMethod(rouMethod, tWeavingTarget);
        }

        private void SyncBuildProxyMethod(RouMethod rouMethod, TsWeavingTarget tWeavingTarget)
        {
            StackTraceHidden(rouMethod.MethodDef);

            tWeavingTarget.M_Proxy.Def.Clear();
            DebuggerStepThrough(tWeavingTarget.M_Proxy.Def);

            SyncBuildProxyMethodInternal(rouMethod, tWeavingTarget);

            tWeavingTarget.M_Proxy.Def.Body.InitLocals = true;
            tWeavingTarget.M_Proxy.Def.Body.OptimizePlus();
        }

        private void SyncBuildProxyMethodInternal(RouMethod rouMethod, TsWeavingTarget tWeavingTarget)
        {
            var instructions = tWeavingTarget.M_Proxy.Def.Body.Instructions;

            var context = new SyncContext();

            var args = tWeavingTarget.M_Proxy.Def.Parameters.Select(x => x.Simulate(this)).ToArray();
            var vException = rouMethod.Features.HasIntersection(Feature.OnException | Feature.OnExit) ? tWeavingTarget.M_Proxy.CreateVariable(_tExceptionRef) : null;
            var vResult = tWeavingTarget.M_Proxy.Result.Ref.IsVoid() ? null : tWeavingTarget.M_Proxy.CreateVariable(tWeavingTarget.M_Proxy.Result);
            var vContext = tWeavingTarget.M_Proxy.CreateVariable<TsMethodContext>(_tMethodContextRef);
            var vMos = rouMethod.Mos.Select(x => tWeavingTarget.M_Proxy.CreateVariable<TsMo>(x.MoTypeRef)).ToArray();

            Instruction poolTryStart = Create(OpCodes.Nop), poolFinallyStart = Create(OpCodes.Nop), poolFinallyEnd = Create(OpCodes.Nop);
            Instruction? tryStart = null, catchStart = null, catchEnd = Create(OpCodes.Nop);

            var pooledItems = new List<IParameterSimulation> { vContext };

            // .var mo = new Mo1Attributue(...);
            instructions.Add(SyncInitMos(rouMethod, tWeavingTarget, vMos, pooledItems));
            // .var context = new MethodContext(...);
            instructions.Add(SyncInitMethodContext(rouMethod, tWeavingTarget, args, vContext, vMos));

            instructions.Add(poolTryStart);
            // .try
            {
                // .mo.OnEntry(context);
                instructions.Add(SyncMosOn(rouMethod, tWeavingTarget, vContext, vMos, Feature.OnEntry, mo => mo.M_OnEntry));
                // .if (context.ReturnValueReplaced) { .. }
                instructions.Add(SyncIfOnEntryReplacedReturn(rouMethod, tWeavingTarget, vContext, vMos, vResult, poolFinallyEnd));
                // .if (_context.RewriteArguments) { ... }
                instructions.Add(SyncIfRewriteArguments(rouMethod, tWeavingTarget, args, vContext));

                // .RETRY:
                instructions.Add(context.AnchorRetry);
                // .try
                {
                    if (vResult == null)
                    {
                        // .actualMethod(...);
                        tryStart = instructions.AddGetFirst(tWeavingTarget.M_Actual.Call(tWeavingTarget.M_Proxy, args));
                    }
                    else
                    {
                        // .result = actualMethod(...);
                        tryStart = instructions.AddGetFirst(vResult.Assign(target => tWeavingTarget.M_Actual.Call(tWeavingTarget.M_Proxy, args)));
                    }
                    // .context.ReturnValue = result;
                    SyncSaveResult(rouMethod, tWeavingTarget, vContext, vResult);

                    if (vException != null)
                    {
                        instructions.Add(Create(OpCodes.Leave, catchEnd));
                    }
                }
                // .catch
                if (vException != null)
                {
                    catchStart = instructions.AddGetFirst(vException.Assign(target => []));
                    // .context.Exception = exception;
                    instructions.Add(vContext.Value.P_Exception.Assign(vException));
                    // .mo.OnException(context); ...
                    instructions.Add(SyncMosOn(rouMethod, tWeavingTarget, vContext, vMos, Feature.OnException, mo => mo.M_OnException));
                    // .arg_i = context.Arguments[i];
                    instructions.Add(SyncRefreshArguments(rouMethod, tWeavingTarget, args, vContext, Feature.OnException));
                    // .if (context.RetryCount > 0) goto RETRY;
                    instructions.Add(SynCheckRetry(rouMethod, tWeavingTarget, vContext, context, Feature.OnException, true));
                    // .if (exceptionHandled) result = context.ReturnValue;
                    instructions.Add(SyncSaveResultIfExceptionHandled(rouMethod, tWeavingTarget, vContext, vResult, out var vExceptionHandled));
                    // .mo.OnExit(context);
                    instructions.Add(SyncMosOn(rouMethod, tWeavingTarget, vContext, vMos, Feature.OnExit, mo => mo.M_OnExit));
                    // .if (exceptionHandled) return result;
                    instructions.Add(SyncReturnIfExceptionHandled(rouMethod, tWeavingTarget, vExceptionHandled, context));
                    instructions.Add(Create(OpCodes.Rethrow));

                    instructions.Add(catchEnd);
                }

                // .context.ReturnValue = result;
                instructions.Add(SyncSaveResult(rouMethod, vContext, vResult));
                // .mo.OnSuccess(context);
                instructions.Add(SyncMosOn(rouMethod, tWeavingTarget, vContext, vMos, Feature.OnSuccess, mo => mo.M_OnSuccess));
                // .arg_i = context.Arguments[i];
                instructions.Add(SyncRefreshArguments(rouMethod, tWeavingTarget, args, vContext, Feature.OnSuccess));
                // .if (context.RetryCount > 0) goto RETRY;
                instructions.Add(SynCheckRetry(rouMethod, tWeavingTarget, vContext, context, Feature.OnSuccess, false));
                // .if (returnValueReplaced) result = context.ReturnValue;
                instructions.Add(SyncSaveResultIfReturnValueReplaced(rouMethod, tWeavingTarget, vContext, vResult));
                // .mo.OnExit(context);
                instructions.Add(SyncMosOn(rouMethod, tWeavingTarget, vContext, vMos, Feature.OnExit, mo => mo.M_OnExit));
                // .return result;
                instructions.Add(context.AnchorReturnResult.Set(OpCodes.Leave, poolFinallyEnd));
            }
            // .finally
            {
                instructions.Add(poolFinallyStart);

                // .RougamoPool<..>.Return(..);
                foreach (var pooledItem in pooledItems)
                {
                    instructions.Add(ReturnToPool(pooledItem, tWeavingTarget.M_Proxy));
                }

                instructions.Add(Create(OpCodes.Endfinally));
                instructions.Add(poolFinallyEnd);
            }
            // .return result;
            if (vResult != null) instructions.Add(vResult.Load());
            instructions.Add(Create(OpCodes.Ret));

            SetTryCatch(tWeavingTarget.M_Proxy.Def, tryStart, catchStart, catchEnd);
            SetTryFinally(tWeavingTarget.M_Proxy.Def, poolTryStart, poolFinallyStart, poolFinallyEnd);
        }

        private IList<Instruction> SyncInitMos(RouMethod rouMethod, TsWeavingTarget tWeavingTarget, VariableSimulation<TsMo>[] vMos, List<IParameterSimulation> pooledItems)
        {
            var instructions = new List<Instruction>();

            var loadables = new List<ILoadable>(rouMethod.Mos.Length);

            for (int i = 0; i < rouMethod.Mos.Length; i++)
            {
                var mo = rouMethod.Mos[i];

                var vMo = vMos[i];
                instructions.Add(vMo.Assign(target => NewMo(rouMethod, mo, vMo.Value, tWeavingTarget.M_Proxy, pooledItems)));
            }

            return instructions;
        }

        private IList<Instruction> SyncInitMethodContext(RouMethod rouMethod, TsWeavingTarget tWeavingTarget, ArgumentSimulation[] args, VariableSimulation<TsMethodContext> vContext, VariableSimulation<TsMo>[] vMos)
        {
            var instructions = new List<Instruction>();

            // .var context = RougamoPool<MethodContext>.Get();
            instructions.AddRange(AssignByPool(vContext));
            // .context.Mos = new Mo[] { ... };
            if (!rouMethod.MethodContextOmits.Contains(Omit.Mos))
            {
                var tMoArray = _tIMoArrayRef.Simulate<TsArray>(this);
                instructions.AddRange(vContext.Value.P_Mos.Assign(tMoArray.NewAsPlainValue(vMos)));
            }
            // .context.Target = this;
            if (!tWeavingTarget.M_Proxy.Def.IsStatic)
            {
                instructions.AddRange(vContext.Value.P_Target.Assign(new This(_simulations.Object)));
            }
            // .context.TargeType = typeof(TARGET_TYPE);
            instructions.AddRange(vContext.Value.P_TargetType.Assign(new SystemType(rouMethod.MethodDef.DeclaringType, this)));
            // .context.Method = methodof(TARGET_METHOD);
            instructions.AddRange(vContext.Value.P_Method.Assign(new SystemMethodBase(rouMethod.MethodDef, this)));
            // .context.Arguments = new object[] { ... };
            if (!rouMethod.MethodContextOmits.Contains(Omit.Arguments))
            {
                var tObjectArray = _tObjectArrayRef.Simulate<TsArray>(this);
                var exceptedRefStructArgs = args.Select(x => x.IsRefStruct() ? x.BeFake(new Null(this)) : x).ToArray();
                instructions.AddRange(vContext.Value.P_Arguments.Assign(tObjectArray.NewAsPlainValue(exceptedRefStructArgs)));
            }

            return instructions;
        }

        private IList<Instruction>? SyncIfOnEntryReplacedReturn(RouMethod rouMethod, TsWeavingTarget tWeavingTarget, VariableSimulation<TsMethodContext> vContext, VariableSimulation<TsMo>[] vMos, VariableSimulation? vResult, Instruction poolFinallyEnd)
        {
            if (!rouMethod.Features.Contains(Feature.EntryReplace) ||
                rouMethod.MethodContextOmits.Contains(Omit.ReturnValue) ||
                vResult != null && rouMethod.SkipRefStruct && vResult.IsRefStruct()) return null;

            // .if (context.ReturnValueReplaced)
            return vContext.Value.P_ReturnValueReplaced.If(anchor =>
            {
                var instructions = new List<Instruction>();

                if (vResult != null)
                {
                    // .result = context.ReturnValue;
                    instructions.Add(vResult.Assign(vContext.Value.P_ReturnValue));
                }
                // .mo.OnExit(context);
                instructions.Add(SyncMosOn(rouMethod, tWeavingTarget, vContext, vMos, Feature.OnExit, mo => mo.M_OnExit));
                // .return result;
                if (vResult != null)
                {
                    instructions.Add(vResult.Load());
                }
                instructions.Add(Create(OpCodes.Leave, poolFinallyEnd));

                return instructions;
            });
        }

        private IList<Instruction> SyncIfRewriteArguments(RouMethod rouMethod, TsWeavingTarget tWeavingTarget, ArgumentSimulation[] args, VariableSimulation<TsMethodContext> vContext)
        {
            if (rouMethod.MethodDef.Parameters.Count == 0 || !rouMethod.Features.Contains(Feature.RewriteArgs) || rouMethod.MethodContextOmits.Contains(Omit.Arguments)) return [];

            return vContext.Value.P_RewriteArguments.If(anchor =>
            {
                var instructions = new List<Instruction>();
                var vArguments = tWeavingTarget.M_Proxy.CreateVariable<TsArray>(_tObjectArrayRef);

                // .var args = _context.Arguments;
                instructions.Add(vArguments.Assign(vContext.Value.P_Arguments));
                for (var i = 0; i < args.Length; i++)
                {
                    var arg = args[i];
                    if (arg.IsRefStruct()) continue;

                    // .if (args[i] == null)
                    instructions.Add(vArguments.Value[i].IsNull().If((a1, a2) =>
                    {
                        // ._parameters[i] = default;
                        return arg.AssignDefault();
                    }, (a1, a2) =>
                    {// .else (args[i] != null)
                        // ._parameters[i] = args[i];
                        return arg.Assign(vArguments.Value[i]);
                    }));
                }

                return instructions;
            });
        }

        private IList<Instruction> SyncSaveResult(RouMethod rouMethod, TsWeavingTarget tWeavingTarget, VariableSimulation<TsMethodContext> vContext, VariableSimulation? vResult)
        {
            if (vResult == null ||
                !rouMethod.Features.HasIntersection(Feature.OnSuccess | Feature.OnExit) ||
                rouMethod.MethodContextOmits.Contains(Omit.ReturnValue) ||
                rouMethod.SkipRefStruct && vResult.IsRefStruct()) return [];

            // .context.ReturnValue  = result;
            return vContext.Value.P_ReturnValue.Assign(vResult);
        }

        private IList<Instruction> SyncRefreshArguments(RouMethod rouMethod, TsWeavingTarget tWeavingTarget, ArgumentSimulation[] args, VariableSimulation<TsMethodContext> vContext, Feature feature, bool checkByRef = true)
        {
            if (!rouMethod.Features.Contains(feature | Feature.OnExit | Feature.FreshArgs) || rouMethod.MethodContextOmits.Contains(Omit.Arguments)) return [];

            var count = checkByRef ? args.Count(x => x.IsByReference) : args.Length;

            if (count == 0) return [];

            var instructions = new List<Instruction>();

            VariableSimulation<TsArray>? vArguments = null;
            if (count > 1)
            {
                // var arguments = context.Arguments;
                vArguments = tWeavingTarget.M_Proxy.CreateVariable<TsArray>(_tObjectArrayRef);
                instructions.Add(vArguments.Assign(vContext.Value.P_Arguments));
            }

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (checkByRef && !arg.IsByReference || arg.IsRefStruct()) continue;

                // .context.Arguments[i] = arg_i;
                if (vArguments == null)
                {
                    instructions.Add([
                        .. vContext.Value.P_Arguments.Getter!.Call(tWeavingTarget.M_Proxy),
                        .. vContext.Value.P_Arguments.Getter.Result[i].Assign(arg)
                    ]);
                }
                else
                {
                    instructions.Add(vArguments.Value[i].Assign(arg));
                }
            }

            return instructions;
        }

        private IList<Instruction> SynCheckRetry(RouMethod rouMethod, TsWeavingTarget tWeavingTarget, VariableSimulation<TsMethodContext> vContext, SyncContext context, Feature action, bool insideBlock)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            if (!rouMethod.Features.Contains(Feature.RetryAny | action)) return [];
#pragma warning restore CS0618 // Type or member is obsolete

            // .if (context.RetryCount > 0)
            return vContext.Value.P_RetryCount.Gt(0).If(anchor =>
            {
                // .goto RETRY;
                return [Create(insideBlock ? OpCodes.Leave : OpCodes.Br, context.AnchorRetry)];
            });
        }

        private IList<Instruction> SyncSaveResultIfExceptionHandled(RouMethod rouMethod, TsWeavingTarget tWeavingTarget, VariableSimulation<TsMethodContext> vContext, VariableSimulation? vResult, out VariableSimulation? vExceptionHandled)
        {
            vExceptionHandled = null;
            if (!rouMethod.Features.Contains(Feature.ExceptionHandle) ||
                rouMethod.MethodContextOmits.Contains(Omit.ReturnValue) ||
                vResult != null && rouMethod.SkipRefStruct && vResult.IsRefStruct()) return [];

            var instructions = new List<Instruction>();

            vExceptionHandled = tWeavingTarget.M_Proxy.CreateVariable(_tBooleanRef);

            // .exceptionHandled = context.ExceptionHandled;
            instructions.Add(vExceptionHandled.Assign(vContext.Value.P_ExceptionHandled));
            if (vResult != null)
            {
                // .if (exceptionHandled)
                instructions.Add(vExceptionHandled.If(anchor =>
                {
                    // .result = context.ReturnValue;
                    return vResult.Assign(vContext.Value.P_ReturnValue);
                }));
            }

            return instructions;
        }

        private IList<Instruction> SyncReturnIfExceptionHandled(RouMethod rouMethod, TsWeavingTarget tWeavingTarget, VariableSimulation? vExceptionHandled, SyncContext context)
        {
            if (vExceptionHandled == null) return [];

            // .if (exceptionHandled)
            return vExceptionHandled.If(anchor =>
            {
                // return result;
                return [Create(OpCodes.Leave, context.AnchorReturnResult)];
            });
        }

        private IList<Instruction> SyncReturnIfExceptionHandled(RouMethod rouMethod, VariableSimulation<TsMethodContext> vContext, SyncContext context)
        {
            // .if (context.ExceptionHandled)
            return vContext.Value.P_ExceptionHandled.If(anchor =>
            {
                // return result;
                return [Create(OpCodes.Leave, context.AnchorReturnResult)];
            });
        }

        private IList<Instruction> SyncSaveResult(RouMethod rouMethod, VariableSimulation<TsMethodContext> vContext, VariableSimulation? vResult)
        {
            if (vResult == null ||
                !rouMethod.Features.HasIntersection(Feature.OnSuccess | Feature.OnExit) ||
                rouMethod.MethodContextOmits.Contains(Omit.ReturnValue) ||
                rouMethod.SkipRefStruct && vResult.IsRefStruct()) return [];

            // .context.ReturnValue = result;
            return vContext.Value.P_ReturnValue.Assign(vResult);
        }

        private IList<Instruction> SyncSaveResultIfReturnValueReplaced(RouMethod rouMethod, TsWeavingTarget tWeavingTarget, VariableSimulation<TsMethodContext> vContext, VariableSimulation? vResult)
        {
            if (vResult == null ||
                !rouMethod.Features.Contains(Feature.SuccessReplace) ||
                rouMethod.MethodContextOmits.Contains(Omit.ReturnValue) ||
                rouMethod.SkipRefStruct && vResult.IsRefStruct()) return [];

            // .if (context.ReturnValueReplaced)
            return vContext.Value.P_ReturnValueReplaced.If(anchor =>
            {
                // .result = context.ReturnValue;
                return vResult.Assign(vContext.Value.P_ReturnValue);
            });
        }

        private IList<Instruction> SyncMosOn(RouMethod rouMethod, TsWeavingTarget tWeavingTarget, VariableSimulation<TsMethodContext> vContext, VariableSimulation<TsMo>[] vMos, Feature feature, Func<TsMo, MethodSimulation> methodFactory)
        {
            return SyncMosOn(rouMethod, tWeavingTarget.M_Proxy, vContext, vMos.Select(x => x.Value).ToArray(), feature, methodFactory);
        }

        private IList<Instruction> SyncMosOn(RouMethod rouMethod, MethodSimulation mHost, IParameterSimulation pContext, TsMo[] tMos, Feature feature, Func<TsMo, MethodSimulation> methodFactory)
        {
            if (!rouMethod.Features.Contains(feature)) return [];

            var instructions = new List<Instruction>();

            var reverseCall = feature != Feature.OnEntry && Configuration.ReverseCallNonEntry;

            for (var i = 0; i < rouMethod.Mos.Length; i++)
            {
                var j = reverseCall ? rouMethod.Mos.Length - i - 1 : i;
                var mo = rouMethod.Mos[j];
                if (!mo.Features.Contains(feature)) continue;

                // .mo.OnXxx(context);
                instructions.Add(methodFactory(tMos[j]).Call(mHost, pContext));
            }

            return instructions;
        }

        private CustomAttribute? SyncClearCustomAttributes(MethodDefinition methodDef)
        {
            CustomAttribute? asyncStateMachineAttribute = null;
            foreach (var attribute in methodDef.CustomAttributes)
            {
                if (attribute.Is(Constants.TYPE_AsyncStateMachineAttribute))
                {
                    if (!methodDef.ReturnType.IsVoid()) throw new FodyWeavingException("Found AsyncStateMachineAttribute but the return type is not void", methodDef);

                    WriteWarning($"[{methodDef}] The async void method may not be able to execute OnSuccess and OnExit at the right time, and may not be able to execute OnException when an exception occurs.", methodDef);
                    asyncStateMachineAttribute = attribute;
                }
            }
            methodDef.CustomAttributes.Clear();

            return asyncStateMachineAttribute;
        }
    }
}
