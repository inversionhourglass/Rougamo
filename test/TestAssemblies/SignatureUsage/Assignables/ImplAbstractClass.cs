using System.Numerics;

namespace SignatureUsage.Assignables
{
    public class ImplAbstractClass : AbstractClass
    {
        public Interface DefaultInterface => default;

        public BaseClass DefaultBaseClass { get; set; }

        public ImplInterfaceAbstractClass DefaultImplInterfaceAbstractClass => default;

        public BigInteger ImplAbstract() => default;

        public static BaseClass GetBase() => default;

        public Interface GetInterface() => default;
    }
}
