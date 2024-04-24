using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Rougamo.Fody.Enhances.Async;
using Rougamo.Fody.Enhances.Iterator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            StrictIteratorProxyCall(rouMethod, stateMachineTypeDef, moveNextDef, actualMethodDef);
        }

        private void StrictIteratorProxyCall(RouMethod rouMethod, TypeDefinition proxyStateMachineTypeDef, MethodDefinition proxyMoveNextDef, MethodDefinition actualMethodDef)
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
            StrictIteratorSetAbsentFields(rouMethod, proxyStateMachineTypeDef, fields);
            StrictFieldCleanup(proxyStateMachineTypeDef, fields);

            var fIterator = new FieldDefinition(Constants.FIELD_Iterator, FieldAttributes.Private, enumeratorTypeRef);
            proxyStateMachineTypeDef.Fields.Add(fIterator);
            fields.Iterator = fIterator;

            var vState = proxyMoveNextDef.Body.CreateVariable(_typeIntRef);

            var instructions = proxyMoveNextDef.Body.Instructions;

            var nopStateIs0 = Create(OpCodes.Nop);
            var nopStateIs1 = Create(OpCodes.Nop);
            var nopHasNext = Create(OpCodes.Nop);

            instructions.Add(Create(OpCodes.Ldarg_0));
            instructions.Add(Create(OpCodes.Ldfld, fields.State));
            instructions.Add(Create(OpCodes.Stloc, vState));

            instructions.Add(Create(OpCodes.Ldloc, vState));
            instructions.Add(Create(OpCodes.Brfalse, nopStateIs0));
            // -if (state != 0)
            {
                instructions.Add(Create(OpCodes.Ldloc, vState));
                instructions.Add(Create(OpCodes.Ldc_I4_1));
                instructions.Add(Create(OpCodes.Beq, nopStateIs1));
                // -if (state != 1)
                {
                    // return false;
                    instructions.Add(Create(OpCodes.Ldc_I4_0));
                    instructions.Add(Create(OpCodes.Ret));
                }
            }

            // else (state == 0)
            {
                instructions.Add(nopStateIs0.Set(OpCodes.Ldarg_0));
                // _iterator = ActualMethod(x, y, z).GetEnumerator();
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

            instructions.Add(nopStateIs1.Set(OpCodes.Ldarg_0));
            instructions.Add(Create(OpCodes.Ldfld, fields.Iterator));
            instructions.Add(Create(OpCodes.Callvirt, _methodIEnumeratorMoveNextRef));
            instructions.Add(Create(OpCodes.Brtrue, nopHasNext));
            // -if (!_iterator.MoveNext())
            {
                // _state = -1;
                instructions.Add(Create(OpCodes.Ldarg_0));
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
            instructions.Add(Create(OpCodes.Ldarg_0));
            instructions.Add(Create(OpCodes.Ldc_I4_1));
            instructions.Add(Create(OpCodes.Stfld, fields.State));

            // return true;
            instructions.Add(Create(OpCodes.Ldc_I4_1));
            instructions.Add(Create(OpCodes.Ret));
        }

        private void StrictIteratorSetAbsentFields(RouMethod rouMethod, TypeDefinition stateMachineTypeDef, IteratorFields fields)
        {
            var instructions = rouMethod.MethodDef.Body.Instructions;
            var vStateMachine = rouMethod.MethodDef.Body.Variables.SingleOrDefault(x => x.VariableType.Resolve() == stateMachineTypeDef);
            var loadStateMachine = vStateMachine == null ? Create(OpCodes.Dup) : vStateMachine.LdlocOrA();
            var stateMachineTypeRef = vStateMachine == null ? StrictIteratorResolveStateMachineType(stateMachineTypeDef, rouMethod.MethodDef) : vStateMachine.VariableType;
            var ret = instructions.Last();
            var genericMap = stateMachineTypeDef.GenericParameters.ToDictionary(x => x.Name, x => x);
            
            var getEnumeratorMethodDef = stateMachineTypeDef.Methods.Single(x => x.Name.StartsWith(Constants.METHOD_GetEnumerator_Prefix) && x.Name.EndsWith(Constants.METHOD_GetEnumerator_Suffix));

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

        private void StrictIteratorSetAbsentFieldThis(TypeDefinition stateMachineTypeDef, MethodDefinition getEnumeratorMethodDef, IteratorFields fields)
        {
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

        private IEnumerable<FieldDefinition> StrictIteratorSetAbsentFieldParameters(MethodDefinition getEnumeratorMethodDef, IteratorFields fields)
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
