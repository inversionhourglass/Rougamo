namespace SignatureUsage.Assignables
{
    public class ImplInterfaceBaseClass : BaseClass, Interface
    {
        public (string, int) ImplInterfaceBase() => default;

        public override Interface GetInterface() => default;

        public AbstractClass GetAbstract() => default;
    }
}
