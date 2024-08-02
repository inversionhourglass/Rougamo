using BasicUsage.Mos;
using Rougamo;
using System;
using System.Collections.Generic;

namespace BasicUsage
{
    [Rougamo<CtorValueMo>]
    public class ConstructorThrows
    {
        private readonly static List<string> _ExecutedMos = new();

        public ConstructorThrows(List<string> executedMos)
        {
            if (executedMos.Count > 0) throw new ArgumentException();
        }

        static ConstructorThrows()
        {

        }

        public static List<string> GetExecutedMos() => _ExecutedMos;

        public static void AddExecutedMos(string mo) => _ExecutedMos.Add(mo);
    }
}
