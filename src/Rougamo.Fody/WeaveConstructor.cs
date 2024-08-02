using Mono.Cecil;
using Mono.Cecil.Cil;
using Rougamo.Fody.Enhances.Sync;
using Rougamo.Fody.Simulations;
using Rougamo.Fody.Simulations.Types;
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

            tWeavingTarget.M_Actual.Def.Body.OptimizePlus(EmptyInstructions);
        }

        private void CtorBuildMethodInternal(RouMethod rouMethod, TsWeavingTarget tWeavingTarget)
        {
            var instructions = rouMethod.MethodDef.Body.Instructions;

            var firstInstruction = LocateAfterSelfOrBaseCtorCalling(rouMethod.MethodDef);
            var returns = instructions.Where(x => x.OpCode.Code == Code.Ret).ToArray();

            var context = new CtorContext();

            var args = tWeavingTarget.M_Proxy.Def.Parameters.Select(x => x.Simulate(this)).ToArray();
            var vException = rouMethod.Features.HasIntersection(Feature.OnException | Feature.OnExit) ? tWeavingTarget.M_Proxy.CreateVariable(_typeExceptionRef) : null;
            var vContext = tWeavingTarget.M_Proxy.CreateVariable<TsMethodContext>(_typeMethodContextRef);
            VariableSimulation<TsMo>[]? vMos = null;
            VariableSimulation<TsArray<TsMo>>? vMoArray = null;
            if (rouMethod.Mos.Length > _config.MoArrayThreshold)
            {
                vMoArray = tWeavingTarget.M_Proxy.CreateVariable<TsArray<TsMo>>(_typeIMoArrayRef);
            }
            else
            {
                vMos = rouMethod.Mos.Select(x => tWeavingTarget.M_Proxy.CreateVariable<TsMo>(x.MoTypeRef)).ToArray();
            }

            Instruction? tryStart = null, catchStart = null, catchEnd = Create(OpCodes.Nop);

            // .var mo = new Mo1Attributue(...);
            instructions.InsertBefore(firstInstruction, SyncInitMos(rouMethod, tWeavingTarget, vMos, vMoArray));
            // .var context = new MethodContext(...);
            instructions.InsertBefore(firstInstruction, SyncInitMethodContext(rouMethod, tWeavingTarget, args, vContext, vMos, vMoArray));
            // .mo.OnEntry(context);
            instructions.InsertBefore(firstInstruction, SyncMosOn(rouMethod, tWeavingTarget, vContext, vMos, vMoArray, Feature.OnEntry, mo => mo.M_OnEntry));
            // .if (context.ReturnValueReplaced) { .. }
            instructions.InsertBefore(firstInstruction, SyncIfOnEntryReplacedReturn(rouMethod, tWeavingTarget, vContext, vMos, vMoArray, null));
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
                instructions.Add(SyncMosOn(rouMethod, tWeavingTarget, vContext, vMos, vMoArray, Feature.OnException, mo => mo.M_OnException));
                // .arg_i = context.Arguments[i];
                instructions.Add(SyncRefreshArguments(rouMethod, tWeavingTarget, args, vContext, Feature.OnException, false));
                // .if (context.RetryCount > 0) goto RETRY;
                instructions.Add(SynCheckRetry(rouMethod, tWeavingTarget, vContext, context, Feature.OnException, true));
                // .mo.OnExit(context);
                instructions.Add(SyncMosOn(rouMethod, tWeavingTarget, vContext, vMos, vMoArray, Feature.OnExit, mo => mo.M_OnExit));
                // .if (context.ExceptionHandled) return;
                instructions.Add(SyncReturnIfExceptionHandled(rouMethod, vContext, context));
                instructions.Add(Create(OpCodes.Rethrow));

                instructions.Add(catchEnd);
            }

            // .mo.OnSuccess(context);
            instructions.Add(SyncMosOn(rouMethod, tWeavingTarget, vContext, vMos, vMoArray, Feature.OnSuccess, mo => mo.M_OnSuccess));
            // .arg_i = context.Arguments[i];
            instructions.Add(SyncRefreshArguments(rouMethod, tWeavingTarget, args, vContext, Feature.OnSuccess, false));
            // .if (context.RetryCount > 0) goto RETRY;
            instructions.Add(SynCheckRetry(rouMethod, tWeavingTarget, vContext, context, Feature.OnSuccess, false));
            // .mo.OnExit(context);
            instructions.Add(SyncMosOn(rouMethod, tWeavingTarget, vContext, vMos, vMoArray, Feature.OnExit, mo => mo.M_OnExit));
            // .return;
            instructions.Add(context.AnchorReturnResult.Set(OpCodes.Ret));

            SetTryCatch(tWeavingTarget.M_Proxy.Def, tryStart, catchStart, catchEnd);

            if (vException != null)
            {
                foreach (var ret in returns)
                {
                    ret.Set(OpCodes.Leave, catchEnd);
                }
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

            throw new RougamoException("Cannot find the self or base's constructor calling instruction. ", methodDef);
        }
    }
}
