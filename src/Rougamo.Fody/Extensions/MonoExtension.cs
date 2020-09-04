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
        public static bool Is(this TypeReference typeRef, string fullName)
        {
            return typeRef.Resolve().FullName == fullName;
        }

        public static bool Is(this CustomAttribute attribute, string fullName)
        {
            return attribute.AttributeType.Is(fullName);
        }

        public static bool Implement(this TypeDefinition typeDef, string @interface)
        {
            do
            {
                if (typeDef.Interfaces.Any(x => x.InterfaceType.FullName == @interface)) return true;
                typeDef = typeDef.BaseType?.Resolve();
            } while (typeDef != null);
            return false;
        }

        public static bool DerivesFrom(this TypeReference typeRef, string baseClass)
        {
            do
            {
                if ((typeRef = typeRef.Resolve().BaseType)?.FullName == baseClass) return true;
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

        public static bool IsArray(this TypeReference typeRef, out TypeReference elementType)
        {
            elementType = null;
            if (!typeRef.IsArray)
                return false;

            elementType = ((ArrayType)typeRef).ElementType;
            return true;
        }

        public static bool IsEnum(this TypeReference typeRef, out TypeReference underlyingType)
        {
            var typeDef = typeRef.Resolve();
            if (typeDef.IsEnum)
            {
                underlyingType = typeDef.Fields.First(f => f.Name == "value__").FieldType;
                return true;
            }

            underlyingType = null;
            return false;
        }

        public static bool IsLdtoken(this Instruction instruction, string @interface, out TypeDefinition typeDef)
        {
            typeDef = null;
            if (instruction.OpCode != OpCodes.Ldtoken) return false;

            typeDef = instruction.Operand as TypeDefinition;
            if(typeDef == null && instruction.Operand is TypeReference typeRef)
            {
                typeDef = typeRef.Resolve();
            }
            if (typeDef == null) return false;
            return typeDef.Implement(@interface);
        }

        public static bool IsStfld(this Instruction instruction, string fieldName, string fieldType)
        {
            if (instruction.OpCode != OpCodes.Stfld) return false;

            var def = instruction.Operand as FieldDefinition;
            if (def == null && instruction.Operand is FieldReference @ref)
            {
                def = @ref.Resolve();
            }
            return def != null && def.Name == fieldName && def.FieldType.Is(fieldType);
        }

        //public static System.Collections.Generic.List<TypeReference> 

        public static OpCode GetStElemCode(this TypeReference typeRef)
        {
            var typeDef = typeRef.Resolve();
            if (typeDef.IsEnum(out TypeReference underlying))
                return underlying.MetadataType.GetStElemCode();
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
            if (zeroCtor == null) throw new RougamoException($"could not found zero arguments constructor from {typeDef.FullName}");
            return zeroCtor;
        }

        public static MethodReference RecursionImportPropertySet(this CustomAttribute attribute, ModuleDefinition moduleDef, string propertyName)
        {
            return RecursionImportPropertySet(attribute.AttributeType.Resolve(), moduleDef, propertyName);
        }

        public static MethodReference RecursionImportPropertySet(this TypeDefinition typeDef, ModuleDefinition moduleDef, string propertyName)
        {
            var propertyDef = typeDef.Properties.FirstOrDefault(pd => pd.Name == propertyName);
            if (propertyDef != null) return moduleDef.ImportReference(propertyDef.SetMethod);

            var baseTypeDef = typeDef.BaseType.Resolve();
            if (baseTypeDef.FullName == typeof(object).FullName) throw new RougamoException($"can not find property({propertyName}) from {typeDef.FullName}");
            return RecursionImportPropertySet(baseTypeDef, moduleDef, propertyName);
        }
        public static MethodReference RecursionImportPropertyGet(this TypeDefinition typeDef, ModuleDefinition moduleDef, string propertyName)
        {
            var propertyDef = typeDef.Properties.FirstOrDefault(pd => pd.Name == propertyName);
            if (propertyDef != null) return moduleDef.ImportReference(propertyDef.GetMethod);

            var baseTypeDef = typeDef.BaseType.Resolve();
            if (baseTypeDef.FullName == typeof(object).FullName) throw new RougamoException($"can not find property({propertyName}) from {typeDef.FullName}");
            return RecursionImportPropertyGet(baseTypeDef, moduleDef, propertyName);
        }

        public static MethodReference RecursionImportMethod(this CustomAttribute attribute, ModuleDefinition moduleDef, string methodName, Func<MethodDefinition, bool> predicate)
        {
            return RecursionImportMethod(attribute.AttributeType.Resolve(), moduleDef, methodName, predicate);
        }

        public static MethodReference RecursionImportMethod(this TypeDefinition typeDef, ModuleDefinition moduleDef, string methodName, Func<MethodDefinition, bool> predicate)
        {
            var methodDef = typeDef.Methods.FirstOrDefault(md => md.Name == methodName && predicate(md));
            if (methodDef != null) return moduleDef.ImportReference(methodDef);

            var baseTypeDef = typeDef.BaseType.Resolve();
            if (baseTypeDef.FullName == typeof(object).FullName) throw new RougamoException($"can not find method({methodName}) from {typeDef.FullName}");
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
                var titf = typeDef.Interfaces.Select(itf => itf.InterfaceType).Where(itfRef => itfRef.FullName.StartsWith(interfaceName + "<"));
                interfaces.AddRange(titf.Cast<GenericInstanceType>());
                typeDef = typeDef.BaseType?.Resolve();
            } while (typeDef != null);
            return interfaces;
        }

        public static TypeDefinition GetInterfaceDefinition(this TypeDefinition typeDef, string interfaceName)
        {
            do
            {
                var interfaceRef = typeDef.Interfaces.Select(itf => itf.InterfaceType).Where(itfRef => itfRef.FullName == interfaceName);
                if (interfaceRef.Any()) return interfaceRef.First().Resolve();
                typeDef = typeDef.BaseType.Resolve();
            } while (typeDef != null);
            return null;
        }

        #region Import

        public static TypeReference ImportInto(this TypeReference typeRef, ModuleDefinition moduleDef) => moduleDef.ImportReference(typeRef);

        public static FieldReference ImportInto(this FieldReference fieldRef, ModuleDefinition moduleDef) => moduleDef.ImportReference(fieldRef);

        public static MethodReference ImportInto(this MethodReference methodRef, ModuleDefinition moduleDef) => moduleDef.ImportReference(methodRef);

        #endregion Import

        public static Instruction Copy(this Instruction instruction)
        {
            if (instruction.Operand == null) return Instruction.Create(instruction.OpCode);
            if (instruction.Operand is sbyte sbyteValue) return Instruction.Create(instruction.OpCode, sbyteValue);
            if (instruction.Operand is byte byteValue) return Instruction.Create(instruction.OpCode, byteValue);
            if (instruction.Operand is int intValue) return Instruction.Create(instruction.OpCode, intValue);
            if (instruction.Operand is long longValue) return Instruction.Create(instruction.OpCode, longValue);
            if (instruction.Operand is float floatValue) return Instruction.Create(instruction.OpCode, floatValue);
            if (instruction.Operand is double doubleValue) return Instruction.Create(instruction.OpCode, doubleValue);
            if (instruction.Operand is string stringValue) return Instruction.Create(instruction.OpCode, stringValue);
            if (instruction.Operand is FieldReference fieldReference) return Instruction.Create(instruction.OpCode, fieldReference);
            if (instruction.Operand is TypeReference typeReference) return Instruction.Create(instruction.OpCode, typeReference);
            if (instruction.Operand is MethodReference methodReference) return Instruction.Create(instruction.OpCode, methodReference);
            if (instruction.Operand is ParameterDefinition parameterDefinition) return Instruction.Create(instruction.OpCode, parameterDefinition);
            if (instruction.Operand is VariableDefinition variableDefinition) return Instruction.Create(instruction.OpCode, variableDefinition);
            if (instruction.Operand is Instruction instruction1) return Instruction.Create(instruction.OpCode, instruction1);
            if (instruction.Operand is Instruction[] instructions) return Instruction.Create(instruction.OpCode, instructions);
            if (instruction.Operand is CallSite callSite) return Instruction.Create(instruction.OpCode, callSite);
            throw new RougamoException($"not support instruction Operand copy type: {instruction.Operand.GetType().FullName}");
        }
    }
}
