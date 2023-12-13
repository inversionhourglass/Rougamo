using System;
using System.Linq;

namespace Rougamo.Fody.Signature.Patterns
{
    public class AttributePattern
    {
        public AttributePattern(AttributePositioon position, int index, ITypePattern attributeType)
        {
            Position = position;
            Index = index;
            AttributeType = attributeType;
        }

        public AttributePositioon Position { get; set; }

        public int Index { get; set; }

        public ITypePattern AttributeType { get; set; }

        public virtual bool IsMatch(AttributeSignatures signature)
        {
            var matched = false;

            if ((Position & AttributePositioon.Type) != 0 && !matched)
            {
                matched = signature.Types.Any(x => AttributeType.IsMatch(x));
            }
            if ((Position & AttributePositioon.Execution) != 0 && !matched)
            {
                matched = signature.Executions.Any(x => AttributeType.IsMatch(x));
            }
            if ((Position & AttributePositioon.Parameter) != 0 && !matched)
            {
                if (Index == -1)
                {
                    matched = signature.Parameters.Any(x => x.Any(y => AttributeType.IsMatch(y)));
                }
                else
                {
                    matched = Index < signature.Parameters.Length && signature.Parameters[Index].Any(x => AttributeType.IsMatch(x));
                }
            }
            if ((Position & AttributePositioon.Return) != 0 && !matched)
            {
                matched = signature.Returns.Any(x => AttributeType.IsMatch(x));
            }

            return matched;
        }
    }

    [Flags]
    public enum AttributePositioon
    {
        Type = 0x1,
        Execution = 0x2,
        Parameter = 0x4,
        Return = 0x8,
        Any = Type | Execution | Parameter | Return
    }
}
