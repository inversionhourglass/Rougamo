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
            if (rouMethod.MethodDef.FullName.Contains("AsyncGenericUseCase"))
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
            var getAwaiterMethodRef = getAwaiterMethodDef.ImportInto(ModuleDefinition).WithGenericDeclaringType(returnTypeRef);
            var isCompletedMethodRef = isCompletedMethodDef.ImportInto(ModuleDefinition).WithGenericDeclaringType(awaiterTypeRef);
            var getResultMethodRef = getResultMethodDef.ImportInto(ModuleDefinition).WithGenericDeclaringType(awaiterTypeRef);
            var declaringTypeRef = actualMethodDef.DeclaringType.MakeReference().ReplaceGenericArgs(genericMap);
            var actualMethodRef = actualMethodDef.WithGenericDeclaringType(declaringTypeRef);
            if (proxyStateMachineTypeDef.HasGenericParameters)
            {
                var parentGenericNames = proxyStateMachineTypeDef.DeclaringType.GenericParameters.Select(x => x.Name).ToArray();
                var generics = proxyStateMachineTypeDef.GenericParameters.Where(x => !parentGenericNames.Contains(x.Name)).ToArray();
                actualMethodRef = actualMethodRef.WithGenerics(generics);
            }

            var fields = AsyncResolveFields(rouMethod, proxyStateMachineTypeDef);

            var builderTypeRef = fields.Builder.FieldType;
            var builderTypeDef = builderTypeRef.Resolve();
            var awaitUnsafeOnCompletedMethodDef = builderTypeDef.Methods.Single(x => x.Name == Constants.METHOD_AwaitUnsafeOnCompleted);
            var awaitUnsafeOnCompletedMethodRef = awaitUnsafeOnCompletedMethodDef.ImportInto(ModuleDefinition).WithGenericDeclaringType(builderTypeRef).WithGenerics(awaiterTypeRef, proxyStateMachineTypeRef);
            var setExceptionMethodRef = builderTypeDef.Methods.Single(x => x.Name == Constants.METHOD_SetException).ImportInto(ModuleDefinition).WithGenericDeclaringType(builderTypeRef);
            var setResultMethodRef = builderTypeDef.Methods.Single(x => x.Name == Constants.METHOD_SetResult).ImportInto(ModuleDefinition).WithGenericDeclaringType(builderTypeRef);

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

            instructions.Add(Create(OpCodes.Ldarg_0));
            instructions.Add(Create(OpCodes.Ldfld, fields.State));
            instructions.Add(Create(OpCodes.Stloc, vState));
            // -try
            {
                instructions.Add(nopTryStart.Set(OpCodes.Ldloc, vState));
                instructions.Add(Create(OpCodes.Brfalse, nopIfStateEqZeroStart));
                // -if
                {
                    if (fields.This != null)
                    {
                        instructions.Add(Create(OpCodes.Ldarg_0));
                        instructions.Add(Create(OpCodes.Ldfld, fields.This));
                    }
                    foreach (var parameter in fields.Parameters)
                    {
                        instructions.Add(Create(OpCodes.Ldarg_0));
                        instructions.Add(Create(OpCodes.Ldfld, parameter));
                    }
                    instructions.Add(Create(OpCodes.Call, actualMethodRef));
                    if (vTargetReturn != null)
                    {
                        instructions.Add(Create(OpCodes.Stloc, vTargetReturn));
                        instructions.Add(Create(OpCodes.Ldloca, vTargetReturn));
                    }
                    instructions.Add(Create(opGetAwaiterCall, getAwaiterMethodRef));
                    instructions.Add(Create(OpCodes.Stloc, vAwaiter));

                    instructions.Add(Create(opAwaiterLdloc, vAwaiter));
                    instructions.Add(Create(opAwaiterCall, isCompletedMethodRef));
                    instructions.Add(Create(OpCodes.Brtrue, nopIfStateEqZeroEnd));
                    // -if
                    {
                        instructions.Add(Create(OpCodes.Ldarg_0));
                        instructions.Add(Create(OpCodes.Ldc_I4_0));
                        instructions.Add(Create(OpCodes.Dup));
                        instructions.Add(Create(OpCodes.Stloc, vState));
                        instructions.Add(Create(OpCodes.Stfld, fields.State));

                        instructions.Add(Create(OpCodes.Ldarg_0));
                        instructions.Add(Create(OpCodes.Ldloc, vAwaiter));
                        instructions.Add(Create(OpCodes.Stfld, fields.Awaiter));

                        if (vThis != null)
                        {
                            instructions.Add(Create(OpCodes.Ldarg_0));
                            instructions.Add(Create(OpCodes.Stloc, vThis));
                        }

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

                        instructions.Add(Create(OpCodes.Leave, ret));
                    }
                }
                // -else
                {
                    instructions.Add(nopIfStateEqZeroStart.Set(OpCodes.Ldarg_0));
                    instructions.Add(Create(OpCodes.Ldfld, fields.Awaiter));
                    instructions.Add(Create(OpCodes.Stloc, vAwaiter));

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
                    instructions.Add(Create(OpCodes.Ldarg_0));
                    instructions.Add(Create(OpCodes.Ldc_I4_M1));
                    instructions.Add(Create(OpCodes.Dup));
                    instructions.Add(Create(OpCodes.Stloc, vState));
                    instructions.Add(Create(OpCodes.Stfld, fields.State));
                }
                instructions.Add(nopIfStateEqZeroEnd.Set(opAwaiterLdloc, vAwaiter));
                instructions.Add(Create(opAwaiterCall, getResultMethodRef));
                if (vResult != null)
                {
                    instructions.Add(Create(OpCodes.Stloc, vResult));
                }
                instructions.Add(Create(OpCodes.Leave, nopCatchEnd));
            }
            // catch
            {
                instructions.Add(nopCatchStart.Set(OpCodes.Stloc, vException));

                instructions.Add(Create(OpCodes.Ldarg_0));
                instructions.Add(Create(OpCodes.Ldc_I4, -2));
                instructions.Add(Create(OpCodes.Stfld, fields.State));

                instructions.Add(Create(OpCodes.Ldarg_0));
                instructions.Add(Create(opBuilderLdfld, fields.Builder));
                instructions.Add(Create(OpCodes.Ldloc, vException));
                instructions.Add(Create(opBuilderCall, setExceptionMethodRef));

                instructions.Add(Create(OpCodes.Leave, ret));
            }
            instructions.Add(nopCatchEnd.Set(OpCodes.Ldarg_0));
            instructions.Add(Create(OpCodes.Ldc_I4, -2));
            instructions.Add(Create(OpCodes.Stfld, fields.State));

            instructions.Add(Create(OpCodes.Ldarg_0));
            instructions.Add(Create(opBuilderLdfld, fields.Builder));
            if (vResult != null)
            {
                instructions.Add(Create(OpCodes.Ldloc, vResult));
            }
            instructions.Add(Create(opBuilderCall, setResultMethodRef));

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

            var cloneStateMachineTypeRef = clonedStateMachineTypeDef.MakeReference();
            var stateMachine = clonedMethodDef.Body.Variables.Single(x => x.VariableType.Resolve() == stateMachineTypeDef);
            var index = clonedMethodDef.Body.Variables.IndexOf(stateMachine);
            clonedMethodDef.Body.Variables.Remove(stateMachine);
            var clonedStateMachine = new VariableDefinition(cloneStateMachineTypeRef); //clonedMethodDef.Body.CreateVariable(stateMachineTypeRef);
            clonedMethodDef.Body.Variables.Insert(index, clonedStateMachine);

            var fieldMap = new Dictionary<FieldDefinition, FieldReference>();
            foreach (var fieldDef in stateMachineTypeDef.Fields)
            {
                fieldMap[fieldDef] = new FieldReference(fieldDef.Name, fieldDef.FieldType, cloneStateMachineTypeRef);
            }

            foreach (var instruction in clonedMethodDef.Body.Instructions)
            {
                if (instruction.Operand == null) continue;

                if (instruction.Operand is MethodReference methodRef)
                {
                    if (methodRef.Resolve().IsConstructor && methodRef.DeclaringType.Resolve() == stateMachineTypeDef)
                    {
                        var stateMachineCtorDef = clonedStateMachineTypeDef.Methods.Single(x => x.IsConstructor && !x.IsStatic);
                        var stateMachineCtorRef = stateMachineCtorDef.ImportInto(ModuleDefinition).WithGenericDeclaringType(cloneStateMachineTypeRef);
                        instruction.Operand = stateMachineCtorRef;
                    }
                    else if (methodRef is GenericInstanceMethod gim)
                    {
                        var mr = new GenericInstanceMethod(gim);
                        foreach (var generic in gim.GenericArguments)
                        {
                            if (generic == stateMachineTypeDef)
                            {
                                mr.GenericArguments.Add(clonedStateMachineTypeDef);
                            }
                            else
                            {
                                mr.GenericArguments.Add(generic);
                            }
                        }
                    }
                }
                else if (instruction.Operand is FieldReference fr && fieldMap.TryGetValue(fr.Resolve(), out var fieldRef))
                {
                    instruction.Operand = fieldRef;
                }
                else if (instruction.Operand == stateMachine)
                {
                    instruction.Operand = clonedStateMachine;
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
    }
}
