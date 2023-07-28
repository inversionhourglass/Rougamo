using Mono.Cecil;
using Xunit;

namespace Rougamo.Fody.Tests
{

    [Collection("SignatureUsage")]
    public class SetterMatchTests : PropertyMatchTests
    {
        protected override string PatternType => "setter";

        protected override bool Filter((MethodDefinition method, PropertyDefinition property) def)
        {
            return def.property != null && def.method == def.property.SetMethod;
        }
    }
}
