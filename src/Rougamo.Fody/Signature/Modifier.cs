using System;

namespace Rougamo.Fody.Signature
{
    [Flags]
    public enum Modifier
    {
        WithinClass = 0x1,
        DerivedClassSameAssembly = 0x2,
        NonDerivedClassSameAssembly = 0x4,
        DerivedClassDifferentAssembly = 0x8,
        NonDerivedClassDifferentAssembly = 0x10,

        Static = 0x20,

        Private = WithinClass,
        PrivateProtected = WithinClass | DerivedClassSameAssembly,
        Internal = WithinClass | DerivedClassSameAssembly | NonDerivedClassSameAssembly,
        Protected = WithinClass | DerivedClassSameAssembly | DerivedClassDifferentAssembly,
        ProtectedInternal = WithinClass | DerivedClassSameAssembly | NonDerivedClassSameAssembly | DerivedClassDifferentAssembly,
        Public = WithinClass | DerivedClassSameAssembly | NonDerivedClassSameAssembly | DerivedClassDifferentAssembly | NonDerivedClassDifferentAssembly,

        PrivateStatic = Private | Static,
        PrivateProtectedStatic = PrivateProtected | Static,
        InternalStatic = Internal | Static,
        ProtectedStatic = Protected | Static,
        ProtectedInternalStatic = ProtectedInternal | Static,
        PublicStatic = Public | Static,
    }

    public static class ModifierExtensions
    {
        public static string ToDefinitionString(this Modifier modifier)
        {
            var str = (modifier & Modifier.Public) switch
            {
                Modifier.Public => "public",
                Modifier.ProtectedInternal => "protectedinternal",
                Modifier.Protected => "protected",
                Modifier.Internal => "internal",
                Modifier.PrivateProtected => "privateprotected",
                Modifier.Private => "private",
                _ => throw new ArgumentException($"Unknow modifier value ({modifier})")
            };

            return (modifier & Modifier.Static) == 0 ? str : str + " static";
        }
    }
}
