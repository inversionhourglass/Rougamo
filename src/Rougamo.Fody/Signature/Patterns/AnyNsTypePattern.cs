namespace Rougamo.Fody.Signature.Patterns
{
    public class AnyNsTypePattern : TypePattern
    {
        private readonly string _name;

        public AnyNsTypePattern(string name)
        {
            _name = name;
        }

        public override GenericNamePattern ExtractNamePattern()
        {
            throw new System.NotImplementedException();
        }

        public override bool IsMatch(TypeSignature signature)
        {
            return PrimitiveCheck(signature) || signature.Name == _name;
        }

        private bool PrimitiveCheck(TypeSignature signature)
        {
            var name = _name switch
            {
                "bool" => typeof(bool).FullName,
                "byte" => typeof(byte).FullName,
                "sbyte" => typeof(sbyte).FullName,
                "char" => typeof(char).FullName,
                "decimal" => typeof(decimal).FullName,
                "double" => typeof(double).FullName,
                "float" => typeof(float).FullName,
                "int" => typeof(int).FullName,
                "uint" => typeof(uint).FullName,
                "long" => typeof(long).FullName,
                "ulong" => typeof(ulong).FullName,
                "object" => typeof(object).FullName,
                "short" => typeof(short).FullName,
                "ushort" => typeof(ushort).FullName,
                "string" => typeof(string).FullName,
                "void" => typeof(void).FullName,
                _ => null
            };

            return name == signature.ToString();
        }
    }
}
