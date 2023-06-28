using System;

namespace SignatureUsage
{
    public class NullableCls
    {
        private int? Int32 { get; set; }

        public Nullable<Xyz> Get() => default;

        public string Set(Xyz? xyz, int seed) => $"{xyz}.{seed}";

        public double GetOrDefault((double?, double) value) => value.Item1 ?? value.Item2;
    }

    public struct Xyz
    {
        public short X { get; set; }

        public int Y { get; set; }

        public long Z { get; set; }
    }
}
