using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody.Signature.Patterns
{
    public class TupleTypePattern : IIntermediateTypePattern
    {
        public TupleTypePattern(IIntermediateTypePattern[] items)
        {
            Items = items;
        }

        public IIntermediateTypePattern[] Items { get; }

        public bool AssignableMatch => Items.Any(x => x.AssignableMatch);

        public GenericNamePattern ExtractNamePattern()
        {
            return Items.Last().ExtractNamePattern();
        }

        public void Compile(List<GenericParameterTypePattern> genericParameters, bool genericIn)
        {
            foreach (var item in Items)
            {
                item.Compile(genericParameters, genericIn);
            }
        }

        public bool IsMatch(TypeSignature signature)
        {
            if (signature.NestedTypes.Length != 1) return false;

            var genericSignature = signature.NestedTypes[0];
            if (genericSignature.Generics.Length != Items.Length) return false;

            if (signature.Namespace != "System" || genericSignature.Name != "Tuple" && genericSignature.Name != "ValueTuple") return false;

            for(var i = 0; i < Items.Length; i++)
            {
                if (!Items[i].IsMatch(genericSignature.Generics[i])) return false;
            }

            return true;
        }
    }
}
