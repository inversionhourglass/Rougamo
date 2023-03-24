using Mono.Cecil;

namespace Rougamo.Fody.Enhances
{
    internal class BoxTypeReference
    {
        private readonly TypeReference _typeReference;
        private readonly bool _needBox;

        public BoxTypeReference(TypeReference typeReference)
        {
            _typeReference = typeReference;
            _needBox = typeReference.IsUnboxable();
        }

        public static implicit operator bool(BoxTypeReference boxTypeReference)
        {
            return boxTypeReference._needBox;
        }

        public static implicit operator TypeReference(BoxTypeReference boxTypeReference)
        {
            return boxTypeReference._typeReference;
        }
    }
}
