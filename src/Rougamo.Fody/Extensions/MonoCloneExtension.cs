using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody
{
    internal static class MonoCloneExtension
    {
        private static readonly System.Reflection.ConstructorInfo _CtorMethodDebugInformation = typeof(MethodDebugInformation).GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Single();
        private static readonly System.Reflection.FieldInfo _FieldVariableIndex = typeof(VariableDebugInformation).GetField("index", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        private static readonly System.Reflection.FieldInfo _FieldIndexVariable = typeof(VariableIndex).GetField("variable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        public static TypeDefinition Clone(this TypeDefinition typeDef, string typeName)
        {
            var clonedTypeDef = new TypeDefinition(typeDef.Namespace, typeName, typeDef.Attributes, typeDef.BaseType);

            if (typeDef.HasInterfaces)
            {
                clonedTypeDef.Interfaces.Add(typeDef.Interfaces);
            }

            if (typeDef.IsNested)
            {
                clonedTypeDef.DeclaringType = typeDef.DeclaringType;
            }

            if (typeDef.HasCustomAttributes)
            {
                clonedTypeDef.CustomAttributes.Add(typeDef.CustomAttributes);
            }

            var map = new Dictionary<object, object>();

            if (typeDef.HasGenericParameters)
            {
                var genericTypeRef = new GenericInstanceType(clonedTypeDef);
                foreach (var generic in typeDef.GenericParameters)
                {
                    var clonedGeneric = new GenericParameter(generic.Name, clonedTypeDef);
                    clonedTypeDef.GenericParameters.Add(clonedGeneric);

                    map[generic] = clonedGeneric;

                    genericTypeRef.GenericArguments.Add(clonedGeneric);
                }
                map[clonedTypeDef] = genericTypeRef;
            }
            else
            {
                map[clonedTypeDef] = clonedTypeDef;
            }

            if (typeDef.HasFields)
            {
                foreach (var field in typeDef.Fields)
                {
                    var clonedField = new FieldDefinition(field.Name, field.Attributes, field.FieldType);
                    clonedTypeDef.Fields.Add(clonedField);

                    map[field] = clonedField;
                }
            }

            if (typeDef.HasMethods)
            {
                var methodMap = new Dictionary<MethodDefinition, MethodDefinition>();
                foreach (var method in typeDef.Methods)
                {
                    var clonedMethod = new MethodDefinition(method.Name, method.Attributes, method.ReturnType);
                    clonedTypeDef.Methods.Add(clonedMethod);

                    methodMap[method] = clonedMethod;
                    map[method] = clonedMethod;
                }
                foreach (var item in methodMap)
                {
                    item.Key.Clone(item.Value, map, true);
                }
            }

            if (typeDef.HasNestedTypes)
            {
                foreach (var nestedType in typeDef.NestedTypes)
                {
                    clonedTypeDef.NestedTypes.Add(nestedType.Clone(nestedType.Name));
                }
            }

            return clonedTypeDef;
        }

        public static MethodDefinition Clone(this MethodDefinition methodDef, string methodName, Dictionary<object, object>? map = null, bool cloneBody = true)
        {
            var methodAttributes = (MethodAttributes)((ushort)methodDef.Attributes & (ushort.MaxValue ^ (ushort)MethodAttributes.Public)) | MethodAttributes.Private;

            var clonedMethodDef = new MethodDefinition(methodName, methodAttributes, methodDef.ReturnType);

            methodDef.Clone(clonedMethodDef, map, false, cloneBody);

            return clonedMethodDef;
        }

        public static void Clone(this MethodDefinition methodDef, MethodDefinition clonedMethodDef, Dictionary<object, object>? map, bool withOverrides, bool cloneBody = true)
        {
            clonedMethodDef.HasThis = methodDef.HasThis;
            clonedMethodDef.ExplicitThis = methodDef.ExplicitThis;
            clonedMethodDef.CallingConvention = methodDef.CallingConvention;

            if (methodDef.HasCustomAttributes)
            {
                clonedMethodDef.CustomAttributes.Add(methodDef.CustomAttributes);
            }

            if (withOverrides && methodDef.HasOverrides)
            {
                clonedMethodDef.Overrides.Add(methodDef.Overrides);
            }

            map ??= [];

            if (methodDef.HasGenericParameters)
            {
                foreach (var generic in methodDef.GenericParameters)
                {
                    var clonedGeneric = new GenericParameter(generic.Name, clonedMethodDef);
                    clonedMethodDef.GenericParameters.Add(clonedGeneric);

                    map[generic] = clonedGeneric;
                }
            }

            if (methodDef.HasParameters)
            {
                foreach (var parameterDef in methodDef.Parameters)
                {
                    var clonedParameterDef = parameterDef.Clone(map);
                    clonedMethodDef.Parameters.Add(clonedParameterDef);

                    map[parameterDef] = clonedParameterDef;
                }
            }

            if (!cloneBody) return;

            if (methodDef.Body.HasVariables)
            {
                foreach (var variable in methodDef.Body.Variables)
                {
                    var variableType = variable.VariableType;
                    if (variableType.Resolve() == methodDef.DeclaringType)
                    {
                        variableType = (TypeReference)map[clonedMethodDef.DeclaringType];
                    }
                    var clonedVariable = new VariableDefinition(variableType);
                    clonedMethodDef.Body.Variables.Add(clonedVariable);

                    map[variable] = clonedVariable;
                }
            }

            var instructionMap = new Dictionary<Instruction, Instruction>();
            var offsetMap = new Dictionary<int, Instruction>();
            var brs = new List<Instruction>();
            foreach (var instruction in methodDef.Body.Instructions)
            {
                var cloned = instruction.Clone();
                clonedMethodDef.Body.Instructions.Add(cloned);

                instructionMap[instruction] = cloned;
                offsetMap[instruction.Offset] = cloned;
                var operand = cloned.Operand;
                if (cloned.Operand != null)
                {
                    if (map.TryGetValue(operand, out var value))
                    {
                        cloned.Operand = value;
                    }
                    else if (operand is Instruction)
                    {
                        brs.Add(cloned);
                    }
                    else if (operand is FieldReference fieldRef && map.TryGetValue(fieldRef.Resolve(), out var fdObj))
                    {
                        var fd = (FieldDefinition)fdObj;
                        var fr = new FieldReference(fd.Name, fd.FieldType, (TypeReference)map[fd.DeclaringType]);
                        cloned.Operand = fr;
                    }
                    else if (operand is MethodReference mr)
                    {
                        if (map.TryGetValue(mr.Resolve(), out var mdObj))
                        {
                            // ignore, rougamo donot needs to implement this
                            throw new NotImplementedException("MonoCloneExtensions -> MethodDefinition -> MethodReference");
                        }
                        else if (mr is GenericInstanceMethod gim && gim.GenericArguments.Any(x => x.Resolve() == methodDef.DeclaringType))
                        {
                            var tr = (TypeReference)map[clonedMethodDef.DeclaringType];
                            var generics = gim.GenericArguments.Select(x => x.Resolve() == methodDef.DeclaringType ? tr : x).ToArray();
                            mr = gim.Resolve().ImportInto(methodDef.Module);
                            if (gim.DeclaringType is GenericInstanceType git)
                            {
                                mr = mr.WithGenericDeclaringType(git);
                            }
                            cloned.Operand = mr.WithGenerics(generics);
                        }
                    }
                }
            }
            foreach (var br in brs)
            {
                br.Operand = instructionMap[(Instruction)br.Operand];
            }

            foreach (var handler in methodDef.Body.ExceptionHandlers)
            {
                var catchType = handler.CatchType == null ? null : (map.TryGetValue(handler.CatchType, out var ct) ? (TypeReference)ct : handler.CatchType);
                clonedMethodDef.Body.ExceptionHandlers.Add(new ExceptionHandler(handler.HandlerType)
                {
                    CatchType = catchType,
                    TryStart = instructionMap[handler.TryStart],
                    TryEnd = instructionMap[handler.TryEnd],
                    HandlerStart = instructionMap[handler.HandlerStart],
                    HandlerEnd = instructionMap[handler.HandlerEnd]
                });
            }

            clonedMethodDef.Body.InitLocals = methodDef.Body.InitLocals;
            clonedMethodDef.Body.MaxStackSize = methodDef.Body.MaxStackSize;

            clonedMethodDef.DebugInformation = (MethodDebugInformation)_CtorMethodDebugInformation.Invoke(new object[] { clonedMethodDef });
            foreach (var point in methodDef.DebugInformation.SequencePoints)
            {
                var clonedPoint = new SequencePoint(offsetMap[point.Offset], point.Document)
                {
                    StartLine = point.StartLine,
                    StartColumn = point.StartColumn,
                    EndLine = point.EndLine,
                    EndColumn = point.EndColumn
                };
                clonedMethodDef.DebugInformation.SequencePoints.Add(clonedPoint);
            }
            clonedMethodDef.DebugInformation.Scope = methodDef.DebugInformation.Scope.Clone(offsetMap, map);
        }

        public static ParameterDefinition Clone(this ParameterDefinition parameterDef, Dictionary<object, object> map)
        {
            var parameterTypeRef = parameterDef.ParameterType;
            var isByReference = parameterTypeRef.IsByReference; // out or ref parameter
            if (isByReference)
            {
                parameterTypeRef = ((ByReferenceType)parameterTypeRef).ElementType;
            }
            if (parameterTypeRef.IsGenericParameter && map.TryGetValue(parameterTypeRef, out var mapTo))
            {
                parameterTypeRef = (GenericParameter)mapTo;
            }
            if (isByReference)
            {
                parameterTypeRef = new ByReferenceType(parameterTypeRef);
                map[parameterDef.ParameterType] = parameterTypeRef;
            }

            return new ParameterDefinition(parameterDef.Name, parameterDef.Attributes, parameterTypeRef);
        }

        public static ScopeDebugInformation? Clone(this ScopeDebugInformation? scope, Dictionary<int, Instruction> offsetMap, Dictionary<object, object> variableMap)
        {
            if (scope == null) return null;

            var start = scope.Start.IsEndOfMethod ? null : offsetMap[scope.Start.Offset];
            var end = scope.End.IsEndOfMethod ? null : offsetMap[scope.End.Offset];
            var clonedScope = new ScopeDebugInformation(start, end);
            foreach (var constant in scope.Constants)
            {
                clonedScope.Constants.Add(constant);
            }
            foreach (var variable in scope.Variables)
            {
                var index = _FieldVariableIndex.GetValue(variable);
                var variableDef = _FieldIndexVariable.GetValue(index);
                clonedScope.Variables.Add(new VariableDebugInformation((VariableDefinition)variableMap[variableDef], variable.Name));
            }
            foreach (var childScope in scope.Scopes)
            {
                clonedScope.Scopes.Add(childScope.Clone(offsetMap, variableMap));
            }

            return clonedScope;
        }
    }
}
