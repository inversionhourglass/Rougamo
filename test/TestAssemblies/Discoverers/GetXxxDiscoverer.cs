using Rougamo;
using System.Reflection;

namespace Discoverers
{
    public class GetXxxDiscoverer : IMethodDiscoverer
    {
        public bool IsMatch(MethodInfo method)
        {
            return method.Name.StartsWith("Get");
        }
    }
}
