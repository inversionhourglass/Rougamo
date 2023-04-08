using Rougamo;
using System.Reflection;

namespace Discoverers
{
    public class EnumGetterDiscoverer : IMethodDiscoverer
    {
        public bool IsMatch(MethodInfo method)
        {
            return method.IsPropertyGetter() && method.ReturnType.IsEnum;
        }
    }
}
