namespace Rougamo.Fody.Signature
{
    public class AttributeSignatures
    {
        public AttributeSignatures(TypeSignature[] types, TypeSignature[] executions, TypeSignature[][] parameters, TypeSignature[] returns)
        {
            Types = types;
            Executions = executions;
            Parameters = parameters;
            Returns = returns;
        }

        public TypeSignature[] Types { get; set; }

        public TypeSignature[] Executions { get; set; }

        public TypeSignature[][] Parameters { get; set; }

        public TypeSignature[] Returns { get; set; }
    }
}
