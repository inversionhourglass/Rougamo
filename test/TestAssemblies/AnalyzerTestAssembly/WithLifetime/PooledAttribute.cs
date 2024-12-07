using Rougamo;
using Rougamo.Metadatas;

namespace AnalyzerTestAssembly.WithLifetime
{
    [Lifetime(Lifetime.Pooled)]
    internal class PooledAttribute(string value) : MoAttribute
    {
        public string Value => value;

        public int SettableValue { get; set; }
    }
}
