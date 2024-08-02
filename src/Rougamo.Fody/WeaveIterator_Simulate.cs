using Mono.Cecil.Cil;
using Rougamo.Fody.Enhances.Iterator;
using Rougamo.Fody.Simulations;
using Rougamo.Fody.Simulations.Operations;
using Rougamo.Fody.Simulations.Types;
using System.Collections.Generic;
using System.Linq;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void WeavingIteratorMethod(RouMethod rouMethod)
        {
            var stateMachineTypeDef = rouMethod.MethodDef.ResolveStateMachine(Constants.TYPE_IteratorStateMachineAttribute);
            var actualStateMachineTypeDef = ProxyStateMachineClone(stateMachineTypeDef);
            if (actualStateMachineTypeDef == null) return;

            var actualMethodDef = ProxyStateMachineSetupMethodClone(rouMethod.MethodDef, stateMachineTypeDef, actualStateMachineTypeDef, Constants.TYPE_IteratorStateMachineAttribute);
            rouMethod.MethodDef.DeclaringType.Methods.Add(actualMethodDef);

            var actualMoveNextDef = actualStateMachineTypeDef.Methods.Single(m => m.Name == Constants.METHOD_MoveNext);
            actualMoveNextDef.DebugInformation.StateMachineKickOffMethod = actualMethodDef;

            var fields = IteratorResolveFields(rouMethod, stateMachineTypeDef);
            ProxyIteratorSetAbsentFields(rouMethod, stateMachineTypeDef, fields, Constants.GenericPrefix(Constants.TYPE_IEnumerable), Constants.GenericSuffix(Constants.METHOD_GetEnumerator));
            ProxyFieldCleanup(stateMachineTypeDef, fields);

            var tStateMachine = stateMachineTypeDef.MakeReference().Simulate<TsIteratorStateMachine>(this).SetFields(fields);
            var mActualMethod = StateMachineResolveActualMethod<TsEnumerable>(tStateMachine, actualMethodDef);
            IteratorBuildMoveNext(rouMethod, tStateMachine, mActualMethod);
        }

        private void IteratorBuildMoveNext(RouMethod rouMethod, TsIteratorStateMachine tStateMachine, MethodSimulation<TsEnumerable> mActualMethod)
        {
            var mMoveNext = tStateMachine.M_MoveNext;

            mMoveNext.Def.DebuggerStepThrough(_methodDebuggerStepThroughCtorRef);
            mMoveNext.Def.Clear();

            IteratorBuildMoveNextInternal(rouMethod, tStateMachine, mActualMethod);

            mMoveNext.Def.Body.InitLocals = true;
            mMoveNext.Def.Body.OptimizePlus(EmptyInstructions);
        }

        private void IteratorBuildMoveNextInternal(RouMethod rouMethod, TsIteratorStateMachine tStateMachine, MethodSimulation<TsEnumerable> mActualMethod)
        {
            var mMoveNext = tStateMachine.M_MoveNext;
            var instructions = mMoveNext.Def.Body.Instructions;

            var context = new IteratorContext();

            var vState = mMoveNext.CreateVariable(_typeIntRef);
            var vHasNext = mMoveNext.CreateVariable(_typeBoolRef);
            var vException = rouMethod.Features.HasIntersection(Feature.OnException | Feature.OnExit) ? mMoveNext.CreateVariable(_typeExceptionRef) : null;

            Instruction? tryStart = null, catchStart = null, catchEnd = Create(OpCodes.Nop);

            // .var state = _state;
            instructions.Add(vState.Assign(tStateMachine.F_State));
            // .if (state == 0)
            instructions.Add(vState.IsEqual(0).If(anchor => [
                // ._items = new List<T>();
                .. IteratorInitItems(rouMethod, tStateMachine),
                // ._mo = new Mo1Attribute(..);
                .. StateMachineInitMos(rouMethod, tStateMachine),
                // ._context = new MethodContext(..);
                .. StateMachineInitMethodContext(rouMethod, tStateMachine),
                // ._mo.OnEntry(_context);
                .. StateMachineSyncMosNo(rouMethod, tStateMachine, Feature.OnEntry, mo => mo.M_OnEntry),
                // .if (_context.RewriteArguments) { .. }
                .. StateMachineIfRewriteArguments(rouMethod, tStateMachine),
                // ._iterator = ActualMethod(x, y, z).GetEnumerator();
                .. tStateMachine.F_Iterator.Assign(target => [
                    .. mActualMethod.Call(tStateMachine.M_MoveNext, tStateMachine.F_Parameters),
                    .. mActualMethod.Result.M_GetEnumerator.Call(tStateMachine.M_MoveNext)
                ]),
                Create(OpCodes.Br, context.AnchorState1)
            ]));
            // .else if (state != 1)
            instructions.Add(vState.IsEqual(1).IfNot(anchor => [
                // return false;
                Create(OpCodes.Ldc_I4_0),
                Create(OpCodes.Ret)
            ]));
            instructions.Add(context.AnchorState1);
            // .try
            {
                // .hasNext = _iterator.MoveNext();
                tryStart = instructions.AddGetFirst(vHasNext.Assign(target => tStateMachine.F_Iterator.Value.M_MoveNext.Call(tStateMachine.M_MoveNext)));
                if (vException != null) instructions.Add(Create(OpCodes.Leave, catchEnd));
            }
            // .catch (Exception ex)
            if (vException != null)
            {
                catchStart = instructions.AddGetFirst(vException.Assign(target => []));
                // ._context.Exception = ex;
                instructions.Add(tStateMachine.F_MethodContext.Value.P_Exception.Assign(vException));
                // ._state = -1;
                instructions.Add(tStateMachine.F_State.Assign(-1));
                // ._mo.OnException(_context);
                instructions.Add(StateMachineSyncMosNo(rouMethod, tStateMachine, Feature.OnException, mo => mo.M_OnException));
                // ._mo.OnExit(_context);
                instructions.Add(StateMachineSyncMosNo(rouMethod, tStateMachine, Feature.OnExit, mo => mo.M_OnExit));
                // throw;
                instructions.Add(Create(OpCodes.Rethrow));

                instructions.Add(catchEnd);
            }
            // .if (hasNext)
            instructions.Add(vHasNext.If(anchor => [
                // ._current = _iterator.Current;
                .. tStateMachine.F_Current.Assign(tStateMachine.F_Iterator.Value.P_Current),
                // ._items.Add(_current);
                .. IteratorSaveItem(rouMethod, tStateMachine),
                // ._state = 1;
                .. tStateMachine.F_State.Assign(1),
                // .return true;
                Create(OpCodes.Ldc_I4_1),
                Create(OpCodes.Ret)
            ]));
            // ._state = -1;
            instructions.Add(tStateMachine.F_State.Assign(-1));
            // ._context.ReturnValue = _items.ToArray();
            instructions.Add(IteratorSaveReturnValue(rouMethod, tStateMachine));
            // ._mo.OnSuccess(_context);
            instructions.Add(StateMachineSyncMosNo(rouMethod, tStateMachine, Feature.OnSuccess, mo => mo.M_OnSuccess));
            // ._mo.OnExit(_context);
            instructions.Add(StateMachineSyncMosNo(rouMethod, tStateMachine, Feature.OnExit, mo => mo.M_OnExit));
            // .return false;
            instructions.Add(Create(OpCodes.Ldc_I4_0));
            instructions.Add(Create(OpCodes.Ret));

            SetTryCatch(tStateMachine.M_MoveNext.Def, tryStart, catchStart, catchEnd);
        }

        private IList<Instruction> IteratorInitItems(RouMethod rouMethod, IIteratorStateMachine tStateMachine)
        {
            if (tStateMachine.F_Items == null) return [];

            // ._items = new List<T>();
            return tStateMachine.F_Items.AssignNew(tStateMachine.M_MoveNext);
        }

        private IList<Instruction> IteratorSaveItem(RouMethod rouMethod, IIteratorStateMachine tStateMachine)
        {
            if (tStateMachine.F_Items == null) return [];

            // ._items.Add(_current);
            return tStateMachine.F_Items.Value.M_Add.Call(tStateMachine.M_MoveNext, tStateMachine.F_Current);
        }

        private IList<Instruction> IteratorSaveReturnValue(RouMethod rouMethod, TsIteratorStateMachine tStateMachine)
        {
            if (tStateMachine.F_Items == null) return [];

            // ._context.ReturnValue = _items.ToArray();
            return tStateMachine.F_MethodContext.Value.P_ReturnValue.Assign(target => tStateMachine.F_Items.Value.M_ToArray.Call(tStateMachine.M_MoveNext));
        }
    }
}
