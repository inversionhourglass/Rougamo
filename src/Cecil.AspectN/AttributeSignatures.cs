namespace Cecil.AspectN
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

        public TypeSignature[] Types { get; }

        public TypeSignature[] Executions { get; }

        public TypeSignature[][] Parameters { get; }

        public TypeSignature[] Returns { get; }
    }
}
