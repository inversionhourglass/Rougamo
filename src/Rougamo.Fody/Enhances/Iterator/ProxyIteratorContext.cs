namespace Rougamo.Fody.Enhances.Iterator
{
    internal class StrictIteratorContext
    {
        public StrictIteratorContext(IteratorFields fields, StrictIteratorAnchors anchors)
        {
            Fields = fields;
            Anchors = anchors;
        }

        public IteratorFields Fields { get; }

        public StrictIteratorAnchors Anchors { get; }
    }
}
