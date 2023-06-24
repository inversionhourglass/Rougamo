namespace SignatureUsage.Assignables
{
    public class ImplInterfaceAbstractClass : AbstractClass, Interface
    {
        public sbyte ImplInterfaceAbstract() => default;

        public static AbstractClass GetAbstract() => default;

        public Interface GetInterface() => default;
    }
}
