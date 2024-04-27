namespace Rougamo.Fody.Enhances.AsyncIterator
{
    internal class StrictAiteratorContext
    {
        public StrictAiteratorContext(AiteratorFields fields, StrictAiteratorAnchors anchors)
        {
            Fields = fields;
            Anchors = anchors;
        }

        public AiteratorFields Fields { get; }

        public StrictAiteratorAnchors Anchors { get; }
    }
}
