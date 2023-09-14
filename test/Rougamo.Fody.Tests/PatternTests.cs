using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using PatternUsage.Attributes.Methods;

namespace Rougamo.Fody.Tests
{
    [Collection("PatternUsage")]
    public class PatternTests : TestBase
    {
        public PatternTests() : base("PatternUsage.dll")
        {
        }

        protected override string RootNamespace => "PatternUsage";

        [Fact]
        public async Task MethodGlobalModifierTest()
        {
            var instance = GetInstance("Public");
            var executedMos = new List<string>();

            executedMos.Clear();
            instance.Instance(executedMos);
            Assert.DoesNotContain(nameof(PublicStaticAttribute), executedMos);

            executedMos.Clear();
            instance.Call("PrivateInstance", executedMos);
            Assert.DoesNotContain(nameof(PublicStaticAttribute), executedMos);

            executedMos.Clear();
            instance.Call("ProtectedInstance", executedMos);
            Assert.DoesNotContain(nameof(PublicStaticAttribute), executedMos);

            executedMos.Clear();
            instance.Call("InternalInstanceXyz", executedMos);
            Assert.DoesNotContain(nameof(PublicStaticAttribute), executedMos);

            executedMos.Clear();
            await (Task)instance.Call("P2InstanceAsync", executedMos);
            Assert.DoesNotContain(nameof(PublicStaticAttribute), executedMos);

            executedMos.Clear();
            await (ValueTask<int>)instance.Call("PiInstanceAsync", executedMos);
            Assert.DoesNotContain(nameof(PublicStaticAttribute), executedMos);

            executedMos.Clear();
            await (Task)instance.Call("StaticAsync", executedMos);
            Assert.Contains(nameof(PublicStaticAttribute), executedMos);

            executedMos.Clear();
            await (ValueTask)instance.Call("PrivateStaticAsync", executedMos);
            Assert.DoesNotContain(nameof(PublicStaticAttribute), executedMos);
        }

        [Fact]
        public async Task MethodIRougamoTest()
        {
            var instance = GetInstance("Public");
            var executedMos = new List<string>();

            executedMos.Clear();
            instance.Instance(executedMos);
            Assert.DoesNotContain(nameof(StaticAttribute), executedMos);

            executedMos.Clear();
            instance.Call("PrivateInstance", executedMos);
            Assert.DoesNotContain(nameof(StaticAttribute), executedMos);

            executedMos.Clear();
            instance.Call("ProtectedInstance", executedMos);
            Assert.DoesNotContain(nameof(StaticAttribute), executedMos);

            executedMos.Clear();
            instance.Call("InternalInstanceXyz", executedMos);
            Assert.DoesNotContain(nameof(StaticAttribute), executedMos);

            executedMos.Clear();
            await (Task)instance.Call("P2InstanceAsync", executedMos);
            Assert.DoesNotContain(nameof(StaticAttribute), executedMos);

            executedMos.Clear();
            await (ValueTask<int>)instance.Call("PiInstanceAsync", executedMos);
            Assert.DoesNotContain(nameof(StaticAttribute), executedMos);

            executedMos.Clear();
            await (Task)instance.Call("StaticAsync", executedMos);
            Assert.Contains(nameof(StaticAttribute), executedMos);

            executedMos.Clear();
            await (ValueTask)instance.Call("PrivateStaticAsync", executedMos);
            Assert.Contains(nameof(StaticAttribute), executedMos);
        }

        [Fact]
        public async Task MethodFuzzyNameTest()
        {
            var instance = GetInstance("Public");
            var executedMos = new List<string>();

            executedMos.Clear();
            instance.Instance(executedMos);
            Assert.Contains(nameof(InstaceSuffixAttribute), executedMos);

            executedMos.Clear();
            instance.Call("PrivateInstance", executedMos);
            Assert.Contains(nameof(InstaceSuffixAttribute), executedMos);

            executedMos.Clear();
            instance.Call("ProtectedInstance", executedMos);
            Assert.Contains(nameof(InstaceSuffixAttribute), executedMos);

            executedMos.Clear();
            instance.Call("InternalInstanceXyz", executedMos);
            Assert.DoesNotContain(nameof(InstaceSuffixAttribute), executedMos);

            executedMos.Clear();
            await (Task)instance.Call("P2InstanceAsync", executedMos);
            Assert.DoesNotContain(nameof(InstaceSuffixAttribute), executedMos);

            executedMos.Clear();
            await (ValueTask<int>)instance.Call("PiInstanceAsync", executedMos);
            Assert.DoesNotContain(nameof(InstaceSuffixAttribute), executedMos);

            executedMos.Clear();
            await (Task)instance.Call("StaticAsync", executedMos);
            Assert.DoesNotContain(nameof(InstaceSuffixAttribute), executedMos);

            executedMos.Clear();
            await (ValueTask)instance.Call("PrivateStaticAsync", executedMos);
            Assert.DoesNotContain(nameof(InstaceSuffixAttribute), executedMos);
        }
    }
}
