using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations
{
    internal interface IAssignable
    {
        IList<Instruction> Assign(Func<IAssignable, IList<Instruction>> valueFactory);
    }

    internal static class AssignableExtensions
    {
        public static IList<Instruction> Assign(this IAssignable assignable, ILoadable from) => assignable.Assign(target => from.Load());
    }
}
