namespace Rougamo.Fody
{
    /// <summary>
    /// 织入来源
    /// </summary>
    internal enum MoFrom
    {
        /// <summary>
        /// method级别的Attribute，包含通过代理织入的Attribute
        /// </summary>
        Method = 1,
        /// <summary>
        /// class级别的Attribute，包含通过代理织入的Attribute
        /// </summary>
        Class = 2,
        /// <summary>
        /// assembly以及module级别的Attribute
        /// </summary>
        Assembly = 3
    }
}
