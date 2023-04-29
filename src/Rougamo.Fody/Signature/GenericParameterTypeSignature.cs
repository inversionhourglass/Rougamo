namespace Rougamo.Fody.Signature
{
    public class GenericParameterTypeSignature : TypeSignature
    {
        public GenericParameterTypeSignature(string name) : base(name, null, TypeCategory.GenericParameter)
        {
        }

        public string SortName { get; set; }

        protected override string FormatName => SortName ?? Name;
    }
}
