using Fody;
using Mono.Cecil.Cil;
using System;
using MethodImplAttribute = System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = System.Runtime.CompilerServices.MethodImplOptions;

namespace Mono.Cecil
{
    public static class InstructionExtensions
    {
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

        public static OpCode GetLdElemCode(this TypeReference typeRef)
        {
            var typeDef = typeRef.Resolve();
            if (typeDef.IsEnum(out TypeReference? underlying))
                return underlying!.MetadataType.GetLdElemCode();
            if (typeRef.IsValueType)
                return typeRef.MetadataType.GetLdElemCode();
            return OpCodes.Ldelem_Ref;
        }

        public static OpCode GetLdElemCode(this MetadataType type)
        {
            switch (type)
            {
                case MetadataType.Boolean:
                case MetadataType.Int32:
                case MetadataType.UInt32:
                    return OpCodes.Ldelem_I4;
                case MetadataType.Byte:
                case MetadataType.SByte:
                    return OpCodes.Ldelem_I1;
                case MetadataType.Char:
                case MetadataType.Int16:
                case MetadataType.UInt16:
                    return OpCodes.Ldelem_I2;
                case MetadataType.Double:
                    return OpCodes.Ldelem_R8;
                case MetadataType.Int64:
                case MetadataType.UInt64:
                    return OpCodes.Ldelem_I8;
                case MetadataType.Single:
                    return OpCodes.Ldelem_R4;
                default:
                    return OpCodes.Ldelem_Ref;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Instruction Ldloc(this VariableDefinition variable)
        {
            return Instruction.Create(OpCodes.Ldloc, variable);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Instruction Ldloca(this VariableDefinition variable)
        {
            return Instruction.Create(OpCodes.Ldloca, variable);
        }

        public static Instruction LdlocAny(this VariableDefinition variable)
        {
            var opLdloc = variable.VariableType.IsValueType ? OpCodes.Ldloca : OpCodes.Ldloc;
            return Instruction.Create(opLdloc, variable);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Instruction Stloc(this VariableDefinition variable)
        {
            return Instruction.Create(OpCodes.Stloc, variable);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Instruction Ldfld(this FieldReference fieldRef)
        {
            return Instruction.Create(OpCodes.Ldfld, fieldRef);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Instruction Ldflda(this FieldReference fieldRef)
        {
            return Instruction.Create(OpCodes.Ldflda, fieldRef);
        }

        public static Instruction LdfldAny(this FieldReference fieldRef)
        {
            var opLdfld = fieldRef.FieldType.IsValueType ? OpCodes.Ldflda : OpCodes.Ldfld;
            return Instruction.Create(opLdfld, fieldRef);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Instruction Stfld(this FieldReference fieldRef)
        {
            return Instruction.Create(OpCodes.Stfld, fieldRef);
        }

        public static Instruction CallAny(this MethodReference methodRef, MethodDefinition? methodDef = null)
        {
            methodDef ??= methodRef.Resolve();
            if (methodDef.IsConstructor) return Instruction.Create(OpCodes.Newobj, methodRef);

            var opCall = methodDef.IsVirtual && !methodDef.DeclaringType.IsValueType ? OpCodes.Callvirt : OpCodes.Call;
            return Instruction.Create(opCall, methodRef);
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

        public static bool IsLdtoken(this Instruction instruction, string @interface, out TypeReference? typeRef)
        {
            typeRef = null;
            if (instruction.OpCode != OpCodes.Ldtoken) return false;

            typeRef = instruction.Operand as TypeReference;

            return typeRef != null && typeRef.Implement(@interface);
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

        public static bool IsCallAny(this Instruction instruction, string methodName)
        {
            return (instruction.OpCode.Code == Code.Call || instruction.OpCode.Code == Code.Callvirt) && ((MethodReference)instruction.Operand).Name == methodName;
        }

        public static Instruction Clone(this Instruction instruction)
        {
            if (instruction.Operand == null) return Instruction.Create(instruction.OpCode);
            if (instruction.Operand is sbyte sbyteValue) return Instruction.Create(instruction.OpCode, sbyteValue);
            if (instruction.Operand is byte byteValue) return Instruction.Create(instruction.OpCode, byteValue);
            if (instruction.Operand is int intValue) return Instruction.Create(instruction.OpCode, intValue);
            if (instruction.Operand is long longValue) return Instruction.Create(instruction.OpCode, longValue);
            if (instruction.Operand is float floatValue) return Instruction.Create(instruction.OpCode, floatValue);
            if (instruction.Operand is double doubleValue) return Instruction.Create(instruction.OpCode, doubleValue);
            if (instruction.Operand is string stringValue) return Instruction.Create(instruction.OpCode, stringValue);
            if (instruction.Operand is FieldReference fieldReference)
                return Instruction.Create(instruction.OpCode, fieldReference);
            if (instruction.Operand is TypeReference typeReference)
                return Instruction.Create(instruction.OpCode, typeReference);
            if (instruction.Operand is MethodReference methodReference)
                return Instruction.Create(instruction.OpCode, methodReference);
            if (instruction.Operand is ParameterDefinition parameterDefinition)
                return Instruction.Create(instruction.OpCode, parameterDefinition);
            if (instruction.Operand is VariableDefinition variableDefinition)
                return Instruction.Create(instruction.OpCode, variableDefinition);
            if (instruction.Operand is Instruction instruction1)
                return Instruction.Create(instruction.OpCode, instruction1);
            if (instruction.Operand is Instruction[] instructions)
                return Instruction.Create(instruction.OpCode, instructions);
            if (instruction.Operand is CallSite callSite) return Instruction.Create(instruction.OpCode, callSite);
            throw new FodyWeavingException($"Unsupported operand type of instruction[offset: {instruction.Offset}]: {instruction.Operand.GetType().FullName}");
        }

        public static int? TryResolveInt32(this Instruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Ldc_I4_M1:
                    return -1;
                case Code.Ldc_I4_0:
                    return 0;
                case Code.Ldc_I4_1:
                    return 1;
                case Code.Ldc_I4_2:
                    return 2;
                case Code.Ldc_I4_3:
                    return 3;
                case Code.Ldc_I4_4:
                    return 4;
                case Code.Ldc_I4_5:
                    return 5;
                case Code.Ldc_I4_6:
                    return 6;
                case Code.Ldc_I4_7:
                    return 7;
                case Code.Ldc_I4_8:
                    return 8;
                case Code.Ldc_I4_S:
                case Code.Ldc_I4:
                    return Convert.ToInt32(instruction.Operand);
            }
            return null;
        }

        public static Instruction Set(this Instruction instruction, OpCode opcode)
        {
            instruction.OpCode = opcode;
            instruction.Operand = null;

            return instruction;
        }

        public static Instruction Set(this Instruction instruction, OpCode opcode, object? operand)
        {
            instruction.OpCode = opcode;
            instruction.Operand = operand;

            return instruction;
        }

        public static VariableDefinition ResolveVariable(this Instruction instruction, MethodDefinition methodDef)
        {
            var variables = methodDef.Body.Variables;

            switch (instruction.OpCode.Code)
            {
                case Code.Stloc_0:
                case Code.Ldloc_0:
                    return variables[0];
                case Code.Stloc_1:
                case Code.Ldloc_1:
                    return variables[1];
                case Code.Stloc_2:
                case Code.Ldloc_2:
                    return variables[2];
                case Code.Stloc_3:
                case Code.Ldloc_3:
                    return variables[3];
                case Code.Stloc:
                case Code.Stloc_S:
                case Code.Ldloc:
                case Code.Ldloca_S:
                    return (VariableDefinition)instruction.Operand;
                default:
                    throw new FodyWeavingException($"Instruction is not a ldloc or stloc operation, its opcode is {instruction.OpCode} and the offset is {instruction.Offset} in method {methodDef}");
            }
        }
    }
}
