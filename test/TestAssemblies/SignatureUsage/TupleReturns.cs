using System;
using System.Numerics;

namespace SignatureUsage
{
    internal class TupleReturns
    {
        public (BigInteger, decimal) PropTuple { get; set; }

        public (int, string) Tuple1() => default;

        public Tuple<double, decimal> Tuple2() => default;

        public ValueTuple<char, string> Tuple3() => default;

        public string Tuple4((int?, double?) value) => value.ToString();

        public string Tuple5(Tuple<BigInteger, object> obj) => obj.ToString();

        public string Tuple<T1, T2>(ValueTuple<T1, T2> item) => item.ToString();
    }
}
