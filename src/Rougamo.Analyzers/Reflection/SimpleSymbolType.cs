using Microsoft.CodeAnalysis;

namespace Rougamo.Analyzers.Reflection
{
    internal readonly struct SimpleSymbolType
    {
        private readonly string? _fullName;
        private readonly SpecialType _specialType;

        public SimpleSymbolType(string fullName)
        {
            _fullName = fullName;
            _specialType = SpecialType.None;
        }

        public SimpleSymbolType(SpecialType specialType)
        {
            _fullName = null;
            _specialType = specialType;
        }

        public bool IsEqual(ITypeSymbol typeSymbol)
        {
            if (_specialType != SpecialType.None)
            {
                return typeSymbol.SpecialType == _specialType;
            }

            return typeSymbol.ToString() == _fullName;
        }

        public static implicit operator SimpleSymbolType(string fullName)
        {
            return new SimpleSymbolType(fullName);
        }

        public static implicit operator SimpleSymbolType(SpecialType specialType)
        {
            return new SimpleSymbolType(specialType);
        }
    }
}
