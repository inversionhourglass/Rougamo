using Mono.Cecil;
using Mono.Cecil.Cil;
using System;

namespace Rougamo.Fody
{
    internal static class InstructionExtensions
    {
        public static bool IsRet(this Instruction instruction)
        {
            if (instruction == null) throw new ArgumentNullException(nameof(instruction));

            return instruction.OpCode.Code == Code.Ret;
        }

        public static bool IsLdtoken(this Instruction instruction, string @interface, out TypeDefinition? typeDef)
        {
            typeDef = null;
            if (instruction.OpCode != OpCodes.Ldtoken) return false;

            typeDef = instruction.Operand as TypeDefinition;
            if (typeDef == null && instruction.Operand is TypeReference typeRef)
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
            throw new RougamoException(
                $"not support instruction Operand copy type: {instruction.Operand.GetType().FullName}");
        }

        public static Instruction ClosePreviousLdarg0(this Instruction instruction, MethodDefinition methodDef)
        {
            while ((instruction = instruction.Previous) != null && instruction.OpCode.Code != Code.Ldarg_0) { }
            return instruction != null && instruction.OpCode.Code == Code.Ldarg_0 ? instruction : throw new RougamoException($"[{methodDef.FullName}] cannot find ldarg.0 from previouses");
        }

        public static Instruction Stloc2Ldloc(this Instruction instruction, string exceptionMessage)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Stloc_0:
                    return Instruction.Create(OpCodes.Ldloc_0);
                case Code.Stloc_1:
                    return Instruction.Create(OpCodes.Ldloc_1);
                case Code.Stloc_2:
                    return Instruction.Create(OpCodes.Ldloc_2);
                case Code.Stloc_3:
                    return Instruction.Create(OpCodes.Ldloc_3);
                case Code.Stloc:
                    return Instruction.Create(OpCodes.Ldloc, (VariableDefinition)instruction.Operand);
                case Code.Stloc_S:
                    return Instruction.Create(OpCodes.Ldloc_S, (VariableDefinition)instruction.Operand);
                default:
                    throw new RougamoException(exceptionMessage);
            }
        }

        public static Instruction Ldloc2Stloc(this Instruction instruction, string exceptionMessage)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Ldloc_0:
                    return Instruction.Create(OpCodes.Stloc_0);
                case Code.Ldloc_1:
                    return Instruction.Create(OpCodes.Stloc_1);
                case Code.Ldloc_2:
                    return Instruction.Create(OpCodes.Stloc_2);
                case Code.Ldloc_3:
                    return Instruction.Create(OpCodes.Stloc_3);
                case Code.Ldloc:
                    return Instruction.Create(OpCodes.Stloc, (VariableDefinition)instruction.Operand);
                case Code.Ldloc_S:
                    return Instruction.Create(OpCodes.Stloc_S, (VariableDefinition)instruction.Operand);
                default:
                    throw new RougamoException(exceptionMessage);
            }
        }

        public static TypeReference GetVariableType(this Instruction ldlocIns, MethodBody body)
        {
            switch (ldlocIns.OpCode.Code)
            {
                case Code.Ldloc_0:
                    return body.Variables[0].VariableType;
                case Code.Ldloc_1:
                    return body.Variables[1].VariableType;
                case Code.Ldloc_2:
                    return body.Variables[2].VariableType;
                case Code.Ldloc_3:
                    return body.Variables[3].VariableType;
                case Code.Ldloc:
                case Code.Ldloc_S:
                    return ((VariableDefinition)ldlocIns.Operand).VariableType;
                case Code.Ldloca:
                case Code.Ldloca_S:
                    throw new RougamoException("need to take a research");
                default:
                    throw new RougamoException("can not get variable type from code: " + ldlocIns.OpCode.Code);

            }
        }
    }
}
