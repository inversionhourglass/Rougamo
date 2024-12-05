using Microsoft.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Rougamo.Analyzers
{
    internal static class R
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LocalizableString S(string name) => new LocalizableResourceString(name, Resources.ResourceManager, typeof(Resources));
    }
}
