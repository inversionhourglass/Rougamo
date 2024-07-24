using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations
{
    internal interface ILoadable : IAnalysable, IAssertable
    {
        ModuleWeaver ModuleWeaver { get; }

        IList<Instruction> Load();

        IList<Instruction> Cast(TypeReference to);
    }

    internal static class LoadableExtensions
    {
        public static IList<Instruction> Cast(this ILoadable from, ILoadable to) => from.Cast(to.Type.Ref);

        public static IList<Instruction> If(this ILoadable loadable, Func<Instruction, IList<Instruction>> handle)
        {
            return If(loadable, null, handle);
        }

        public static IList<Instruction> If(this ILoadable loadable, Instruction? anchor, Func<Instruction, IList<Instruction>> handle)
        {
            return If(loadable.FalseToken, loadable, anchor, handle);
        }

        public static IList<Instruction> If(this ILoadable loadable, Func<Instruction, Instruction, IList<Instruction>> handleIf, Func<Instruction, Instruction, IList<Instruction>> handleElse)
        {
            return If(loadable, null, null, handleIf, handleElse);
        }

        public static IList<Instruction> If(this ILoadable loadable, Instruction? anchorIfNot, Instruction? anchorEnd, Func<Instruction, Instruction, IList<Instruction>> handleIf, Func<Instruction, Instruction, IList<Instruction>> handleElse)
        {
            return If(loadable.FalseToken, loadable, anchorIfNot, anchorEnd, handleIf, handleElse);
        }

        public static IList<Instruction> IfNot(this ILoadable loadable, Func<Instruction, IList<Instruction>> handle)
        {
            return IfNot(loadable, null, handle);
        }

        public static IList<Instruction> IfNot(this ILoadable loadable, Instruction? anchor, Func<Instruction, IList<Instruction>> handle)
        {
            return If(loadable.TrueToken, loadable, anchor, handle);
        }

        public static IList<Instruction> IfNot(this ILoadable loadable, Func<Instruction, Instruction, IList<Instruction>> handleIfNot, Func<Instruction, Instruction, IList<Instruction>> handleElse)
        {
            return IfNot(loadable, null, null, handleIfNot, handleElse);
        }

        public static IList<Instruction> IfNot(this ILoadable loadable, Instruction? anchorIf, Instruction? anchorEnd, Func<Instruction, Instruction, IList<Instruction>> handleIfNot, Func<Instruction, Instruction, IList<Instruction>> handleElse)
        {
            return If(loadable.TrueToken, loadable, anchorIf, anchorEnd, handleIfNot, handleElse);
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

        private static IList<Instruction> If(OpCode brCode, ILoadable loadable, Instruction? anchorIfNot, Instruction? anchorEnd, Func<Instruction, Instruction, IList<Instruction>> handleIf, Func<Instruction, Instruction, IList<Instruction>> handleElse)
        {
            var instructions = new List<Instruction>();
            anchorIfNot ??= Instruction.Create(OpCodes.Nop);
            anchorEnd ??= Instruction.Create(OpCodes.Nop);

            instructions.Add(loadable.Load());
            instructions.Add(Instruction.Create(brCode, anchorIfNot));
            instructions.Add(handleIf(anchorIfNot, anchorEnd));
            instructions.Add(Instruction.Create(OpCodes.Br, anchorEnd));
            instructions.Add(anchorIfNot);
            instructions.Add(handleElse(anchorIfNot, anchorEnd));
            instructions.Add(anchorEnd);

            return instructions;
        }
    }
}
