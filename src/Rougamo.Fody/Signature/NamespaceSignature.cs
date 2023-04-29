namespace Rougamo.Fody.Signature
{
    public class NamespaceSignature : SimpleTypeSignature
    {
        public NamespaceSignature(string @namespace) : base(@namespace, null)
        {
        }

        protected override char Separator => '.';
    }
}
