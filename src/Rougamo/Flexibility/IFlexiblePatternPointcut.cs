namespace Rougamo.Flexibility
{
    /// <summary>
    /// When an attribute implements this interface, it allows setting the pattern pointcut when applied to the method.
    /// </summary>
    public interface IFlexiblePatternPointcut
    {
        /// <summary>
        /// Method matched with this pattern will be weaving. This attribute has no effect when applied at the method level. It has a higher priority than <see cref="Flags"/>
        /// </summary>
        /// <remarks>
        /// https://github.com/inversionhourglass/Shared.Cecil.AspectN
        /// </remarks>
        string? Pattern { get; }
    }
}
