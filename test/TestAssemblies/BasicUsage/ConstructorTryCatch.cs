using BasicUsage.Mos;
using Rougamo;
using System;
using System.Collections.Generic;

namespace BasicUsage
{
    [Rougamo<CtorValueMo>]
    public class ConstructorTryCatch
    {
        private readonly int _seed;
        private static readonly int _Seed;
        private readonly static List<string> _ExecutedMos = new();

        public ConstructorTryCatch(List<string> executedMos)
        {
            var random = new Random();
            var r1 = random.Next() + executedMos.Count;
            if (r1 % 32123 == 12321)
            {
                var r3 = random.Next();
                if (r3 % 11 == 33) goto TRY;
                _seed = r3 + 12345;
                return;
            }
        TRY:
            try
            {
                if (r1 == int.MaxValue)
                {
                    var r2 = random.Next() % 123;
                    if (r2 == 321)
                    {
                        if (r1 + r2 == int.MinValue) throw new InvalidOperationException();
                        _seed = r2 - r1;
                        return;
                    }
                    _seed = r1 * r2;
                }
            }
            catch (Exception e)
            {
                _seed = e.Message.Length;
            }
        }

        static ConstructorTryCatch()
        {
            var random = new Random();
            var r1 = random.Next();
            if (r1 % 32123 == 12321)
            {
                var r3 = random.Next();
                if (r3 % 11 == 33) goto TRY;
                _Seed = r3 + 12345;
                return;
            }
        TRY:
            try
            {
                if (r1 == int.MaxValue)
                {
                    var r2 = random.Next() % 123;
                    if (r2 == 321)
                    {
                        if (r1 + r2 == int.MinValue) throw new InvalidOperationException();
                        _Seed = r2 - r1;
                        return;
                    }
                    _Seed = r1 * r2;
                }
            }
            catch (Exception e)
            {
                _Seed = e.Message.Length;
            }
        }

        public static List<string> GetExecutedMos() => _ExecutedMos;

        public static void AddExecutedMos(string mo) => _ExecutedMos.Add(mo);
    }
}
