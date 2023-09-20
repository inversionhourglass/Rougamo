using PatternUsage.Attributes.Executions;
using PatternUsage.Attributes.Methods;
using Rougamo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PatternUsage
{
    [InstaceSuffix]
    [DoubleAnyParameter]
    [IntAtTheLastOfThreeParameter]
    [AnyTwoItemTupleParameter]
    [SpecificTupleReturn]
    public class Public : NonPublicCaller, Interface, IRougamo<DoubleAtTheSecondOfThreeParameterAttribute>
    {
        public void Instance(List<string> executedMos)
        {
        }

        private string? PrivateInstance(List<string> executedMos) => default;

        protected int ProtectedInstance(List<string> executedMos) => default;

        internal double InternalInstanceXyz(List<string> executedMos) => default;

        private protected Task P2InstanceAsync(List<string> executedMos) => Task.CompletedTask;

        protected internal async ValueTask<int> PiInstanceAsync(List<string> executedMos)
        {
            await Task.Yield();
            return 1;
        }

        public static async Task StaticAsync(List<string> executedMos)
        {
            await Task.Yield();
        }

        private static async ValueTask PrivateStaticAsync(List<string> executedMos)
        {
            await Task.Yield();
        }

        public void P1_1(List<string> executedMos) { }

        public void P2_1(List<string> executedMos, int x) { }

        public void P2_2(List<string> executedMos, double y) { }

        public void P3_1(List<string> executedMos, int x, int y) { }

        public void P3_2(List<string> executedMos, double x, double y) { }

        public void P3_3(List<string> executedMos, double a, int b) { }

        public (int, string) TupleOut(List<string> executedMos) => (1, "2");

        public List<string>? TupleIn(Tuple<DateTime, DateTimeOffset> tuple) => default;
    }
}
