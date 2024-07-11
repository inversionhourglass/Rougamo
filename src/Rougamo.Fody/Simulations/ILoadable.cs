using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations
{
    internal interface ILoadable
    {
        TypeReference TypeRef { get; }

        IList<Instruction> Load();
    }

    internal static class LoadableExtensions
    {
        public static IList<Instruction> If(this ILoadable loadable, Instruction? anchor, Func<Instruction, IList<Instruction>> handle)
        {
            return If(OpCodes.Brtrue, loadable, anchor, handle);
        }

        public static IList<Instruction> IfNot(this ILoadable loadable, Instruction? anchor, Func<Instruction, IList<Instruction>> handle)
        {
            return If(OpCodes.Brfalse, loadable, anchor, handle);
        }

        private static IList<Instruction> If(OpCode brCode, ILoadable loadable, Instruction? anchor, Func<Instruction, IList<Instruction>> handle)
        {
            var instructions = new List<Instruction>();
            anchor ??= Instruction.Create(OpCodes.Nop);

            instructions.Add(loadable.Load());
            instructions.Add(Instruction.Create(brCode, anchor));
            instructions.Add(handle(anchor));
            instructions.Add(anchor);

            return instructions;
        }
    }
}
