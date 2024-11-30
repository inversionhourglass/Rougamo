#if FODY
namespace Rougamo.Fody
#else
namespace Rougamo
#endif
{
    /// <summary>
    /// The lifetime of Mo.
    /// </summary>
#if FODY
    internal enum Lifetime
#else
    public enum Lifetime
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
