namespace Rougamo.Fody.Enhances.Iterator
{
    internal class StrictIteratorContext
    {
        public StrictIteratorContext(IteratorFields fields, StrictIteratorAnchor anchors)
        {
            Fields = fields;
            Anchors = anchors;
        }

        public IteratorFields Fields { get; }

        public StrictIteratorAnchor Anchors { get; }
    }
}
