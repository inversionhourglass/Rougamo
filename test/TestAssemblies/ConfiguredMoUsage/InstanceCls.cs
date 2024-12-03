using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace ConfiguredMoUsage
{
    public class InstanceCls
    {
        private readonly BigInteger _x;

        private static readonly decimal _Y;

        public void Sync(List<string> executedMos) { }

        public int SyncReturnInt32(List<string> executedMos) => 1;

        public async Task AsyncTask(List<string> executedMos) => await Task.Yield();

        public async Task<string> AsyncTaskReturnString(List<string> executedMos)
        {
            await Task.Yield();
            return nameof(AsyncTaskReturnString);
        }

        public async ValueTask AsyncValueTask(List<string> executedMos) => await Task.Yield();

        public ValueTask<object> AsyncValueTaskReturnObject(List<string> executedMos) => new(new object());

        public BigInteger GetX(List<string> executedMos) => _x;

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
