using Rougamo;
using System.Reflection;

namespace Discoverers
{
    public class PublicDiscoverer : IMethodDiscoverer
    {
        public bool IsMatch(MethodInfo method)
        {
            return method.IsPublic;
        }
    }
}