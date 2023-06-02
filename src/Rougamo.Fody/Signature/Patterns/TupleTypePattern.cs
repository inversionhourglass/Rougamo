using System.Linq;

namespace Rougamo.Fody.Signature.Patterns
{
    public class TupleTypePattern : TypePattern
    {
        public TupleTypePattern(TypePattern[] items)
        {
            Items = items;
        }

        public TypePattern[] Items { get; }

        public override GenericNamePattern ExtractNamePattern()
        {
            return Items.Last().ExtractNamePattern();
        }

        public override bool IsMatch(TypeSignature signature)
        {
            if (signature.Name != "System.Tuple" && signature.Name != "System.ValueTuple") return false;
            if (signature is not GenericTypeSignature genericSignature) return false;
            if (Items.Length != genericSignature.GenericParameters.Length) return false;

            for(var i = 0; i < Items.Length; i++)
            {
                if (!Items[i].IsMatch(genericSignature.GenericParameters[i])) return false;
            }

            return true;
        }
    }
}
