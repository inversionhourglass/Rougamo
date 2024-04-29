namespace Rougamo.Fody.Enhances.AsyncIterator
{
    internal class ProxyAiteratorContext
    {
        public ProxyAiteratorContext(AiteratorFields fields, ProxyAiteratorAnchors anchors)
        {
            Fields = fields;
            Anchors = anchors;
        }

        public AiteratorFields Fields { get; }

        public ProxyAiteratorAnchors Anchors { get; }
    }
}
