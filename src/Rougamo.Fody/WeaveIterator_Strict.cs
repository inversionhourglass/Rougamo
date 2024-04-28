using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Rougamo.Fody.Enhances;
using Rougamo.Fody.Enhances.Iterator;
using System.Collections.Generic;
using System.Linq;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void StrictIteratorMethodWeave(RouMethod rouMethod, TypeDefinition stateMachineTypeDef)
        {
            var actualStateMachineTypeDef = StrictStateMachineClone(stateMachineTypeDef);
            if (actualStateMachineTypeDef == null) return;

            var actualMethodDef = StrictStateMachineSetupMethodClone(rouMethod.MethodDef, stateMachineTypeDef, actualStateMachineTypeDef, Constants.TYPE_IteratorStateMachineAttribute);
            rouMethod.MethodDef.DeclaringType.Methods.Add(actualMethodDef);

            var moveNextDef = stateMachineTypeDef.Methods.Single(m => m.Name == Constants.METHOD_MoveNext);
            moveNextDef.Clear();
            var context = StrictIteratorProxyCall(rouMethod, stateMachineTypeDef, moveNextDef, actualMethodDef);
            StrictIteratorWeave(rouMethod, moveNextDef, context);

            moveNextDef.Body.OptimizePlus(EmptyInstructions);
        }

        private StrictIteratorContext StrictIteratorProxyCall(RouMethod rouMethod, TypeDefinition proxyStateMachineTypeDef, MethodDefinition proxyMoveNextDef, MethodDefinition actualMethodDef)
        {
            var genericMap = proxyStateMachineTypeDef.GenericParameters.ToDictionary(x => x.Name, x => x);

            var declaringTypeRef = actualMethodDef.DeclaringType.MakeReference().ReplaceGenericArgs(genericMap);
            var returnTypeRef = actualMethodDef.ReturnType.ReplaceGenericArgs(genericMap);
            var getEnumeratorMethodDef = returnTypeRef.Resolve().Methods.Single(x => x.Name == Constants.METHOD_GetEnumerator);
            var getEnumeratorMethodRef = getEnumeratorMethodDef.WithGenericDeclaringType(returnTypeRef);
            var enumeratorTypeDef = getEnumeratorMethodRef.ReturnType.Resolve();
            var enumeratorTypeRef = enumeratorTypeDef.ImportInto(ModuleDefinition).MakeGenericInstanceType(((GenericInstanceType)returnTypeRef).GenericArguments.ToArray());
            var getCurrentMethodRef = enumeratorTypeDef.Methods.Single(x => x.Name == Constants.Getter(Constants.PROP_Current)).WithGenericDeclaringType(enumeratorTypeRef);
            var actualMethodRef = actualMethodDef.WithGenericDeclaringType(declaringTypeRef);
            if (proxyStateMachineTypeDef.HasGenericParameters)
            {
                var parentGenericNames = proxyStateMachineTypeDef.DeclaringType.GenericParameters.Select(x => x.Name).ToArray();
                var generics = proxyStateMachineTypeDef.GenericParameters.Where(x => !parentGenericNames.Contains(x.Name)).ToArray();
                actualMethodRef = actualMethodRef.WithGenerics(generics);
            }

            var fields = IteratorResolveFields(rouMethod, proxyStateMachineTypeDef);
            StrictIteratorSetAbsentFields(rouMethod, proxyStateMachineTypeDef, fields, Constants.GenericPrefix(Constants.TYPE_IEnumerable), Constants.GenericSuffix(Constants.METHOD_GetEnumerator));
            StrictFieldCleanup(proxyStateMachineTypeDef, fields);

            var fIterator = new FieldDefinition(Constants.FIELD_Iterator, FieldAttributes.Private, enumeratorTypeRef);
            proxyStateMachineTypeDef.Fields.Add(fIterator);
            fields.Iterator = fIterator;

            var vState = proxyMoveNextDef.Body.CreateVariable(_typeIntRef);

            var instructions = proxyMoveNextDef.Body.Instructions;

            var anchor = new StrictIteratorAnchors();
            var nopHasNext = Create(OpCodes.Nop);

            instructions.Add(Create(OpCodes.Ldarg_0));
            instructions.Add(Create(OpCodes.Ldfld, fields.State));
            instructions.Add(Create(OpCodes.Stloc, vState));

            instructions.Add(Create(OpCodes.Ldloc, vState));
            instructions.Add(Create(OpCodes.Brfalse, anchor.StateIs0));
            // -if (state != 0)
            {
                instructions.Add(Create(OpCodes.Ldloc, vState));
                instructions.Add(Create(OpCodes.Ldc_I4_1));
                instructions.Add(Create(OpCodes.Beq, anchor.StateIs1));
                // -if (state != 1)
                {
                    // return false;
                    instructions.Add(Create(OpCodes.Ldc_I4_0));
                    instructions.Add(Create(OpCodes.Ret));
                }
            }

            // else (state == 0)
            {
                instructions.Add(anchor.StateIs0);
                // _iterator = ActualMethod(x, y, z).GetEnumerator();
                instructions.Add(Create(OpCodes.Ldarg_0));
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
                instructions.Add(Create(OpCodes.Callvirt, getEnumeratorMethodRef));
                instructions.Add(Create(OpCodes.Stfld, fields.Iterator));
            }

            instructions.Add(anchor.StateIs1.Set(OpCodes.Ldarg_0));
            instructions.Add(Create(OpCodes.Ldfld, fields.Iterator));
            instructions.Add(Create(OpCodes.Callvirt, _methodIEnumeratorMoveNextRef));
            instructions.Add(anchor.HasNextBrTo.Set(OpCodes.Brtrue, nopHasNext));
            // -if (!_iterator.MoveNext())
            {
                // _state = -1;
                instructions.Add(anchor.SetStateToM1.Set(OpCodes.Ldarg_0));
                instructions.Add(Create(OpCodes.Ldc_I4_M1));
                instructions.Add(Create(OpCodes.Stfld, fields.State));

                // return false;
                instructions.Add(Create(OpCodes.Ldc_I4_0));
                instructions.Add(Create(OpCodes.Ret));
            }

            // _current = _iterator.Current;
            instructions.Add(nopHasNext.Set(OpCodes.Ldarg_0));
            instructions.Add(Create(OpCodes.Ldarg_0));
            instructions.Add(Create(OpCodes.Ldfld, fields.Iterator));
            instructions.Add(Create(OpCodes.Callvirt, getCurrentMethodRef));
            instructions.Add(Create(OpCodes.Stfld, fields.Current));

            // _state = 1;
            instructions.Add(anchor.SetStateTo1.Set(OpCodes.Ldarg_0));
            instructions.Add(Create(OpCodes.Ldc_I4_1));
            instructions.Add(Create(OpCodes.Stfld, fields.State));

            // return true;
            instructions.Add(Create(OpCodes.Ldc_I4_1));
            instructions.Add(Create(OpCodes.Ret));

            return new(fields, anchor);
        }

        private void StrictIteratorWeave(RouMethod rouMethod, MethodDefinition proxyMoveNextDef, StrictIteratorContext context)
        {
            var instructions = proxyMoveNextDef.Body.Instructions;

            var vHasNext = proxyMoveNextDef.Body.CreateVariable(_typeBoolRef);
            var vException = proxyMoveNextDef.Body.CreateVariable(_typeExceptionRef);

            var catchStart = Create(OpCodes.Stloc, vException);

            /**
             * _items = new List<>(); // record return value only
             * _mo1 = new Mo1Attribute();
             * _context = new MethodContext(...);
             * if (_context.RewriteArguments)
             * {
             *     arg1 = (int)_context.Argument[0];
             * }
             */
            var anchorStateIs0 = context.Anchors.StateIs0.Next;
            instructions.InsertBefore(anchorStateIs0, StrictIteratorInitRecordedReturn(rouMethod, context.Fields));
            instructions.InsertBefore(anchorStateIs0, StrictStateMachineInitMos(proxyMoveNextDef, rouMethod.Mos, context.Fields));
            instructions.InsertBefore(anchorStateIs0, StrictStateMachineInitMethodContext(rouMethod, proxyMoveNextDef, context.Fields));
            instructions.InsertBefore(anchorStateIs0, StateMachineOnEntry(rouMethod, proxyMoveNextDef, null, context.Fields));
            instructions.InsertBefore(anchorStateIs0, StateMachineRewriteArguments(rouMethod, anchorStateIs0, context.Fields));

            /**
             * try
             * {
             *     hasNext = _iterator.MoveNext();
             * }
             * catch (Exception e)
             * {
             *     _context.Exception = e;
             *     _mo1.OnException(_context);
             *     _mo1.OnExit(_context);
             *     throw;
             * }
             */
            var anchorCheckHasNext = Create(OpCodes.Ldloc, vHasNext);
            var anchorHasNextBrTo = context.Anchors.HasNextBrTo;
            instructions.InsertBefore(anchorHasNextBrTo, Create(OpCodes.Stloc, vHasNext));
            if ((rouMethod.Features & (int)(Feature.OnException | Feature.OnExit)) != 0)
            {
                instructions.InsertBefore(anchorHasNextBrTo, Create(OpCodes.Leave, anchorCheckHasNext));
                instructions.InsertBefore(anchorHasNextBrTo, catchStart);
                instructions.InsertBefore(anchorHasNextBrTo, StrictStateMachineSaveException(rouMethod, context.Fields, vException));
                instructions.InsertBefore(anchorHasNextBrTo, StateMachineOnException(rouMethod, proxyMoveNextDef, null, context.Fields));
                instructions.InsertBefore(anchorHasNextBrTo, StateMachineOnExit(rouMethod, proxyMoveNextDef, null, context.Fields));
                instructions.InsertBefore(anchorHasNextBrTo, Create(OpCodes.Rethrow));
            }
            instructions.InsertBefore(anchorHasNextBrTo, anchorCheckHasNext);

            /**
             * _items.Add(_iterator.Current);
             */
            var anchorSetStateTo1 = context.Anchors.SetStateTo1;
            instructions.InsertBefore(anchorSetStateTo1, IteratorSaveYeildReturn(rouMethod, context.Fields));

            /**
             * _context.ReturnValue = _items;
             * _mo1.OnSuccess(_context);
             * _mo1.OnExit(_context);
             */
            var anchorSetStateToM1 = context.Anchors.SetStateToM1;
            instructions.InsertBefore(anchorSetStateToM1, IteratorSaveReturnValue(rouMethod, context.Fields));
            instructions.InsertBefore(anchorSetStateToM1, StateMachineOnSuccess(rouMethod, proxyMoveNextDef, null, context.Fields));
            instructions.InsertBefore(anchorSetStateToM1, StateMachineOnExit(rouMethod, proxyMoveNextDef, anchorSetStateToM1, context.Fields));

            if ((rouMethod.Features & (int)(Feature.OnException | Feature.OnExit)) != 0)
            {
                proxyMoveNextDef.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
                {
                    TryStart = context.Anchors.StateIs1,
                    TryEnd = catchStart,
                    HandlerStart = catchStart,
                    HandlerEnd = anchorCheckHasNext,
                    CatchType = _typeExceptionRef
                });
            }
        }

        private IList<Instruction>? StrictIteratorInitRecordedReturn(RouMethod rouMethod, IIteratorFields fields)
        {
            if (fields.RecordedReturn == null || (rouMethod.Features & (int)(Feature.OnSuccess | Feature.OnExit)) == 0 || (rouMethod.MethodContextOmits & Omit.ReturnValue) != 0) return null;

            var returnsTypeCtorDef = _typeListDef.GetZeroArgsCtor();
            var returnsTypeCtorRef = returnsTypeCtorDef.WithGenericDeclaringType(fields.RecordedReturn.FieldType);

            return [
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Newobj, returnsTypeCtorRef),
                Create(OpCodes.Stfld, fields.RecordedReturn)
            ];
        }

        private void StrictIteratorSetAbsentFields(RouMethod rouMethod, TypeDefinition stateMachineTypeDef, IIteratorFields fields, string mGetEnumeratorPrefix, string mGetEnumeratorSuffix)
        {
            var instructions = rouMethod.MethodDef.Body.Instructions;
            var vStateMachine = rouMethod.MethodDef.Body.Variables.SingleOrDefault(x => x.VariableType.Resolve() == stateMachineTypeDef);
            var loadStateMachine = vStateMachine == null ? Create(OpCodes.Dup) : vStateMachine.LdlocOrA();
            var stateMachineTypeRef = vStateMachine == null ? StrictIteratorResolveStateMachineType(stateMachineTypeDef, rouMethod.MethodDef) : vStateMachine.VariableType;
            var ret = instructions.Last();
            var genericMap = stateMachineTypeDef.GenericParameters.ToDictionary(x => x.Name, x => x);
            
            var getEnumeratorMethodDef = stateMachineTypeDef.Methods.Single(x => x.Name.StartsWith(mGetEnumeratorPrefix) && x.Name.EndsWith(mGetEnumeratorSuffix));

            StrictAddAbsentField(stateMachineTypeDef, StrictSetAbsentFieldThis(rouMethod, fields, stateMachineTypeRef, loadStateMachine, ret, genericMap));

            StrictAddAbsentField(stateMachineTypeDef, StrictSetAbsentFieldParameters(rouMethod, fields, stateMachineTypeRef, loadStateMachine, ret, genericMap));

            StrictIteratorSetAbsentFieldThis(stateMachineTypeDef, getEnumeratorMethodDef, fields);

            StrictAddAbsentField(stateMachineTypeDef, StrictIteratorSetAbsentFieldParameters(getEnumeratorMethodDef, fields));
        }

        private TypeReference StrictIteratorResolveStateMachineType(TypeDefinition stateMachineTypeDef, MethodDefinition methodDef)
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

        private void StrictIteratorSetAbsentFieldThis(TypeDefinition stateMachineTypeDef, MethodDefinition getEnumeratorMethodDef, IIteratorFields fields)
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
                else if (instruction.OpCode.Code == Code.Ldfld && instruction.Operand is FieldReference fr && fr.Resolve() == fields.DeclaringThis?.Resolve())
                {
                    return;
                }
            }

            if (stloc == null) throw new RougamoException($"Cannot find new operation of {stateMachineTypeDef} in method {getEnumeratorMethodDef}");

            var vStateMachine = stloc.ResolveVariable(getEnumeratorMethodDef);

            instructions.InsertAfter(stloc, [
                Create(OpCodes.Ldloc, vStateMachine),
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Ldfld, fields.DeclaringThis!),
                Create(OpCodes.Stfld, new FieldReference(fields.DeclaringThis!.Name, fields.DeclaringThis!.FieldType, vStateMachine.VariableType))
            ]);
        }

        private IEnumerable<FieldDefinition> StrictIteratorSetAbsentFieldParameters(MethodDefinition getEnumeratorMethodDef, IIteratorFields fields)
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
