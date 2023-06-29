using Mono.Cecil;
using System.Linq;
using Xunit;

namespace Rougamo.Fody.Tests
{
    [Collection("SignatureUsage")]
    public class MethodMatchTests : ExecutionMatchTests
    {
        protected override string PatternType => "method";

        protected override bool Filter(MethodDefinition methodDef)
        {
            return methodDef.DeclaringType == null || !methodDef.DeclaringType.Properties.Any(x => x.GetMethod == methodDef || x.SetMethod == methodDef);
        }
    }
}
