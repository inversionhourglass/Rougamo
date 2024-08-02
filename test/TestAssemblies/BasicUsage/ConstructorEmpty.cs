using BasicUsage.Mos;
using Rougamo;
using System.Collections.Generic;

namespace BasicUsage
{
    [Rougamo<CtorValueMo>]
    public class ConstructorEmpty
    {
        private readonly static List<string> _ExecutedMos = new();

        public ConstructorEmpty(List<string> executedMos) { }

        static ConstructorEmpty()
        {

        }

        public static List<string> GetExecutedMos() => _ExecutedMos;

        public static void AddExecutedMos(string mo) => _ExecutedMos.Add(mo);
    }
}
