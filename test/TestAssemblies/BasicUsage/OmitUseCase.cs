using BasicUsage.Mos;
using Rougamo;
using System.Collections.Generic;

namespace BasicUsage
{
    [Rougamo<ValueOmitAll>]
    public class OmitUseCase
    {
        [Rougamo(typeof(ValueOmitMos))]
        public void Mos(List<string> executedMos)
        {

        }

        [Rougamo<ValueOmitArguments>]
        public List<string> Arguments(int x) => null;

        [Rougamo(typeof(ValueOmitArgumentsButFeature))]
        public List<string> ArgumentsFailed(int x) => null;

        [Rougamo(typeof(ValueOmitAll))]
        public static List<string> All(int x) => null;
    }
}
