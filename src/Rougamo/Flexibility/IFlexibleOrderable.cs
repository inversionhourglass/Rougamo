namespace Rougamo.Flexibility
{
    /// <summary>
    /// When an attribute implements this interface, it allows setting the execution order when applied to the method.
    /// </summary>
    public interface IFlexibleOrderable
    {
        /// <summary>
        /// Execution order.
        /// </summary>
        double Order { get; set; }
    }
}
