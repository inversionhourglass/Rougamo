using Mono.Cecil;
using Mono.Cecil.Cil;
using Rougamo.Fody.Enhances;
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

        private IteratorFields IteratorResolveFields(RouMethod rouMethod, TypeDefinition stateMachineTypeDef)
        {
            var getEnumeratorMethodDef = stateMachineTypeDef.Methods.Single(x => x.Name.StartsWith(Constants.GenericPrefix(Constants.TYPE_IEnumerable)) && x.Name.EndsWith(Constants.GenericSuffix(Constants.METHOD_GetEnumerator)));

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
            var current = stateMachineTypeDef.Fields.Single(m => m.Name.EndsWith(Constants.FIELD_Current_Suffix));
            var initialThreadId = stateMachineTypeDef.Fields.Single(x => x.Name == Constants.FIELD_InitialThreadId);
            var declaringThis = stateMachineTypeDef.Fields.SingleOrDefault(x => x.FieldType.Resolve() == stateMachineTypeDef.DeclaringType);
            FieldDefinition? recordedReturn = null;
            var transitParameters = StateMachineParameterFields(rouMethod);
            var parameters = IteratorGetPrivateParameterFields(getEnumeratorMethodDef, transitParameters);
            if (_config.RecordingIteratorReturns && rouMethod.Features.HasIntersection(Feature.OnSuccess | Feature.OnExit) && !rouMethod.MethodContextOmits.Contains(Omit.ReturnValue))
            {
                var listReturnsRef = new GenericInstanceType(_typeListRef);
                listReturnsRef.GenericArguments.Add(((GenericInstanceType)rouMethod.MethodDef.ReturnType).GenericArguments[0]);
                recordedReturn = new FieldDefinition(Constants.FIELD_IteratorReturnList, FieldAttributes.Public, listReturnsRef);
                stateMachineTypeDef.AddUniqueField(recordedReturn);
            }

            var enumerableTypeRef = rouMethod.MethodDef.ReturnType;
            var enumeratorTypeRef = enumerableTypeRef.GetMethod(Constants.METHOD_GetEnumerator, false).ReturnType;
            enumeratorTypeRef = stateMachineTypeDef.Import(enumerableTypeRef, enumeratorTypeRef);
            var iterator = new FieldDefinition(Constants.FIELD_Iterator, FieldAttributes.Private, enumeratorTypeRef);

            stateMachineTypeDef.AddUniqueField(methodContext);
            stateMachineTypeDef.AddUniqueField(iterator);

            return new IteratorFields(stateMachineTypeDef, moArray, mos, methodContext, state, current, initialThreadId, recordedReturn, declaringThis, iterator, transitParameters, parameters);
        }

        private FieldDefinition?[] IteratorGetPrivateParameterFields(MethodDefinition getEnumeratorMethodDef, FieldDefinition?[] transitParameterFieldDefs)
        {
            if (transitParameterFieldDefs.Length == 0) return [];

            var parameters = new FieldDefinition?[transitParameterFieldDefs.Length];
            var map = new Dictionary<FieldDefinition, int>();
            for (var i = 0; i < transitParameterFieldDefs.Length; i++)
            {
                var parameterFieldDef = transitParameterFieldDefs[i];
                if (parameterFieldDef == null) continue;

                map.Add(parameterFieldDef, i);
            }

            foreach (var instruction in getEnumeratorMethodDef.Body.Instructions)
            {
                if (instruction.OpCode.Code == Code.Ldfld && map.TryGetValue(((FieldReference)instruction.Operand).Resolve(), out var index))
                {
                    parameters[index] = ((FieldReference)instruction.Next.Operand).Resolve();
                }
            }

            return parameters;
        }

        private void ProxyIteratorSetAbsentFields(RouMethod rouMethod, TypeDefinition stateMachineTypeDef, IIteratorFields fields, string mGetEnumeratorPrefix, string mGetEnumeratorSuffix)
        {
            var instructions = rouMethod.MethodDef.Body.Instructions;
            var vStateMachine = rouMethod.MethodDef.Body.Variables.SingleOrDefault(x => x.VariableType.Resolve() == stateMachineTypeDef);
            var loadStateMachine = vStateMachine == null ? Create(OpCodes.Dup) : vStateMachine.LdlocAny();
            var stateMachineTypeRef = vStateMachine == null ? ProxyIteratorResolveStateMachineType(stateMachineTypeDef, rouMethod.MethodDef) : vStateMachine.VariableType;
            var ret = instructions.Last();
            var genericMap = stateMachineTypeDef.GenericParameters.ToDictionary(x => x.Name, x => (TypeReference)x);

            var getEnumeratorMethodDef = stateMachineTypeDef.Methods.Single(x => x.Name.StartsWith(mGetEnumeratorPrefix) && x.Name.EndsWith(mGetEnumeratorSuffix));

            ProxyAddAbsentField(stateMachineTypeDef, ProxySetAbsentFieldThis(rouMethod, fields, stateMachineTypeRef, loadStateMachine, ret, genericMap));

            ProxyAddAbsentField(stateMachineTypeDef, ProxySetAbsentFieldParameters(rouMethod, fields, stateMachineTypeRef, loadStateMachine, ret, genericMap));

            ProxyIteratorSetAbsentFieldThis(stateMachineTypeDef, getEnumeratorMethodDef, fields);

            ProxyAddAbsentField(stateMachineTypeDef, ProxyIteratorSetAbsentFieldParameters(getEnumeratorMethodDef, fields));
        }

        private TypeReference ProxyIteratorResolveStateMachineType(TypeDefinition stateMachineTypeDef, MethodDefinition methodDef)
        {
            foreach (var instruction in methodDef.Body.Instructions)
            {
                if (instruction.OpCode.Code == Code.Newobj)
                {
                    if (instruction.Operand is MethodReference mr && mr.Resolve().IsConstructor && mr.DeclaringType.Resolve() == stateMachineTypeDef) return mr.DeclaringType;
                }
                else if (instruction.OpCode.Code == Code.Initobj)
                {
                    if (instruction.Operand is TypeReference tr && tr.Resolve() == stateMachineTypeDef) return tr;
                }
            }

            throw new RougamoException($"Cannot find {stateMachineTypeDef} init instruction from {methodDef}");
        }

        private void ProxyIteratorSetAbsentFieldThis(TypeDefinition stateMachineTypeDef, MethodDefinition getEnumeratorMethodDef, IIteratorFields fields)
        {
            if (fields.DeclaringThis == null) return;

            var instructions = getEnumeratorMethodDef.Body.Instructions;

            Instruction? stloc = null;
            foreach (var instruction in instructions)
            {
                if (instruction.OpCode.Code == Code.Newobj && instruction.Operand is MethodReference mr && mr.Resolve().IsConstructor && mr.DeclaringType.Resolve() == stateMachineTypeDef)
                {
                    stloc = instruction.Next;
                }
                else if (instruction.OpCode.Code == Code.Ldfld && instruction.Operand is FieldReference fr && fr.Resolve() == fields.DeclaringThis.Resolve())
                {
                    return;
                }
            }

            if (stloc == null) throw new RougamoException($"Cannot find new operation of {stateMachineTypeDef} in method {getEnumeratorMethodDef}");

            var vStateMachine = stloc.ResolveVariable(getEnumeratorMethodDef);

            instructions.InsertAfter(stloc, [
                Create(OpCodes.Ldloc, vStateMachine),
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.DeclaringThis),
                Create(OpCodes.Stfld, new FieldReference(fields.DeclaringThis!.Name, fields.DeclaringThis!.FieldType, vStateMachine.VariableType))
            ]);
        }

        private IEnumerable<FieldDefinition> ProxyIteratorSetAbsentFieldParameters(MethodDefinition getEnumeratorMethodDef, IIteratorFields fields)
        {
            if (fields.TransitParameters.All(x => x != null)) yield break;

            var instructions = getEnumeratorMethodDef.Body.Instructions;

            var ldloc = instructions.Last().Previous;

            var vStateMachine = ldloc.ResolveVariable(getEnumeratorMethodDef);

            for (var i = fields.TransitParameters.Length - 1; i >= 0; i--)
            {
                if (fields.TransitParameters[i] != null) continue;

                fields.TransitParameters[i] = fields.Parameters[i];

                var transitParameterFieldRef = fields.TransitParameters[i]!;
                var parameterFieldDef = new FieldDefinition($">_<{transitParameterFieldRef.Name}", FieldAttributes.Private, transitParameterFieldRef.FieldType);
                var parameterFieldRef = new FieldReference(parameterFieldDef.Name, parameterFieldDef.FieldType, vStateMachine.VariableType);

                fields.SetParameter(i, parameterFieldDef);

                instructions.InsertAfter(ldloc, [
                    Create(OpCodes.Ldarg_0),
                    Create(OpCodes.Ldfld, transitParameterFieldRef),
                    Create(OpCodes.Stfld, parameterFieldRef),
                    Create(OpCodes.Ldloc, vStateMachine)
                ]);

                yield return parameterFieldDef;
            }
        }
    }
}
