namespace Cecil.AspectN
{
    public struct CollectionCount
    {
        private const int _ANY = -1;
        private const int _ONE_PLUS = -2;

        public static readonly CollectionCount ANY = new(_ANY);
        public static readonly CollectionCount ZERO = new(0);
        public static readonly CollectionCount Single = new(1);
        public static readonly CollectionCount OnePlus = new(_ONE_PLUS);

        public CollectionCount(int count)
        {
            Value = count;
        }

        public int Value { get; }

        public static implicit operator CollectionCount(int count) => new(count);

        public static bool operator ==(CollectionCount count1, CollectionCount count2)
        {
            if (count1.Value == _ANY || count2.Value == _ANY) return true;
            if (count1.Value == _ONE_PLUS && count2.Value >= 1 || count2.Value == _ONE_PLUS && count1.Value >= 1) return true;
            return count1.Value == count2.Value;
        }

        public static bool operator !=(CollectionCount count1, CollectionCount count2)
        {
            if (count1.Value == _ANY || count2.Value == _ANY) return false;
            if (count1.Value == _ONE_PLUS && count2.Value >= 1 || count2.Value == _ONE_PLUS && count1.Value >= 1) return false;
            return count1.Value != count2.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is not CollectionCount cc) return false;
            return this == cc;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
