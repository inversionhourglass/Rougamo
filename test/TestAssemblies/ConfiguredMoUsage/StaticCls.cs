using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConfiguredMoUsage
{
    public static class StaticCls
    {
        private static readonly decimal _Y;

        public static void SyncStatic(List<string> executedMos) { }

        public static double SyncStaticReturnDouble(List<string> executedMos) => 1.0;

        public static async Task AsyncStaticTask(List<string> executedMos) => await Task.Yield();

        public static Task<DateTime> AsyncStaticTaskReturnDateTime(List<string> executedMos) => Task.FromResult(DateTime.Now);

        public static async ValueTask AsyncStaticValueTask(List<string> executedMos) => await Task.Yield();

        public static async ValueTask<Guid> AsyncStaticValueTaskReturnGuid(List<string> executedMos)
        {
            await Task.Yield();
            return Guid.NewGuid();
        }

        public static decimal GetY(List<string> executedMos) => _Y;
    }
}
