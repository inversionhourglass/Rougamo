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
    }
}
