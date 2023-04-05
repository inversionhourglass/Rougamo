using BasicUsage.Attributes;
using Discoverers;
using Rougamo;
using System.Reflection;
using System.Threading.Tasks;

namespace BasicUsage
{
    [Discovery(DiscovererType = typeof(PublicDiscoverer))]
    [Discovery(DiscovererType = typeof(PrivateDiscoverer))]
    [MethodDiscovery]
    public class PublicDiscoveryUseCase
    {
        private string Value { get; set; }

        public int Flag { get; set; }

        public void PublicInstance()
        {

        }

        public static async Task PublicStaticAsync()
        {

        }

        private async ValueTask PrivateInstanceAsync()
        {

        }

        private static void PrivateStatic()
        {

        }
    }

    public class PrivateDiscoverer : IMethodDiscoverer
    {
        public bool IsMatch(MethodInfo method)
        {
            return method.IsPrivate;
        }
    }
}
