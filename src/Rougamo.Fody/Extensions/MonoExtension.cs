using Mono.Cecil;
using Mono.Cecil.Cil;
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

        public static MethodReference RecursionImportMethod(this CustomAttribute attribute, ModuleDefinition moduleDef, string methodName)
        {
            return RecursionImportMethod(attribute.AttributeType.Resolve(), moduleDef, methodName);
        }

        public static MethodReference RecursionImportMethod(this TypeDefinition typeDef, ModuleDefinition moduleDef, string methodName)
        {
            var methodDef = typeDef.Methods.FirstOrDefault(md => md.Name == methodName && !md.HasParameters);
            if (methodDef != null) return moduleDef.ImportReference(methodDef);

            var baseTypeDef = typeDef.BaseType.Resolve();
            if (baseTypeDef.FullName == typeof(object).FullName) throw new RougamoException($"can not find method({methodName}) from {typeDef.FullName}");
            return RecursionImportMethod(baseTypeDef, moduleDef, methodName);
        }

        public static VariableDefinition CreateVariable(this MethodBody body, TypeReference variableTypeReference)
        {
            var variable = new VariableDefinition(variableTypeReference);
            body.Variables.Add(variable);
            return variable;
        }

        //public static AccessFlags GetPropertyFlags(this PropertyDefinition propertyDef)
        //{
        //    var flags = AccessFlags.Default;
        //    flags |= propertyDef.GetMethod.IsPublic ? AccessFlags.Public : AccessFlags.NonPublic;
        //    flags |= propertyDef.HasThis ? AccessFlags.Instance : AccessFlags.Static;
        //    return flags;
        //}

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

        #region Import

        public static TypeReference ImportInto(this TypeDefinition typeDef, ModuleDefinition moduleDef) => moduleDef.ImportReference(typeDef);

        public static FieldReference ImportInto(this FieldDefinition fieldDef, ModuleDefinition moduleDef) => moduleDef.ImportReference(fieldDef);

        public static MethodReference ImportInto(this MethodDefinition methodDef, ModuleDefinition moduleDef) => moduleDef.ImportReference(methodDef);

        #endregion Import
    }
}
