namespace Rougamo.Fody.Enhances.Iterator
{
    internal class ProxyIteratorContext
    {
        public ProxyIteratorContext(IteratorFields fields, ProxyIteratorAnchors anchors)
        {
            Fields = fields;
            Anchors = anchors;
        }

        public IteratorFields Fields { get; }

        public ProxyIteratorAnchors Anchors { get; }
    }
}
