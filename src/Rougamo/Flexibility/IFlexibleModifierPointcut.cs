namespace Rougamo.Flexibility
{
    /// <summary>
    /// When an attribute implements this interface, it allows setting the modifier pointcut when applied to the method.
    /// </summary>
    public interface IFlexibleModifierPointcut
    {
        /// <summary>
        /// Weavable method type, this attribute has no effect when applied at the method level.
        /// </summary>
        AccessFlags Flags { get; set; }
    }
}
