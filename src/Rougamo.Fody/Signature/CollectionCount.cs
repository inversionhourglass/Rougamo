namespace Rougamo.Fody.Signature
{
    public struct CollectionCount
    {
        private const int _ANY = -1;

        public static readonly CollectionCount ANY = new CollectionCount(_ANY);
        public static readonly CollectionCount ZERO = new CollectionCount(0);
        public static readonly CollectionCount Single = new CollectionCount(1);

        public CollectionCount(int count)
        {
            Value = count;
        }

        public int Value { get; }

        public static implicit operator CollectionCount(int count) => new CollectionCount(count);

        public static bool operator ==(CollectionCount count1, CollectionCount count2)
        {
            return count1.Value == count2.Value || count1.Value == _ANY || count2.Value == _ANY;
        }

        public static bool operator !=(CollectionCount count1, CollectionCount count2)
        {
            return count1.Value != count2.Value && count1.Value != _ANY && count2.Value != _ANY;
        }
    }
}
