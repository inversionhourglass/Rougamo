using Rougamo;
using System;

namespace BasicUsage.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class DiscoveryAttribute : MoAttribute
    {

    }
}
