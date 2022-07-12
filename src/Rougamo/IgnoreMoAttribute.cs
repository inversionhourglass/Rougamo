using System;

namespace Rougamo
{
    /// <summary>
    /// 被标记的类型或方法将忽略代码织入
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly|AttributeTargets.Module|AttributeTargets.Class|AttributeTargets.Method, AllowMultiple = true)]
    public sealed class IgnoreMoAttribute : Attribute
    {
        /// <summary>
        /// 忽略指定实现<see cref="IMo"/>接口的织入类型，不传入忽略所有
        /// </summary>
        public Type[]? MoTypes { get; set; }
    }
}
