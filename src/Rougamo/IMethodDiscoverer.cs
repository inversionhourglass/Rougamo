using System.Reflection;

namespace Rougamo
{
    public interface IMethodDiscoverer
    {
        bool IsMatch(MethodInfo method);
    }
}
