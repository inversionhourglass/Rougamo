using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody
{
    internal static class MonoExtension
    {
        private static readonly System.Reflection.ConstructorInfo _CtorMethodDebugInformation = typeof(MethodDebugInformation).GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Single();
        private static readonly System.Reflection.FieldInfo _FieldVariableIndex = typeof(VariableDebugInformation).GetField("index", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        private static readonly System.Reflection.FieldInfo _FieldIndexVariable = typeof(VariableIndex).GetField("variable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        public static bool Is(this TypeReference typeRef, string fullName)
        {
            return typeRef.Resolve()?.FullName == fullName;
        }

        public static bool Is(this CustomAttribute attribute, string fullName)
        {
            return attribute.AttributeType.Is(fullName);
        }

        public static bool Is(this MethodReference methodRef, string fullName)
        {
            return methodRef.FullName == fullName;
        }

        public static bool IsGeneric(this TypeReference typeRef, string genericName, out TypeReference[]? genericTypeRefs)
        {
            var typeDef = typeRef.Resolve();
            if (typeRef is GenericInstanceType git && typeDef != null && typeDef.FullName == genericName)
            {
                genericTypeRefs = git.GenericArguments.ToArray();
                return true;
            }
            genericTypeRefs = null;
            return false;
        }

        public static bool IsOrDerivesFrom(this TypeReference typeRef, string className)
        {
            return Is(typeRef, className) || DerivesFrom(typeRef, className);
        }

        public static bool Implement(this TypeReference typeRef, string @interface) => Implement(typeRef.Resolve(), @interface);

        public static bool Implement(this TypeDefinition typeDef, string @interface)
        {
            do
            {
                if (typeDef.Interfaces.Any(x => x.InterfaceType.FullName == @interface)) return true;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                typeDef = typeDef.BaseType?.Resolve();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            } while (typeDef != null);

            return false;
        }

        public static bool DerivesFrom(this TypeReference typeRef, string baseClass)
        {
            do
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                if ((typeRef = typeRef.Resolve()?.BaseType)?.FullName == baseClass) return true;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            } while (typeRef != null);

            return false;
        }

        public static bool DerivesFromAny(this TypeReference typeRef, params string[] baseClasses)
        {
            foreach (var baseClass in baseClasses)
            {
                if (typeRef.DerivesFrom(baseClass)) return true;
            }

            return false;
        }

        public static bool DerivesFrom(this CustomAttribute attribute, string baseClass)
        {
            return attribute.AttributeType.DerivesFrom(baseClass);
        }

        public static bool IsDelegate(this TypeReference typeRef)
        {
            return DerivesFrom(typeRef, Constants.TYPE_MulticastDelegate);
        }

        public static bool IsArray(this TypeReference typeRef, out TypeReference? elementType)
        {
            elementType = null;
            if (!typeRef.IsArray)
                return false;

            elementType = ((ArrayType)typeRef).ElementType;
            return true;
        }

        public static bool IsEnum(this TypeReference typeRef, out TypeReference? underlyingType)
        {
            var typeDef = typeRef.Resolve();
            if (typeDef != null && typeDef.IsEnum)
            {
                underlyingType = typeDef.Fields.First(f => f.Name == "value__").FieldType;
                return true;
            }

            underlyingType = null;
            return false;
        }

        public static bool IsString(this TypeReference typeRef)
        {
            return typeRef.Resolve().FullName == typeof(string).FullName;
        }

        public static bool IsNullable(this TypeReference typeRef)
        {
            return typeRef.Resolve().FullName.StartsWith("System.Nullable");
        }

        public static bool IsUnboxable(this TypeReference typeRef)
        {
            return typeRef.IsValueType || typeRef.IsEnum(out _) && !typeRef.IsArray || typeRef.IsGenericParameter;
        }

        public static OpCode GetStElemCode(this TypeReference typeRef)
        {
            var typeDef = typeRef.Resolve();
            if (typeDef.IsEnum(out TypeReference? underlying))
                return underlying!.MetadataType.GetStElemCode();
            if (typeRef.IsValueType)
                return typeRef.MetadataType.GetStElemCode();
            return OpCodes.Stelem_Ref;
        }

        public static OpCode GetStElemCode(this MetadataType type)
        {
            switch (type)
            {
                case MetadataType.Boolean:
                case MetadataType.Int32:
                case MetadataType.UInt32:
                    return OpCodes.Stelem_I4;
                case MetadataType.Byte:
                case MetadataType.SByte:
                    return OpCodes.Stelem_I1;
                case MetadataType.Char:
                case MetadataType.Int16:
                case MetadataType.UInt16:
                    return OpCodes.Stelem_I2;
                case MetadataType.Double:
                    return OpCodes.Stelem_R8;
                case MetadataType.Int64:
                case MetadataType.UInt64:
                    return OpCodes.Stelem_I8;
                case MetadataType.Single:
                    return OpCodes.Stelem_R4;
                default:
                    return OpCodes.Stelem_Ref;
            }
        }

        public static MethodDefinition GetZeroArgsCtor(this TypeDefinition typeDef)
        {
            var zeroCtor = typeDef.GetConstructors().FirstOrDefault(ctor => !ctor.HasParameters);
            if (zeroCtor == null)
                throw new RougamoException($"could not found zero arguments constructor from {typeDef.FullName}");
            return zeroCtor;
        }

        public static MethodReference? RecursionImportPropertySet(this CustomAttribute attribute,
            ModuleDefinition moduleDef, string propertyName)
        {
            return RecursionImportPropertySet(attribute.AttributeType.Resolve(), moduleDef, propertyName);
        }

        public static MethodReference? RecursionImportPropertySet(this TypeDefinition typeDef,
            ModuleDefinition moduleDef, string propertyName)
        {
            var propertyDef = typeDef.Properties.FirstOrDefault(pd => pd.Name == propertyName);
            if (propertyDef != null)
            {
                return propertyDef.SetMethod == null ? null : moduleDef.ImportReference(propertyDef.SetMethod);
            }

            var baseTypeDef = typeDef.BaseType.Resolve();
            if (baseTypeDef.FullName == typeof(object).FullName)
                throw new RougamoException($"can not find property({propertyName}) from {typeDef.FullName}");
            return RecursionImportPropertySet(baseTypeDef, moduleDef, propertyName);
        }

        public static MethodReference RecursionImportPropertyGet(this TypeDefinition typeDef,
            ModuleDefinition moduleDef, string propertyName)
        {
            var propertyDef = typeDef.Properties.FirstOrDefault(pd => pd.Name == propertyName);
            if (propertyDef != null) return moduleDef.ImportReference(propertyDef.GetMethod);

            var baseTypeDef = typeDef.BaseType.Resolve();
            if (baseTypeDef.FullName == typeof(object).FullName)
                throw new RougamoException($"can not find property({propertyName}) from {typeDef.FullName}");
            return RecursionImportPropertyGet(baseTypeDef, moduleDef, propertyName);
        }

        public static MethodReference RecursionImportMethod(this CustomAttribute attribute, ModuleDefinition moduleDef,
            string methodName, Func<MethodDefinition, bool> predicate)
        {
            return RecursionImportMethod(attribute.AttributeType.Resolve(), moduleDef, methodName, predicate);
        }

        public static MethodReference RecursionImportMethod(this TypeDefinition typeDef, ModuleDefinition moduleDef,
            string methodName, Func<MethodDefinition, bool> predicate)
        {
            var methodDef = typeDef.Methods.FirstOrDefault(md => md.Name == methodName && predicate(md));
            if (methodDef != null) return moduleDef.ImportReference(methodDef);

            var baseTypeDef = typeDef.BaseType.Resolve();
            if (baseTypeDef.FullName == typeof(object).FullName)
                throw new RougamoException($"can not find method({methodName}) from {typeDef.FullName}");
            return RecursionImportMethod(baseTypeDef, moduleDef, methodName, predicate);
        }

        public static VariableDefinition CreateVariable(this MethodBody body, TypeReference variableTypeReference)
        {
            var variable = new VariableDefinition(variableTypeReference);
            body.Variables.Add(variable);
            return variable;
        }

        public static List<GenericInstanceType> GetGenericInterfaces(this TypeDefinition typeDef, string interfaceName)
        {
            var interfaces = new List<GenericInstanceType>();
            do
            {
                var titf = typeDef.Interfaces.Select(itf => itf.InterfaceType)
                    .Where(itfRef => itfRef.FullName.StartsWith(interfaceName + "<"));
                interfaces.AddRange(titf.Cast<GenericInstanceType>());
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                typeDef = typeDef.BaseType?.Resolve();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            } while (typeDef != null);

            return interfaces;
        }

        public static TypeDefinition? GetInterfaceDefinition(this TypeDefinition typeDef, string interfaceName)
        {
            do
            {
                var interfaceRef = typeDef.Interfaces.Select(itf => itf.InterfaceType)
                    .Where(itfRef => itfRef.FullName == interfaceName);
                if (interfaceRef.Any()) return interfaceRef.First().Resolve();
                typeDef = typeDef.BaseType.Resolve();
            } while (typeDef != null);

            return null;
        }

        #region Import

        public static TypeReference ImportInto(this TypeReference typeRef, ModuleDefinition moduleDef) =>
            moduleDef.ImportReference(typeRef);

        public static FieldReference ImportInto(this FieldReference fieldRef, ModuleDefinition moduleDef) =>
            moduleDef.ImportReference(fieldRef);

        public static MethodReference ImportInto(this MethodReference methodRef, ModuleDefinition moduleDef) =>
            moduleDef.ImportReference(methodRef);

        public static TypeReference ImportInto(this ParameterDefinition parameterDef, ModuleDefinition moduleDef)
        {
            var parameterTypeRef = parameterDef.ParameterType.ImportInto(moduleDef);
            var isByReference = parameterDef.ParameterType.IsByReference;
            if (isByReference)
            {
                parameterTypeRef = ((ByReferenceType)parameterDef.ParameterType).ElementType.ImportInto(moduleDef);
            }

            return parameterTypeRef;
        }

        #endregion Import

        public static TypeDefinition ResolveStateMachine(this MethodDefinition methodDef, string stateMachineAttributeName)
        {
            var stateMachineAttr = methodDef.CustomAttributes.Single(attr => attr.Is(stateMachineAttributeName));
            var obj = stateMachineAttr.ConstructorArguments[0].Value;
            return obj as TypeDefinition ?? ((TypeReference)obj).Resolve();
        }

        public static Instruction Ldloc(this VariableDefinition variable)
        {
            return Instruction.Create(OpCodes.Ldloc, variable);
        }

        public static Instruction LdlocOrA(this VariableDefinition variable)
        {
            var variableTypeDef = variable.VariableType.Resolve();
            return variable.VariableType.IsGenericParameter || variableTypeDef.IsValueType && !variableTypeDef.IsEnum && !variableTypeDef.IsPrimitive ? Instruction.Create(OpCodes.Ldloca, variable) : Instruction.Create(OpCodes.Ldloc, variable);
        }

        public static Instruction Ldind(this TypeReference typeRef)
        {
            if (typeRef == null) throw new ArgumentNullException(nameof(typeRef), "Ldind argument null");
            var typeDef = typeRef.Resolve();
            if (typeDef == null) return Instruction.Create(OpCodes.Ldobj, typeRef);
            if (!typeRef.IsValueType) return Instruction.Create(OpCodes.Ldind_Ref);
            if (typeDef.Is(typeof(byte).FullName)) return Instruction.Create(OpCodes.Ldind_I1);
            if (typeDef.Is(typeof(short).FullName)) return Instruction.Create(OpCodes.Ldind_I2);
            if (typeDef.Is(typeof(int).FullName)) return Instruction.Create(OpCodes.Ldind_I4);
            if (typeDef.Is(typeof(long).FullName)) return Instruction.Create(OpCodes.Ldind_I8);
            if (typeDef.Is(typeof(sbyte).FullName)) return Instruction.Create(OpCodes.Ldind_U1);
            if (typeDef.Is(typeof(ushort).FullName)) return Instruction.Create(OpCodes.Ldind_U2);
            if (typeDef.Is(typeof(uint).FullName)) return Instruction.Create(OpCodes.Ldind_U4);
            if (typeDef.Is(typeof(ulong).FullName)) return Instruction.Create(OpCodes.Ldind_I8);
            if (typeDef.Is(typeof(float).FullName)) return Instruction.Create(OpCodes.Ldind_R4);
            if (typeDef.Is(typeof(double).FullName)) return Instruction.Create(OpCodes.Ldind_R8);
            if (typeDef.IsEnum)
            {
                if (typeDef.Fields.Count == 0) return Instruction.Create(OpCodes.Ldind_I);
                return Ldind(typeDef.Fields[0].FieldType);
            }
            return Instruction.Create(OpCodes.Ldobj, typeRef); // struct & enum & generic parameter
        }

        public static Instruction Stind(this TypeReference typeRef)
        {
            if (typeRef == null) throw new ArgumentNullException(nameof(typeRef), "Stind argument null");
            if (typeRef.IsGenericParameter) return Instruction.Create(OpCodes.Stobj, typeRef);
            var typeDef = typeRef.Resolve();
            if (!typeRef.IsValueType) return Instruction.Create(OpCodes.Stind_Ref);
            if (typeDef.Is(typeof(byte).FullName) || typeDef.Is(typeof(sbyte).FullName)) return Instruction.Create(OpCodes.Stind_I1);
            if (typeDef.Is(typeof(short).FullName) || typeDef.Is(typeof(ushort).FullName)) return Instruction.Create(OpCodes.Stind_I2);
            if (typeDef.Is(typeof(int).FullName) || typeDef.Is(typeof(uint).FullName)) return Instruction.Create(OpCodes.Stind_I4);
            if (typeDef.Is(typeof(long).FullName) || typeDef.Is(typeof(ulong).FullName)) return Instruction.Create(OpCodes.Stind_I8);
            if (typeDef.Is(typeof(float).FullName)) return Instruction.Create(OpCodes.Stind_R4);
            if (typeDef.Is(typeof(double).FullName)) return Instruction.Create(OpCodes.Stind_R8);
            if (typeDef.IsEnum)
            {
                if (typeDef.Fields.Count == 0) return Instruction.Create(OpCodes.Stind_I);
                return Stind(typeDef.Fields[0].FieldType);
            }
            return Instruction.Create(OpCodes.Stobj, typeRef); // struct & enum & generic parameter
        }

        public static MethodReference GenericTypeMethodReference(this TypeReference typeRef, MethodReference methodRef, ModuleDefinition moduleDefinition)
        {
            var genericMethodRef = new MethodReference(methodRef.Name, methodRef.ReturnType, typeRef)
            {
                HasThis = methodRef.HasThis,
                ExplicitThis = methodRef.ExplicitThis,
                CallingConvention = methodRef.CallingConvention
            };
            foreach (var parameter in methodRef.Parameters)
            {
                genericMethodRef.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
            }
            foreach (var parameter in methodRef.GenericParameters)
            {
                genericMethodRef.GenericParameters.Add(new GenericParameter(parameter.Name, genericMethodRef));
            }

            return genericMethodRef.ImportInto(moduleDefinition);
        }

        public static MethodReference GenericMethodReference(this MethodReference methodRef, params TypeReference[] genericTypeRefs)
        {
            var genericInstanceMethod = new GenericInstanceMethod(methodRef);
            genericInstanceMethod.GenericArguments.Add(genericTypeRefs);

            return genericInstanceMethod;
        }

        private static Code[] _EmptyCodes = new[] { Code.Nop, Code.Ret };
        public static bool IsEmpty(this MethodDefinition methodDef)
        {
            foreach (var instruction in methodDef.Body.Instructions)
            {
                if (!_EmptyCodes.Contains(instruction.OpCode.Code)) return false;
            }

            return true;
        }

        private static readonly Dictionary<Code, OpCode> _OptimizeCodes = new Dictionary<Code, OpCode>
        {
            { Code.Leave_S, OpCodes.Leave }, { Code.Br_S, OpCodes.Br },
            { Code.Brfalse_S, OpCodes.Brfalse }, { Code.Brtrue_S, OpCodes.Brtrue },
            { Code.Beq_S, OpCodes.Beq }, { Code.Bne_Un_S, OpCodes.Bne_Un },
            { Code.Bge_S, OpCodes.Bge }, { Code.Bgt_S, OpCodes.Bgt },
            { Code.Ble_S, OpCodes.Ble }, { Code.Blt_S, OpCodes.Blt },
            { Code.Bge_Un_S, OpCodes.Bge_Un }, { Code.Bgt_Un_S, OpCodes.Bgt_Un },
            { Code.Ble_Un_S, OpCodes.Ble_Un }, { Code.Blt_Un_S, OpCodes.Blt_Un }
        };
        public static void OptimizePlus(this MethodBody body, Instruction[] nops)
        {
            body.OptimizeShort();
            body.OptimizeUselessBr();
            body.OptimizeNops(nops);
            body.Optimize();
        }

        private static void OptimizeShort(this MethodBody body)
        {
            foreach (var instruction in body.Instructions)
            {
                if (_OptimizeCodes.TryGetValue(instruction.OpCode.Code, out var opcode))
                {
                    instruction.OpCode = opcode;
                }
            }
        }

        private static void OptimizeUselessBr(this MethodBody body)
        {
            var cannotRemoves = new List<Instruction>();
            var canRemoves = new List<Instruction>();
            foreach (var instruction in body.Instructions)
            {
                if (instruction.Operand is Instruction br2)
                {
                    cannotRemoves.Add(br2);
                    if (instruction.OpCode.Code == Code.Br)
                    {
                        var current = instruction.Next;
                        while (current != br2)
                        {
                            if (current.OpCode.Code != Code.Nop) break;
                            current = current.Next;
                        }
                        if (current == br2) canRemoves.Add(instruction);
                    }
                }
            }
            foreach (var handler in body.ExceptionHandlers)
            {
                cannotRemoves.Add(handler.TryStart);
                cannotRemoves.Add(handler.TryEnd);
                cannotRemoves.Add(handler.HandlerStart);
                cannotRemoves.Add(handler.HandlerEnd);
            }
            foreach (var item in canRemoves)
            {
                if (cannotRemoves.Contains(item)) continue;

                body.Instructions.Remove(item);
            }
        }

        private static void OptimizeNops(this MethodBody body, Instruction[] nops)
        {
            foreach (var handler in body.ExceptionHandlers)
            {
                if (NeedUpdateHandlerAnchor(handler.TryStart, handler.TryEnd, nops, out var newTryStart))
                {
                    handler.TryStart = newTryStart;
                }
                if (handler.TryEnd != handler.HandlerStart)
                {
                    if (NeedUpdateHandlerAnchor(handler.TryEnd, handler.HandlerStart, nops, out var newTryEnd))
                    {
                        handler.TryEnd = newTryEnd;
                    }
                    if (NeedUpdateHandlerAnchor(handler.HandlerStart, handler.HandlerEnd, nops, out var newHandlerStart))
                    {
                        handler.HandlerStart = newHandlerStart;
                    }
                }
                else
                {
                    if (NeedUpdateHandlerAnchor(handler.TryEnd, handler.HandlerEnd, nops, out var newTryEndHandlerStart))
                    {
                        handler.TryEnd = newTryEndHandlerStart;
                        handler.HandlerStart = newTryEndHandlerStart;
                    }
                }
                if (NeedUpdateHandlerAnchor(handler.HandlerEnd, null, nops, out var newHandlerEnd))
                {
                    handler.HandlerEnd = newHandlerEnd;
                }
            }

            foreach (var instruction in body.Instructions)
            {
                if (instruction.Operand is Instruction to)
                {
                    var current = to;
                    while (nops.Contains(current))
                    {
                        current = current.Next;
                    }
                    if (current != to && current != null)
                    {
                        instruction.Operand = current;
                    }
                }
            }
            foreach (var nop in nops)
            {
                body.Instructions.Remove(nop);
            }
        }

        private static bool NeedUpdateHandlerAnchor(Instruction? target, Instruction? checkLimit, Instruction[] nops, out Instruction? updateTarget)
        {
            updateTarget = null;
            if (target == null) return false;

            var current = target;
            while (current != checkLimit)
            {
                if (!nops.Contains(current)) break;
                current = current.Next;
            }
            if (current == checkLimit || current == target)
            {
                return false;
            }
            updateTarget = current;
            return true;
        }

        public static MethodDefinition Clone(this MethodDefinition methodDef, string methodName)
        {
            var clonedMethodDef = new MethodDefinition(methodName, methodDef.Attributes, methodDef.ReturnType);

            if (methodDef.HasCustomAttributes)
            {
                clonedMethodDef.CustomAttributes.Add(methodDef.CustomAttributes);
            }

            var map = new Dictionary<object, object>();

            if (methodDef.HasGenericParameters)
            {
                foreach (var generic in methodDef.GenericParameters)
                {
                    var clonedGeneric = new GenericParameter(generic.Name, clonedMethodDef);
                    clonedMethodDef.GenericParameters.Add(clonedGeneric);

                    map[generic] = clonedGeneric;
                }
            }

            foreach (var parameterDef in methodDef.Parameters)
            {
                var parameterTypeRef = parameterDef.ParameterType;
                var isByReference = parameterTypeRef.IsByReference;
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
                var clonedParameterDef = new ParameterDefinition(parameterDef.Name, parameterDef.Attributes, parameterTypeRef);
                clonedMethodDef.Parameters.Add(clonedParameterDef);

                map[parameterDef] = clonedParameterDef;
            }

            foreach (var variable in methodDef.Body.Variables)
            {
                var clonedVariable = new VariableDefinition(variable.VariableType);
                clonedMethodDef.Body.Variables.Add(clonedVariable);

                map[variable] = clonedVariable;
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
                if (cloned.Operand != null)
                {
                    if (map.TryGetValue(cloned.Operand, out var value))
                    {
                        cloned.Operand = value;
                    }
                    else if (cloned.Operand is Instruction)
                    {
                        brs.Add(cloned);
                    }
                }
            }
            foreach (var br in brs)
            {
                br.Operand = instructionMap[(Instruction)br.Operand];
            }

            foreach (var handler in methodDef.Body.ExceptionHandlers)
            {
                clonedMethodDef.Body.ExceptionHandlers.Add(new ExceptionHandler(handler.HandlerType)
                {
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

            return clonedMethodDef;
        }

        public static ScopeDebugInformation Clone(this ScopeDebugInformation scope, Dictionary<int, Instruction> offsetMap, Dictionary<object, object> variableMap)
        {
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