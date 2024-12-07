namespace Rougamo
{
    /// <summary>
    /// The lifetime of Mo.
    /// </summary>
#if ROUGAMO
    public enum Lifetime
#else
    internal enum Lifetime
#endif
    {
        /// <summary>
        /// Singleton
        /// </summary>
        Singleton,
        /// <summary>
        /// Pooled
        /// </summary>
        Pooled,
        /// <summary>
        /// Transient
        /// </summary>
        Transient
    }
}
