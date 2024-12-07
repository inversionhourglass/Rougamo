using Rougamo;
using Rougamo.Metadatas;

namespace AnalyzerTestAssembly.WithLifetime
{
    [Lifetime(Lifetime.Transient)]
    internal class TransientAttribute(string value) : MoAttribute
    {
        public string Value => value;

        public int SettableValue { get; set; }
    }
}
