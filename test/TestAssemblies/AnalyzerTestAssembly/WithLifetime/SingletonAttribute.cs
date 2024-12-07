using Rougamo;
using Rougamo.Metadatas;

namespace AnalyzerTestAssembly.WithLifetime
{
    [Lifetime(Lifetime.Singleton)]
    internal class SingletonAttribute(string value) : MoAttribute
    {
        public string Value => value;

        public int SettableValue { get; set; }
    }
}
