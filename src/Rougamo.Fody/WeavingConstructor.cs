using Fody;
using Fody.Simulations;
using Fody.Simulations.Types;
using Mono.Cecil;
using Mono.Cecil.Cil;
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
        private void WeavingConstructor(RouMethod rouMethod)
        {
            var tWeavingTarget = rouMethod.MethodDef.DeclaringType.Simulate<TsWeavingTarget>(this).SetMethod(rouMethod.MethodDef);

            CtorBuildMethod(rouMethod, tWeavingTarget);
        }

        private void CtorBuildMethod(RouMethod rouMethod, TsWeavingTarget tWeavingTarget)
        {
            CtorBuildMethodInternal(rouMethod, tWeavingTarget);

            tWeavingTarget.M_Actual.Def.Body.InitLocals = true;
            tWeavingTarget.M_Actual.Def.Body.OptimizePlus();
        }

        private void CtorBuildMethodInternal(RouMethod rouMethod, TsWeavingTarget tWeavingTarget)
        {
            var instructions = rouMethod.MethodDef.Body.Instructions;

            var firstInstruction = LocateAfterSelfOrBaseCtorCalling(rouMethod.MethodDef);
            var returns = instructions.Where(x => x.OpCode.Code == Code.Ret).ToArray();

            var context = new SyncContext();

            var args = tWeavingTarget.M_Proxy.Def.Parameters.Select(x => x.Simulate(this)).ToArray();
            var vException = rouMethod.Features.HasIntersection(Feature.OnException | Feature.OnExit) ? tWeavingTarget.M_Proxy.CreateVariable(_tExceptionRef) : null;
            var vContext = tWeavingTarget.M_Proxy.CreateVariable<TsMethodContext>(_tMethodContextRef);
            var vMos = rouMethod.Mos.Select(x => tWeavingTarget.M_Proxy.CreateVariable<TsMo>(x.MoTypeRef)).ToArray();

            Instruction poolTryStart = Create(OpCodes.Nop), poolFinallyStart = Create(OpCodes.Nop), poolFinallyEnd = Create(OpCodes.Nop);
            Instruction? tryStart = null, catchStart = null, catchEnd = Create(OpCodes.Nop);

            var pooledItems = new List<IParameterSimulation> { vContext };

            // .var mo = new Mo1Attributue(...);
            instructions.InsertBefore(firstInstruction, SyncInitMos(rouMethod, tWeavingTarget, vMos, pooledItems));
            // .var context = new MethodContext(...);
            instructions.InsertBefore(firstInstruction, SyncInitMethodContext(rouMethod, tWeavingTarget, args, vContext, vMos));

            instructions.InsertBefore(firstInstruction, poolTryStart);
            // .try
            {
                // .mo.OnEntry(context);
                instructions.InsertBefore(firstInstruction, SyncMosOn(rouMethod, tWeavingTarget, vContext, vMos, Feature.OnEntry, mo => mo.M_OnEntry));
                // .if (context.ReturnValueReplaced) { .. }
                instructions.InsertBefore(firstInstruction, SyncIfOnEntryReplacedReturn(rouMethod, tWeavingTarget, vContext, vMos, null, poolFinallyEnd));
                // .if (_context.RewriteArguments) { ... }
                instructions.InsertBefore(firstInstruction, SyncIfRewriteArguments(rouMethod, tWeavingTarget, args, vContext));

                // .RETRY:
                instructions.InsertBefore(firstInstruction, context.AnchorRetry);
                // .try
                {
                    tryStart = firstInstruction;

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
                    instructions.Add(SyncRefreshArguments(rouMethod, tWeavingTarget, args, vContext, Feature.OnException, false));
                    // .if (context.RetryCount > 0) goto RETRY;
                    instructions.Add(SynCheckRetry(rouMethod, tWeavingTarget, vContext, context, Feature.OnException, true));
                    // .mo.OnExit(context);
                    instructions.Add(SyncMosOn(rouMethod, tWeavingTarget, vContext, vMos, Feature.OnExit, mo => mo.M_OnExit));
                    // .if (context.ExceptionHandled) return;
                    instructions.Add(SyncReturnIfExceptionHandled(rouMethod, vContext, context));
                    instructions.Add(Create(OpCodes.Rethrow));
                }
                instructions.Add(catchEnd);

                // .mo.OnSuccess(context);
                instructions.Add(SyncMosOn(rouMethod, tWeavingTarget, vContext, vMos, Feature.OnSuccess, mo => mo.M_OnSuccess));
                // .arg_i = context.Arguments[i];
                instructions.Add(SyncRefreshArguments(rouMethod, tWeavingTarget, args, vContext, Feature.OnSuccess, false));
                // .if (context.RetryCount > 0) goto RETRY;
                instructions.Add(SynCheckRetry(rouMethod, tWeavingTarget, vContext, context, Feature.OnSuccess, false));
                // .mo.OnExit(context);
                instructions.Add(SyncMosOn(rouMethod, tWeavingTarget, vContext, vMos, Feature.OnExit, mo => mo.M_OnExit));

                // .return;
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
            // .return;
            instructions.Add(Create(OpCodes.Ret));

            SetTryCatch(tWeavingTarget.M_Proxy.Def, tryStart, catchStart, catchEnd);
            SetTryFinally(tWeavingTarget.M_Proxy.Def, poolTryStart, poolFinallyStart, poolFinallyEnd);

            var opCode = vException == null ? OpCodes.Br : OpCodes.Leave;
            foreach (var ret in returns)
            {
                ret.Set(opCode, catchEnd);
            }
        }

        private Instruction LocateAfterSelfOrBaseCtorCalling(MethodDefinition methodDef)
        {
            if (methodDef.IsStatic) return methodDef.Body.Instructions.First();

            var baseType = methodDef.DeclaringType.BaseType;
            foreach (var instruction in methodDef.Body.Instructions)
            {
                if (instruction.OpCode.Code == Code.Call &&
                    instruction.Operand is MethodReference mr && mr.Resolve().IsConstructor &&
                    (mr.DeclaringType == methodDef.DeclaringType || mr.DeclaringType == baseType))
                {
                    return instruction.Next;
                }
            }

            throw new FodyWeavingException("Cannot find the self or base's constructor calling instruction. ", methodDef);
        }
    }
}
