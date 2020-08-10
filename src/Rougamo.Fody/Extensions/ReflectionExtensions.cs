using Mono.Cecil;
using System.Reflection;

namespace Rougamo.Fody
{
    internal static class ReflectionExtensions
    {
        #region Import

        public static MethodReference ImportInto(this MethodBase methodInfo, ModuleDefinition moduleDef) => moduleDef.ImportReference(methodInfo);

        #endregion Import
    }
}
