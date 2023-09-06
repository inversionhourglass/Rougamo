using Mono.Cecil;
using Rougamo.Fody;
using System.Linq;
using Xunit;

namespace Rougamo.Fody.Tests.Patterns
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
