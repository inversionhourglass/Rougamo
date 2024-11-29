namespace Rougamo
{
    /// <summary>
    /// Represents an object that can be reset to its initial state.
    /// </summary>
    public interface IResettable
    {
        /// <summary>
        /// Resets the object to its initial state.
        /// </summary>
        bool TryReset();
    }
}
