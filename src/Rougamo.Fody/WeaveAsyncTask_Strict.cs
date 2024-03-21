using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Rougamo.Fody.Enhances;
using Rougamo.Fody.Enhances.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody
{
    partial class ModuleWeaver
    {
        private void StrictAsyncTaskMethodWeave(RouMethod rouMethod, TypeDefinition stateMachineTypeDef)
        {
            var actualStateMachineTypeDef = StrictAsyncStateMachineClone(stateMachineTypeDef);
            if (actualStateMachineTypeDef == null) return;

            var actualMethodDef = StrictAsyncSetupMethodClone(rouMethod.MethodDef, stateMachineTypeDef, actualStateMachineTypeDef);
            rouMethod.MethodDef.DeclaringType.Methods.Add(actualMethodDef);

            var moveNextDef = stateMachineTypeDef.Methods.Single(m => m.Name == Constants.METHOD_MoveNext);
#if DEBUG
            if (rouMethod.MethodDef.FullName.Contains("Strict_"))
#endif
            {
                moveNextDef.Clear();
                StrictAsyncProxyCall(rouMethod, stateMachineTypeDef, moveNextDef, actualMethodDef);
            }
        }

        private void StrictAsyncProxyCall(RouMethod rouMethod, TypeDefinition proxyStateMachineTypeDef, MethodDefinition proxyMoveNextDef, MethodDefinition actualMethodDef)
        {
            var genericMap = proxyStateMachineTypeDef.GenericParameters.ToDictionary(x => x.Name, x => x);

            var proxyStateMachineTypeRef = proxyStateMachineTypeDef.MakeReference();
            var returnTypeRef = actualMethodDef.ReturnType;
            var getAwaiterMethodDef = returnTypeRef.Resolve().Methods.Single(x => x.Name == Constants.METHOD_GetAwaiter);
            var awaiterTypeRef = getAwaiterMethodDef.ReturnType;
            var awaiterTypeDef = awaiterTypeRef.Resolve();
            var isCompletedMethodDef = awaiterTypeDef.Methods.Single(x => x.Name == Constants.Getter(Constants.PROP_IsCompleted));
            var getResultMethodDef = awaiterTypeDef.Methods.Single(x => x.Name == Constants.METHOD_GetResult);
            if (returnTypeRef is GenericInstanceType)
            {
                returnTypeRef = returnTypeRef.ReplaceGenericArgs(genericMap);
                var git = new GenericInstanceType(awaiterTypeRef.GetElementType().ImportInto(ModuleDefinition));
                git.GenericArguments.Add(((GenericInstanceType)returnTypeRef).GenericArguments);
                awaiterTypeRef = git;
            }
            else
            {
                returnTypeRef = returnTypeRef.ImportInto(ModuleDefinition);
                awaiterTypeRef = awaiterTypeRef.ImportInto(ModuleDefinition);
            }
            var getAwaiterMethodRef = getAwaiterMethodDef.WithGenericDeclaringType(returnTypeRef);
            var isCompletedMethodRef = isCompletedMethodDef.WithGenericDeclaringType(awaiterTypeRef);
            var getResultMethodRef = getResultMethodDef.WithGenericDeclaringType(awaiterTypeRef);
            var declaringTypeRef = actualMethodDef.DeclaringType.MakeReference().ReplaceGenericArgs(genericMap);
            var actualMethodRef = actualMethodDef.WithGenericDeclaringType(declaringTypeRef);
            if (proxyStateMachineTypeDef.HasGenericParameters)
            {
                var parentGenericNames = proxyStateMachineTypeDef.DeclaringType.GenericParameters.Select(x => x.Name).ToArray();
                var generics = proxyStateMachineTypeDef.GenericParameters.Where(x => !parentGenericNames.Contains(x.Name)).ToArray();
                actualMethodRef = actualMethodRef.WithGenerics(generics);
            }

            var fields = AsyncResolveFields(rouMethod, proxyStateMachineTypeDef);
            StrictSetAbsentFields(rouMethod, proxyStateMachineTypeDef, fields);

            var builderTypeRef = fields.Builder.FieldType;
            var builderTypeDef = builderTypeRef.Resolve();
            var awaitUnsafeOnCompletedMethodDef = builderTypeDef.Methods.Single(x => x.Name == Constants.METHOD_AwaitUnsafeOnCompleted && x.IsPublic);
            var awaitUnsafeOnCompletedMethodRef = awaitUnsafeOnCompletedMethodDef.WithGenericDeclaringType(builderTypeRef).WithGenerics(awaiterTypeRef, proxyStateMachineTypeRef);
            var setExceptionMethodRef = builderTypeDef.Methods.Single(x => x.Name == Constants.METHOD_SetException && x.IsPublic).WithGenericDeclaringType(builderTypeRef);
            var setResultMethodRef = builderTypeDef.Methods.Single(x => x.Name == Constants.METHOD_SetResult && x.IsPublic).WithGenericDeclaringType(builderTypeRef);

            StrictAsyncFieldCleanup(proxyStateMachineTypeDef, fields);
            var fAwaiter = new FieldDefinition(Constants.FIELD_Awaiter, FieldAttributes.Private, awaiterTypeRef);
            proxyStateMachineTypeDef.Fields.Add(fAwaiter);
            fields.Awaiter = fAwaiter;

            var vState = proxyMoveNextDef.Body.CreateVariable(_typeIntRef);
            var vAwaiter = proxyMoveNextDef.Body.CreateVariable(awaiterTypeRef);
            var vException = proxyMoveNextDef.Body.CreateVariable(_typeExceptionRef);
            VariableDefinition? vThis = null;
            VariableDefinition? vTargetReturn = null;
            VariableDefinition? vResult = null;
            if (!proxyStateMachineTypeDef.IsValueType)
            {
                vThis = proxyMoveNextDef.Body.CreateVariable(proxyStateMachineTypeRef);
            }
            if (returnTypeRef.IsValueType)
            {
                vTargetReturn = proxyMoveNextDef.Body.CreateVariable(returnTypeRef);
            }
            if (awaiterTypeRef is GenericInstanceType gitt)
            {
                vResult = proxyMoveNextDef.Body.CreateVariable(gitt.GenericArguments.Single());
            }

            var builderIsValueType = fields.Builder.FieldType.IsValueType;
            var opBuilderLdfld = builderIsValueType ? OpCodes.Ldflda : OpCodes.Ldfld;
            var opBuilderCall = builderIsValueType ? OpCodes.Call : OpCodes.Callvirt;
            var awaiterIsValueType = awaiterTypeRef.IsValueType;
            var opAwaiterLdloc = awaiterIsValueType ? OpCodes.Ldloca : OpCodes.Ldloc;
            var opAwaiterCall = awaiterIsValueType ? OpCodes.Call : OpCodes.Callvirt;
            var opGetAwaiterCall = returnTypeRef.IsValueType ? OpCodes.Call : OpCodes.Callvirt;

            var instructions = proxyMoveNextDef.Body.Instructions;

            var nopIfStateEqZeroStart = Create(OpCodes.Nop);
            var nopIfStateEqZeroEnd = Create(OpCodes.Nop);
            var nopTryStart = Create(OpCodes.Nop);
            var nopCatchStart = Create(OpCodes.Nop);
            var nopCatchEnd = Create(OpCodes.Nop);
            var ret = Create(OpCodes.Ret);

            // var state = this._state;
            instructions.Add(Create(OpCodes.Ldarg_0));
            instructions.Add(Create(OpCodes.Ldfld, fields.State));
            instructions.Add(Create(OpCodes.Stloc, vState));
            // -try
            {
                instructions.Add(nopTryStart.Set(OpCodes.Ldloc, vState));
                instructions.Add(Create(OpCodes.Brfalse, nopIfStateEqZeroStart));
                // -if (state != 0)
                {
                    // var awaiter = ActualMethod(x, y, z).GetAwaiter();
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
                    if (vTargetReturn != null)
                    {// value type result has to use address to call
                        instructions.Add(Create(OpCodes.Stloc, vTargetReturn));
                        instructions.Add(Create(OpCodes.Ldloca, vTargetReturn));
                    }
                    instructions.Add(Create(opGetAwaiterCall, getAwaiterMethodRef));
                    instructions.Add(Create(OpCodes.Stloc, vAwaiter));

                    instructions.Add(Create(opAwaiterLdloc, vAwaiter));
                    instructions.Add(Create(opAwaiterCall, isCompletedMethodRef));
                    instructions.Add(Create(OpCodes.Brtrue, nopIfStateEqZeroEnd));
                    // -if (!awaiter.IsCompleted)
                    {
                        // state = (this._state = 0);
                        instructions.Add(Create(OpCodes.Ldarg_0));
                        instructions.Add(Create(OpCodes.Ldc_I4_0));
                        instructions.Add(Create(OpCodes.Dup));
                        instructions.Add(Create(OpCodes.Stloc, vState));
                        instructions.Add(Create(OpCodes.Stfld, fields.State));

                        // this._awaiter = awaiter;
                        instructions.Add(Create(OpCodes.Ldarg_0));
                        instructions.Add(Create(OpCodes.Ldloc, vAwaiter));
                        instructions.Add(Create(OpCodes.Stfld, fields.Awaiter));

                        if (vThis != null)
                        {// @this = this;
                            instructions.Add(Create(OpCodes.Ldarg_0));
                            instructions.Add(Create(OpCodes.Stloc, vThis));
                        }

                        // this._builder.AwaitUnsafeOnCompleted(ref awaiter, ref @this);
                        instructions.Add(Create(OpCodes.Ldarg_0));
                        instructions.Add(Create(opBuilderLdfld, fields.Builder));
                        instructions.Add(Create(OpCodes.Ldloca, vAwaiter));
                        if (vThis != null)
                        {
                            instructions.Add(Create(OpCodes.Ldloca, vThis));
                        }
                        else
                        {
                            instructions.Add(Create(OpCodes.Ldarg_0));
                        }
                        instructions.Add(Create(opBuilderCall, awaitUnsafeOnCompletedMethodRef));

                        // return;
                        instructions.Add(Create(OpCodes.Leave, ret));
                    }
                }
                // -else
                {
                    // awaiter = this._awaiter;
                    instructions.Add(nopIfStateEqZeroStart.Set(OpCodes.Ldarg_0));
                    instructions.Add(Create(OpCodes.Ldfld, fields.Awaiter));
                    instructions.Add(Create(OpCodes.Stloc, vAwaiter));

                    // this._awaiter = default;
                    instructions.Add(Create(OpCodes.Ldarg_0));
                    if (awaiterTypeRef.IsValueType)
                    {
                        instructions.Add(Create(OpCodes.Ldflda, fields.Awaiter));
                        instructions.Add(Create(OpCodes.Initobj, awaiterTypeRef));
                    }
                    else
                    {
                        instructions.Add(Create(OpCodes.Ldnull));
                        instructions.Add(Create(OpCodes.Stfld));
                    }

                    // state = (this._state = -1);
                    instructions.Add(Create(OpCodes.Ldarg_0));
                    instructions.Add(Create(OpCodes.Ldc_I4_M1));
                    instructions.Add(Create(OpCodes.Dup));
                    instructions.Add(Create(OpCodes.Stloc, vState));
                    instructions.Add(Create(OpCodes.Stfld, fields.State));
                }
                // var result = awaiter.GetResult(); <--> awaiter.GetResult();
                instructions.Add(nopIfStateEqZeroEnd.Set(opAwaiterLdloc, vAwaiter));
                instructions.Add(Create(opAwaiterCall, getResultMethodRef));
                if (vResult != null)
                {
                    instructions.Add(Create(OpCodes.Stloc, vResult));
                }

                instructions.Add(Create(OpCodes.Leave, nopCatchEnd));
            }
            // catch (Exception ex)
            {
                instructions.Add(nopCatchStart.Set(OpCodes.Stloc, vException));

                // this._state = -2;
                instructions.Add(Create(OpCodes.Ldarg_0));
                instructions.Add(Create(OpCodes.Ldc_I4, -2));
                instructions.Add(Create(OpCodes.Stfld, fields.State));

                // this._builder.SetException(ex);
                instructions.Add(Create(OpCodes.Ldarg_0));
                instructions.Add(Create(opBuilderLdfld, fields.Builder));
                instructions.Add(Create(OpCodes.Ldloc, vException));
                instructions.Add(Create(opBuilderCall, setExceptionMethodRef));

                // return;
                instructions.Add(Create(OpCodes.Leave, ret));
            }
            // this._state = -2;
            instructions.Add(nopCatchEnd.Set(OpCodes.Ldarg_0));
            instructions.Add(Create(OpCodes.Ldc_I4, -2));
            instructions.Add(Create(OpCodes.Stfld, fields.State));

            // this._builder.SetResult(result); <--> this._builder.SetResult();
            instructions.Add(Create(OpCodes.Ldarg_0));
            instructions.Add(Create(opBuilderLdfld, fields.Builder));
            if (vResult != null)
            {
                instructions.Add(Create(OpCodes.Ldloc, vResult));
            }
            instructions.Add(Create(opBuilderCall, setResultMethodRef));

            // return;
            instructions.Add(ret);

            proxyMoveNextDef.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
            {
                TryStart = nopTryStart,
                TryEnd = nopCatchStart,
                HandlerStart = nopCatchStart,
                HandlerEnd = nopCatchEnd,
                CatchType = _typeExceptionRef
            });

            proxyMoveNextDef.Body.OptimizePlus(EmptyInstructions);
        }

        private TypeDefinition? StrictAsyncStateMachineClone(TypeDefinition stateMachineTypeDef)
        {
            var typeName = $"$Rougamo_{stateMachineTypeDef.Name}";
            if (stateMachineTypeDef.DeclaringType.NestedTypes.Any(x => x.Name == typeName)) return null;

            var actualTypeDef = stateMachineTypeDef.Clone(typeName);
            actualTypeDef.DeclaringType.NestedTypes.Add(actualTypeDef);

            return actualTypeDef;
        }

        private MethodDefinition StrictAsyncSetupMethodClone(MethodDefinition methodDef, TypeDefinition stateMachineTypeDef, TypeDefinition clonedStateMachineTypeDef)
        {
            var clonedMethodDef = methodDef.Clone($"$Rougamo_{methodDef.Name}");

            StrictAsyncAsyncStateMachineAttributeClone(clonedMethodDef, clonedStateMachineTypeDef);

            var genericMap = methodDef.DeclaringType.GenericParameters.ToDictionary(x => x.Name, x => x);
            genericMap.AddRange(clonedMethodDef.GenericParameters.ToDictionary(x => x.Name, x => x));
            var stateMachineVariableTypeRef = clonedStateMachineTypeDef.MakeReference().ReplaceGenericArgs(genericMap);
            var vStateMachine = clonedMethodDef.Body.Variables.Single(x => x.VariableType.Resolve() == stateMachineTypeDef);
            var index = clonedMethodDef.Body.Variables.IndexOf(vStateMachine);
            clonedMethodDef.Body.Variables.Remove(vStateMachine);
            var vClonedStateMachine = new VariableDefinition(stateMachineVariableTypeRef);
            clonedMethodDef.Body.Variables.Insert(index, vClonedStateMachine);

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

        private void StrictAsyncAsyncStateMachineAttributeClone(MethodDefinition clonedMethodDef, TypeDefinition clonedStateMachineTypeDef)
        {
            var asyncStateMachineAttribute = clonedMethodDef.CustomAttributes.Single(x => x.Is(Constants.TYPE_AsyncStateMachineAttribute));
            clonedMethodDef.CustomAttributes.Remove(asyncStateMachineAttribute);

            asyncStateMachineAttribute = new CustomAttribute(_methodAsyncStateMachineAttributeCtorRef);
            asyncStateMachineAttribute.ConstructorArguments.Add(new CustomAttributeArgument(_typeSystemRef, clonedStateMachineTypeDef));
            clonedMethodDef.CustomAttributes.Add(asyncStateMachineAttribute);
        }

        private void StrictAsyncFieldCleanup(TypeDefinition stateMachineTypeDef, AsyncFields fields)
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

        private void StrictSetAbsentFields(RouMethod rouMethod, TypeDefinition stateMachineTypeDef, AsyncFields fields)
        {
            var instructions = rouMethod.MethodDef.Body.Instructions;
            var setState = instructions.Single(x => x.OpCode.Code == Code.Ldc_I4_M1 && x.Next.OpCode.Code == Code.Stfld && x.Next.Operand is FieldReference fr && fr.Resolve() == fields.State.Resolve());
            var vStateMachine = rouMethod.MethodDef.Body.Variables.Single(x => x.VariableType.Resolve() == stateMachineTypeDef);
            var genericMap = stateMachineTypeDef.GenericParameters.ToDictionary(x => x.Name, x => x);

            StrictAddAbsentField(stateMachineTypeDef, StrictSetAbsentFieldThis(rouMethod, fields, vStateMachine, setState, genericMap));

            StrictAddAbsentField(stateMachineTypeDef, StrictSetAbsentFieldParameters(rouMethod, fields, vStateMachine, setState, genericMap));
        }

        private IEnumerable<FieldDefinition> StrictSetAbsentFieldThis(RouMethod rouMethod, AsyncFields fields, VariableDefinition vStateMachine, Instruction setState, Dictionary<string, GenericParameter> genericMap)
        {
            if (rouMethod.MethodDef.IsStatic || fields.DeclaringThis != null) yield break;

            var thisTypeRef = vStateMachine.VariableType.DeclaringType.ReplaceGenericArgs(genericMap);
            var thisFieldDef = new FieldDefinition(Constants.FIELD_This, FieldAttributes.Public, thisTypeRef);
            var thisFieldRef = new FieldReference(thisFieldDef.Name, thisFieldDef.FieldType, vStateMachine.VariableType);

            fields.DeclaringThis = thisFieldDef;

            rouMethod.MethodDef.Body.Instructions.InsertBefore(setState, [
                vStateMachine.LdlocOrA(),
                Create(OpCodes.Ldarg_0),
                Create(OpCodes.Stfld, thisFieldRef)
            ]);

            yield return thisFieldDef;
        }

        private IEnumerable<FieldDefinition> StrictSetAbsentFieldParameters(RouMethod rouMethod, AsyncFields fields, VariableDefinition vStateMachine, Instruction setState, Dictionary<string, GenericParameter> genericMap)
        {
            if (fields.Parameters.All(x => x != null)) yield break;

            var instructions = rouMethod.MethodDef.Body.Instructions;
            var parameters = rouMethod.MethodDef.Parameters;

            for (var i = 0; i < fields.Parameters.Length; i++)
            {
                var parameterFieldRef = fields.Parameters[i];
                if (parameterFieldRef != null) continue;

                var parameter = parameters[i];
                var fieldTypeRef = parameter.ParameterType.ReplaceGenericArgs(genericMap);
                var parameterFieldDef = new FieldDefinition(parameter.Name, FieldAttributes.Public, fieldTypeRef);
                parameterFieldRef = new FieldReference(parameterFieldDef.Name, parameterFieldDef.FieldType, vStateMachine.VariableType);
                
                fields.SetParameter(i, parameterFieldDef);

                instructions.InsertBefore(setState, [
                    vStateMachine.LdlocOrA(),
                    Create(OpCodes.Ldarg, parameter),
                    Create(OpCodes.Stfld, parameterFieldRef)
                ]);

                yield return parameterFieldDef;
            }
        }

        private void StrictAddAbsentField(TypeDefinition typeDef, IEnumerable<FieldDefinition> fieldDefs)
        {
            foreach (var fieldDef in fieldDefs)
            {
                typeDef.Fields.Add(fieldDef);
            }
        }
    }
}
