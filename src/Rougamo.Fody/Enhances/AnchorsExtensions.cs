using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Reflection;

namespace Rougamo.Fody.Enhances
{
    internal static class AnchorsExtensions
    {
        public static Instruction[] GetNops(this IAnchors anchors)
        {
            var nops = new List<Instruction>();

            foreach (var property in anchors.GetType().GetProperties())
            {
                if (!property.Name.StartsWith("Hosts") && property.PropertyType == typeof(Instruction) && property.GetCustomAttribute(typeof(SkipAttribute)) == null)
                {
                    var value = property.GetValue(anchors);
                    if (value is Instruction instruction && instruction.OpCode.Code == Code.Nop)
                    {
                        nops.Add(instruction);
                    }
                }
            }

            return nops.ToArray();
        }
    }
}
